// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DatabaseTest
    {
        [Fact]
        public void Methods_delegate_to_configured_store_creator()
        {
            var model = Mock.Of<IModel>();
            var creatorMock = new Mock<DataStoreCreator>();
            creatorMock.Setup(m => m.Exists()).Returns(true);
            creatorMock.Setup(m => m.HasTables()).Returns(true);
            creatorMock.Setup(m => m.EnsureCreated(model)).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted()).Returns(true);

            var connection = Mock.Of<DataStoreConnection>();
            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.DataStoreCreator).Returns(creatorMock.Object);
            configurationMock.Setup(m => m.Model).Returns(model);
            configurationMock.Setup(m => m.Connection).Returns(connection);

            var database = new Database(configurationMock.Object);

            Assert.True(database.Exists());
            creatorMock.Verify(m => m.Exists(), Times.Once);

            database.Create();
            creatorMock.Verify(m => m.Create(), Times.Once);

            database.CreateTables();
            creatorMock.Verify(m => m.CreateTables(model), Times.Once);

            Assert.True(database.HasTables());
            creatorMock.Verify(m => m.HasTables(), Times.Once);

            database.Delete();
            creatorMock.Verify(m => m.Delete(), Times.Once);

            Assert.True(database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(model), Times.Once);

            Assert.True(database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(), Times.Once);

            Assert.Same(connection, database.Connection);
        }

        [Fact]
        public async void Async_methods_delegate_to_configured_store_creator()
        {
            var model = Mock.Of<IModel>();
            var cancellationToken = new CancellationTokenSource().Token;

            var creatorMock = new Mock<DataStoreCreator>();
            creatorMock.Setup(m => m.ExistsAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.HasTablesAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureCreatedAsync(model, cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureDeletedAsync(cancellationToken)).Returns(Task.FromResult(true));

            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.DataStoreCreator).Returns(creatorMock.Object);
            configurationMock.Setup(m => m.Model).Returns(model);

            var database = new Database(configurationMock.Object);

            Assert.True(await database.ExistsAsync(cancellationToken));
            creatorMock.Verify(m => m.ExistsAsync(cancellationToken), Times.Once);

            await database.CreateAsync(cancellationToken);
            creatorMock.Verify(m => m.CreateAsync(cancellationToken), Times.Once);

            await database.CreateTablesAsync(cancellationToken);
            creatorMock.Verify(m => m.CreateTablesAsync(model, cancellationToken), Times.Once);

            Assert.True(await database.HasTablesAsync(cancellationToken));
            creatorMock.Verify(m => m.HasTablesAsync(cancellationToken), Times.Once);

            await database.DeleteAsync(cancellationToken);
            creatorMock.Verify(m => m.DeleteAsync(cancellationToken), Times.Once);

            Assert.True(await database.EnsureCreatedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureCreatedAsync(model, cancellationToken), Times.Once);

            Assert.True(await database.EnsureDeletedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureDeletedAsync(cancellationToken), Times.Once);
        }
    }
}
