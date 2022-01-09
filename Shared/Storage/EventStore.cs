// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace Shared
{
    public interface IEventStore<TDomainEvent> where TDomainEvent : DomainEvent
    {
        Task<Result<int, string>> Commit(IEnumerable<TDomainEvent> events);
        IAsyncEnumerable<TDomainEvent> GetDomainEvents(int streamId);
    }

    /// <summary>
    /// A basic, proof-of-concept in-memory event store
    /// </summary>
    /// <typeparam name="TAggregate">Domain event</typeparam>
    public abstract class EventStoreBase<TAggregate, TDomainEvent> : IEventStore<TDomainEvent> where TDomainEvent : DomainEvent
        where TAggregate : DomainAggregate
    {
        private readonly InMemoryContext _context;
        private readonly string _table;

        private readonly List<TDomainEvent> _domainEvents = new List<TDomainEvent>();

        protected EventStoreBase(InMemoryContext context, string serviceName)
        {
            _table = $"{serviceName}_{typeof(TAggregate).Name}_ev";
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private async Task CreateTable()
        {
            using DbCommand dbCommand = _context.Connection.CreateCommand();
            dbCommand.CommandText = $"CREATE TABLE IF NOT EXISTS {_table} (EventId INTEGER NOT NULL, StreamId INTEGER NOT NULL, EventObject TEXT, PRIMARY KEY (EventId, StreamId))";
            await dbCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Get domain events for a stream ID
        /// </summary>
        /// <param name="streamId">Stream ID</param>
        /// <returns>Async enumerable of domain events</returns>
        public async IAsyncEnumerable<TDomainEvent> GetDomainEvents(int streamId)
        {
            await CreateTable();
            using DbCommand command = _context.Connection.CreateCommand();
            var streamParam = command.CreateParameter();
            streamParam.ParameterName = "$streamId";
            streamParam.Value = streamId;
            command.CommandText = $"SELECT EventObject FROM {_table} WHERE StreamId = $streamId ORDER BY EventId ASC";
            command.Parameters.Add(streamParam);
            var results = await command.ExecuteReaderAsync();
            while (results.Read())
            {
                yield return JsonConvert.DeserializeObject<TDomainEvent>(results[0].ToString(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            }
        }

        /// <summary>
        /// Attempt to commit an insert. Will abort the transaction and throw a failure on event ID collisions to prevent concurrency issues.
        /// </summary>
        /// <returns>Result success, or error message</returns>
        public async Task<Result<int, string>> Commit(IEnumerable<TDomainEvent> events)
        {
            try
            {
                await CreateTable();
                using var transaction = await _context.Connection.BeginTransactionAsync();
                using var command = _context.Connection.CreateCommand();
                command.CommandText = @$"INSERT INTO {_table} VALUES ($eventId, $streamId, $eventObject)";

                var eventParam = command.CreateParameter();
                eventParam.ParameterName = "$eventId";
                var streamParam = command.CreateParameter();
                streamParam.ParameterName = "$streamId";
                var objParam = command.CreateParameter();
                objParam.ParameterName = "$eventObject";

                command.Parameters.AddRange(new[] { eventParam, streamParam, objParam });

                foreach (var de in events)
                {
                    eventParam.Value = de.EventId;
                    streamParam.Value = de.StreamId;
                    objParam.Value = JsonConvert.SerializeObject(de, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    await command.ExecuteNonQueryAsync();
                }
                await transaction.CommitAsync();
                _domainEvents.Clear();
                return Result.Success<int, string>(0);
            }
            catch (Exception ex)
            {
                return Result.Failure<int, string>(ex.Message);
            }
        }

    }
}
