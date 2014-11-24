// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Sequence = Microsoft.Data.Entity.Relational.Model.Sequence;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelperExtensions
    {
        public static EntityServicesBuilder AddProviderServices(this EntityServicesBuilder entityServicesBuilder)
        {
            entityServicesBuilder
                .AddRelational().ServiceCollection
                .AddSingleton<ILoggerFactory, RecordingLoggerFactory>()
                .AddSingleton<FakeDatabaseBuilder>()
                .AddSingleton<FakeValueGeneratorCache>()
                .AddSingleton<FakeSqlGenerator>()
                .AddSingleton<SqlStatementExecutor, RecordingSqlStatementExecutor>()
                .AddSingleton<FakeTypeMapper>()
                .AddSingleton<ModificationCommandBatchFactory>()
                .AddSingleton<FakeCommandBatchPreparer>()
                .AddScoped<BatchExecutor>()
                .AddScoped<DataStoreSource, FakeDataStoreSource>()
                .AddScoped<FakeDataStoreServices>()
                .AddScoped<FakeDataStore>()
                .AddScoped<FakeRelationalConnection>()
                .AddScoped<FakeModelDiffer>()
                .AddScoped<FakeDatabase>()
                .AddScoped<FakeMigrationOperationSqlGeneratorFactory>()
                .AddScoped<RecordingDataStoreCreator>()
                .AddScoped<MigrationAssembly>()
                .AddScoped<HistoryRepository>()
                .AddScoped<TestMigrator>();

            return entityServicesBuilder;
        }

        public static DbContextOptions UseProviderOptions(this DbContextOptions options)
        {
            ((IDbContextOptions)options).AddOrUpdateExtension<FakeRelationalOptionsExtension>(e => e.Connection = new FakeDbConnection());

            return options;
        }
    }

    public class TestMigrator : Migrator
    {
        public TestMigrator(
            HistoryRepository historyRepository,
            MigrationAssembly migrationAssembly,
            FakeModelDiffer modelDiffer,
            FakeMigrationOperationSqlGeneratorFactory ddlSqlGeneratorFactory,
            FakeSqlGenerator dmlSqlGenerator,
            SqlStatementExecutor sqlExecutor,
            RecordingDataStoreCreator storeCreator,
            FakeRelationalConnection connection,
            ILoggerFactory loggerFactory)
            : base(
                historyRepository,
                migrationAssembly,
                modelDiffer,
                ddlSqlGeneratorFactory,
                dmlSqlGenerator,
                sqlExecutor,
                storeCreator,
                connection,
                loggerFactory)
        {
        }
    }

    public class FakeDatabaseBuilder : DatabaseBuilder
    {
        public FakeDatabaseBuilder([NotNull] FakeTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        protected override Sequence BuildSequence(IProperty property)
        {
            return null;
        }
    }

    public class FakeModelDiffer : ModelDiffer
    {
        public FakeModelDiffer(FakeDatabaseBuilder databaseBuilder)
            : base(databaseBuilder)
        {
        }

        protected override string GetSequenceName(Column column)
        {
            Check.NotNull(column, "column");

            return null;
        }
    }

    public class FakeTypeMapper : RelationalTypeMapper
    {
    }

    public class FakeSqlGenerator : SqlGenerator
    {
        public override void AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, SchemaQualifiedName schemaQualifiedName)
        {
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        {
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
        }
    }

    public class FakeMigrationOperationSqlGeneratorFactory : IMigrationOperationSqlGeneratorFactory
    {
        public virtual MigrationOperationSqlGenerator Create()
        {
            return new FakeMigrationOperationSqlGenerator();
        }

        public virtual MigrationOperationSqlGenerator Create(DatabaseModel database)
        {
            return new FakeMigrationOperationSqlGenerator();
        }
    }

    public class FakeMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        public override IEnumerable<SqlStatement> Generate(IEnumerable<MigrationOperation> migrationOperations)
        {
            return new FakeMigrationsSqlGenerator().Generate(migrationOperations);
        }

        public override void Generate(RenameSequenceOperation renameSequenceOperation, IndentedStringBuilder stringBuilder)
        {
        }

        public override void Generate(MoveSequenceOperation moveSequenceOperation, IndentedStringBuilder stringBuilder)
        {
        }

        public override void Generate(RenameTableOperation renameTableOperation, IndentedStringBuilder stringBuilder)
        {
        }

        public override void Generate(MoveTableOperation moveTableOperation, IndentedStringBuilder stringBuilder)
        {
        }

        public override void Generate(RenameColumnOperation renameColumnOperation, IndentedStringBuilder stringBuilder)
        {
        }

        public override void Generate(RenameIndexOperation renameIndexOperation, IndentedStringBuilder stringBuilder)
        {
        }
    }

    public class FakeMigrationsSqlGenerator : MigrationOperationVisitor<IndentedStringBuilder>
    {
        public virtual IEnumerable<SqlStatement> Generate(IEnumerable<MigrationOperation> operations)
        {
            return operations.Select(
                o =>
                {
                    var sqlOperation = o as SqlOperation;
                    var builder = new IndentedStringBuilder();

                    o.Accept(this, builder);

                    return
                            new SqlStatement(builder.ToString())
                            {
                                SuppressTransaction = sqlOperation != null && sqlOperation.SuppressTransaction
                            };
                });
        }

        public override void Visit(CreateTableOperation operation, IndentedStringBuilder builder)
        {
            builder.Append("Create").Append(operation.TableName).Append("Sql");
        }

        public override void Visit(DropTableOperation operation, IndentedStringBuilder builder)
        {
            builder.Append("Drop").Append(operation.TableName).Append("Sql");
        }

        public override void Visit(SqlOperation operation, IndentedStringBuilder builder)
        {
            builder.Append(operation.Sql);
        }

        protected override void VisitDefault(MigrationOperation operation, IndentedStringBuilder builder)
        {
            builder.Append(operation.GetType().Name).Append("Sql");
        }
    }

    public class RecordingSqlStatementExecutor : SqlStatementExecutor
    {
        private List<Tuple<DbConnection, DbTransaction, string[]>> _nonQueries
            = new List<Tuple<DbConnection, DbTransaction, string[]>>();

        public RecordingSqlStatementExecutor(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public List<Tuple<DbConnection, DbTransaction, string[]>> NonQueries
        {
            get { return _nonQueries; }
        }

        public override void ExecuteNonQuery(DbConnection connection, DbTransaction transaction, IEnumerable<SqlStatement> statements)
        {
            _nonQueries.Add(Tuple.Create(connection, transaction, statements.Select(s => s.Sql).ToArray()));

            base.ExecuteNonQuery(connection, transaction, statements);
        }
    }

    public class RecordingLoggerFactory : ILoggerFactory
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public string LogContent
        {
            get { return _builder.ToString(); }
        }

        public ILogger Create(string name)
        {
            return new RecordingLogger(name, _builder);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }
    }

    public class RecordingLogger : ILogger
    {
        private readonly string _name;
        private readonly StringBuilder _builder;

        public RecordingLogger(string name, StringBuilder builder)
        {
            _name = name;
            _builder = builder;
        }

        public void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            _builder
                .Append(_name)
                .Append(" ")
                .Append(logLevel.ToString("G"))
                .Append(" ")
                .AppendLine(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope(object state)
        {
            throw new NotImplementedException();
        }
    }

    public class RecordingDataStoreCreator : RelationalDataStoreCreator
    {
        private bool _existsState = true;

        public bool ExistsState
        {
            get { return _existsState; }
            set { _existsState = value; }
        }

        public bool Created { get; set; }

        public override bool Exists()
        {
            return ExistsState;
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(false);
        }

        public override void Create()
        {
            ExistsState = true;
            Created = true;
        }

        public override Task CreateAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ExistsState = true;
            Created = true;
            return Task.FromResult(0);
        }

        public override void Delete()
        {
            ExistsState = false;
        }

        public override Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ExistsState = false;
            return Task.FromResult(0);
        }

        public override void CreateTables(IModel model)
        {
        }

        public override Task CreateTablesAsync(IModel model, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(0);
        }

        public override bool HasTables()
        {
            return false;
        }

        public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(false);
        }
    }

    public class FakeRelationalConnection : RelationalConnection
    {
        public FakeRelationalConnection(ContextService<IDbContextOptions> options, ILoggerFactory loggerFactory)
            : base(options, loggerFactory)
        {
        }

        protected override DbConnection CreateDbConnection()
        {
            return new FakeDbConnection();
        }
    }

    public class FakeDataStoreSource : DataStoreSource<FakeDataStoreServices, FakeRelationalOptionsExtension>
    {
        public FakeDataStoreSource(ContextServices services, ContextService<IDbContextOptions> options)
            : base(services, options)
        {
        }

        public override string Name
        {
            get { return GetType().Name; }
        }

        public override bool IsAvailable
        {
            get { return true; }
        }

        public override bool IsConfigured
        {
            get { return true; }
        }
    }

    public class FakeDataStoreServices : MigrationsDataStoreServices
    {
        private readonly FakeDataStore _store;
        private readonly RecordingDataStoreCreator _creator;
        private readonly FakeRelationalConnection _connection;
        private readonly FakeValueGeneratorCache _valueGeneratorCache;
        private readonly FakeDatabase _database;
        private readonly ModelBuilderFactory _modelBuilderFactory;
        private readonly TestMigrator _migrator;

        public FakeDataStoreServices(
            FakeDataStore store,
            RecordingDataStoreCreator creator,
            FakeRelationalConnection connection,
            FakeValueGeneratorCache valueGeneratorCache,
            FakeDatabase database,
            ModelBuilderFactory modelBuilderFactory,
            TestMigrator migrator)
        {
            _store = store;
            _creator = creator;
            _connection = connection;
            _valueGeneratorCache = valueGeneratorCache;
            _database = database;
            _modelBuilderFactory = modelBuilderFactory;
            _migrator = migrator;
        }

        public override DataStore Store
        {
            get { return _store; }
        }

        public override DataStoreCreator Creator
        {
            get { return _creator; }
        }

        public override DataStoreConnection Connection
        {
            get { return _connection; }
        }

        public override ValueGeneratorCache ValueGeneratorCache
        {
            get { return _valueGeneratorCache; }
        }

        public override Database Database
        {
            get { return _database; }
        }

        public override IModelBuilderFactory ModelBuilderFactory
        {
            get { return _modelBuilderFactory; }
        }

        public override Migrator Migrator
        {
            get { return _migrator; }
        }
    }

    public class FakeDatabase : MigrationsEnabledDatabase
    {
        public FakeDatabase(
            ContextService<IModel> model,
            RecordingDataStoreCreator dataStoreCreator,
            FakeRelationalConnection connection,
            TestMigrator migrator,
            ILoggerFactory loggerFactory)
            : base(model, dataStoreCreator, connection, migrator, loggerFactory)
        {
        }
    }

    public class FakeRelationalOptionsExtension : RelationalOptionsExtension
    {
        protected override void ApplyServices(EntityServicesBuilder builder)
        {
        }
    }

    public class FakeDataStore : RelationalDataStore
    {
        public FakeDataStore(
            StateManager stateManager,
            ContextService<IModel> model,
            EntityKeyFactorySource entityKeyFactorySource,
            EntityMaterializerSource entityMaterializerSource,
            ClrCollectionAccessorSource collectionAccessorSource,
            ClrPropertySetterSource propertySetterSource,
            FakeRelationalConnection connection,
            FakeCommandBatchPreparer batchPreparer,
            BatchExecutor batchExecutor,
            ILoggerFactory loggerFactory)
            : base(stateManager, model, entityKeyFactorySource, entityMaterializerSource,
                collectionAccessorSource, propertySetterSource, connection, batchPreparer, batchExecutor, loggerFactory)
        {
        }
    }

    public class FakeValueGeneratorCache : ValueGeneratorCache
    {
    }

    public class FakeCommandBatchPreparer : CommandBatchPreparer
    {
        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
        {
            throw new NotImplementedException();
        }

        public override IRelationalEntityTypeExtensions GetEntityTypeExtensions(IEntityType entityType)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeDbTransaction : DbTransaction
    {
        private readonly DbConnection _connection;

        public FakeDbTransaction(DbConnection connection)
        {
            _connection = connection;
        }

        public bool Committed { get; set; }

        public override void Commit()
        {
            Committed = true;
        }

        public override void Rollback()
        {
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return IsolationLevel.Chaos; }
        }
    }


    public class FakeDbException : DbException
    {
    }

    public class FakeDbConnection : DbConnection
    {
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new FakeDbTransaction(this);
        }

        public override void Close()
        {
        }

        public override void ChangeDatabase(string databaseName)
        {
        }

        public override void Open()
        {
        }

        public override string ConnectionString
        {
            get { return "MyConnectionString"; }
            set { }
        }

        public override string Database
        {
            get { return ""; }
        }

        public override ConnectionState State
        {
            get { return ConnectionState.Closed; }
        }

        public override string DataSource
        {
            get { throw new NotImplementedException(); }
        }

        public override string ServerVersion
        {
            get { throw new NotImplementedException(); }
        }

        protected override DbCommand CreateDbCommand()
        {
            return new FakeDbCommand();
        }
    }

    public class FakeDbCommand : DbCommand
    {
        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override string CommandText { get; set; }

        public override int CommandTimeout { get; set; }

        public override CommandType CommandType { get; set; }

        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbConnection DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection
        {
            get { throw new NotImplementedException(); }
        }

        protected override DbTransaction DbTransaction { get; set; }

        public override bool DesignTimeVisible { get; set; }

        public override void Cancel()
        {
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            return 1;
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }
    }
}
