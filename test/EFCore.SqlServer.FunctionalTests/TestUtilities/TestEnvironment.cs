// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class TestEnvironment
    {
        public static IConfiguration Config { get; }

        static TestEnvironment()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile("config.test.json", optional: true)
                .AddEnvironmentVariables();

            Config = configBuilder.Build()
                .GetSection("Test:SqlServer");
        }

        private const string DefaultConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=30";

        public static string DefaultConnection => Config["DefaultConnection"] ?? DefaultConnectionString;

        public static bool IsSqlAzure => new SqlConnectionStringBuilder(DefaultConnection).DataSource.Contains("database.windows.net");

        public static bool IsTeamCity => Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null;

        public static bool SupportsFullTextSearch
        {
            get
            {
                using (var sql = new SqlConnection(SqlServerTestStore.CreateConnectionString("master")))
                {
                    using (var command = new SqlCommand("SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') as result", sql))
                    {
                        sql.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();

                            var result = (int)reader["result"];

                            reader.Close();

                            sql.Close();

                            return result == 1;
                        }
                    }
                }
            }
        }

        public static string ElasticPoolName => Config["ElasticPoolName"];

        public static bool? GetFlag(string key)
        {
            return bool.TryParse(Config[key], out var flag) ? flag : (bool?)null;
        }

        public static int? GetInt(string key)
        {
            return int.TryParse(Config[key], out var value) ? value : (int?)null;
        }
    }
}
