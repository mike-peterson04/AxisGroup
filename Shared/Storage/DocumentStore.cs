// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace Shared
{
    public interface IDocumentStore<TAggregate> where TAggregate : DomainAggregate
    {
        IAsyncEnumerable<TAggregate> Get();
        Task<Result<TAggregate, string>> Get(int id);
        Task<Result<int, string>> Store(TAggregate aggregate);
    }

    /// <summary>
    /// A basic, proof-of-concept in-memory document store
    /// </summary>
    /// <typeparam name="TAggregate">Domain aggregate</typeparam>
    public abstract class DocumentStoreBase<TAggregate> : IDocumentStore<TAggregate> where TAggregate : DomainAggregate
    {
        private readonly InMemoryContext _context;
        private readonly string _table;

        protected DocumentStoreBase(InMemoryContext context, string serviceName)
        {
            _table = $"{serviceName}_{typeof(TAggregate).Name}_doc";
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private async Task CreateTable()
        {
            using DbCommand dbCommand = _context.Connection.CreateCommand();
            dbCommand.CommandText = $"CREATE TABLE IF NOT EXISTS {_table} (StreamId INTEGER NOT NULL PRIMARY KEY,  AggrObject TEXT)";
            await dbCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Commit an aggregate to the document store
        /// </summary>
        /// <param name="aggregate">A domain aggregate</param>
        /// <returns>Result with success or error</returns>
        public async Task<Result<int, string>> Store(TAggregate aggregate)
        {
            try
            {
                await CreateTable();
                using var transaction = await _context.Connection.BeginTransactionAsync();
                using var command = _context.Connection.CreateCommand();
                command.CommandText = @$"REPLACE INTO {_table} VALUES ($streamId, $aggrObject)";

                var streamParam = command.CreateParameter();
                streamParam.ParameterName = "$streamId";
                var aggrParam = command.CreateParameter();
                aggrParam.ParameterName = "$aggrObject";

                command.Parameters.AddRange(new[] { streamParam, aggrParam });
                streamParam.Value = aggregate.StreamId;
                aggrParam.Value = JsonConvert.SerializeObject(aggregate, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                int exec = await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                return Result.Success<int, string>(exec);
            }
            catch (Exception ex)
            {
                return Result.Failure<int, string>(ex.Message);
            }
        }

        /// <summary>
        /// Get all stored aggregates
        /// </summary>
        /// <returns>Async enumerable of aggregates</returns>
        public async IAsyncEnumerable<TAggregate> Get()
        {
            using var command = _context.Connection.CreateCommand();
            command.CommandText = @$"SELECT AggrObject FROM {_table}";
            var results = await command.ExecuteReaderAsync();
            while (results.Read())
            {
                yield return JsonConvert.DeserializeObject<TAggregate>(results[0].ToString(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            }
        }

        /// <summary>
        /// Get an aggregate by ID
        /// </summary>
        /// <param name="id">Stream ID of aggregate</param>
        /// <returns>Result with aggregate or error</returns>
        public async Task<Result<TAggregate, string>> Get(int id)
        {
            using var command = _context.Connection.CreateCommand();
            command.CommandText = @$"SELECT AggrObject FROM {_table} WHERE StreamId = $streamId";
            var streamParam = command.CreateParameter();
            streamParam.ParameterName = "$streamId";
            streamParam.Value = id;
            command.Parameters.Add(streamParam);
            try
            {
                var result = await command.ExecuteScalarAsync();
                if (result != null) return Result.Success<TAggregate, string>(JsonConvert.DeserializeObject<TAggregate>(result.ToString(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
            }
            catch { }
            return Result.Failure<TAggregate, string>($"ID {id} not found");
        }
    }
}
