// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Data.Common;
using System.Data.SQLite;
using Microsoft.Extensions.Logging;

namespace Shared
{
    /// <summary>
    /// Context class for maintaining an in-memory connection for the duration of the process execution
    /// </summary>
    public class InMemoryContext
    {

        public DbConnection Connection { get; init; }

        public InMemoryContext(ILogger<InMemoryContext> logger)
        {
            try
            {
                DbConnection dbConnection = SQLiteFactory.Instance.CreateConnection();
                dbConnection.ConnectionString = @"Data Source=:memory:;cache=shared";

                dbConnection.Open();
                Connection = dbConnection;
                logger.LogInformation($"SQLite in-memory context registered");
            }
            catch (Exception ex)
            {
                throw new Exception("Error opening database connection", ex);
            }
        }
    }
}
