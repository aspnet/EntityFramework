// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Services;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class QueryCachingTests
    {
        private readonly Mock<AtsConnection> _connection;
        private readonly AtsDataStore _dataStore;

        public QueryCachingTests()
        {
            _connection = new Mock<AtsConnection>();
            _connection
                .Setup(s => s.ExecuteRequest(It.IsAny<TableRequest<bool>>(), It.IsAny<ILogger>()))
                .Returns(false); // keep all requests in memory

            var configuration = new Mock<DbContextConfiguration>();
            configuration.SetupGet(s => s.Connection).Returns(_connection.Object);
            configuration.SetupGet(s => s.Model).Returns(CreateModel());
            configuration.SetupGet(s => s.LoggerFactory).Returns(new NullLoggerFactory());
            configuration.SetupGet(s => s.StateManager).Returns(new Mock<StateManager>().Object);
            configuration.SetupGet(s => s.Services.EntityKeyFactorySource).Returns(new Mock<EntityKeyFactorySource>().Object);
            configuration.SetupGet(s => s.Services.StateEntryFactory).Returns(new Mock<StateEntryFactory>().Object);

            _dataStore = new AtsDataStore(
                configuration.Object,
                _connection.Object,
                new AtsQueryFactory(new AtsValueReaderFactory()),
                new TableEntityAdapterFactory());
        }

        [Fact]
        public void Multiple_identical_queries()
        {
            AssertQuery<Customer>(Times.Once(), cs =>
                (from c in cs
                orderby cs.Any(c2 => c2.CustomerID == c.CustomerID)
                select c).AsNoTracking());
        }

        private void AssertQuery<T>(Times times, Expression<Func<DbSet<T>, IQueryable>> expression) where T : class, new()
        {
            var query = expression.Compile()(new DbSet<T>(Mock.Of<DbContext>()));
            var queryModel = new EntityQueryProvider(new EntityQueryExecutor(Mock.Of<DbContext>())).GenerateQueryModel(query.Expression);

            _connection.Setup(s => s.ExecuteRequest(
                It.IsAny<QueryTableRequest<T>>(),
                It.IsAny<ILogger>()))
                .Returns(NewTCallback<T>);

            _dataStore.Query<T>(queryModel).ToList();

            _connection.Verify(s => s.ExecuteRequest(
                It.IsAny<QueryTableRequest<T>>(),
                It.IsAny<ILogger>()),
                times);
        }

        private static T[] NewTCallback<T>() where T : class, new()
        {
            return new[] { new T() };
        }

        private IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<Customer>().Property(s => s.CustomerID);

            return model;
        }

        internal class Customer
        {
            public string CustomerID { get; set; }
        }
    }
}
