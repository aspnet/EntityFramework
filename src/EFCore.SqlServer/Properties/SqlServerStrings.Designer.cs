// <auto-generated />

using System;
using System.Reflection;
using System.Resources;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///		This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class SqlServerStrings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.EntityFrameworkCore.Properties.SqlServerStrings", typeof(SqlServerStrings).GetTypeInfo().Assembly);

        /// <summary>
        ///     Identity value generation cannot be used for the property '{property}' on entity type '{entityType}' because the property type is '{propertyType}'. Identity value generation can only be used with signed integer properties.
        /// </summary>
        public static string IdentityBadType([CanBeNull] object property, [CanBeNull] object entityType, [CanBeNull] object propertyType)
            => string.Format(
                GetString("IdentityBadType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

        /// <summary>
        ///     Data type '{dataType}' is not supported in this form. Either specify the length explicitly in the type name, for example as '{dataType}(16)', or remove the data type and use APIs such as HasMaxLength to allow EF choose the data type.
        /// </summary>
        public static string UnqualifiedDataType([CanBeNull] object dataType)
            => string.Format(
                GetString("UnqualifiedDataType", nameof(dataType)),
                dataType);

        /// <summary>
        ///     SQL Server sequences cannot be used to generate values for the property '{property}' on entity type '{entityType}' because the property type is '{propertyType}'. Sequences can only be used with integer properties.
        /// </summary>
        public static string SequenceBadType([CanBeNull] object property, [CanBeNull] object entityType, [CanBeNull] object propertyType)
            => string.Format(
                GetString("SequenceBadType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

        /// <summary>
        ///     SQL Server requires the table name to be specified for rename index operations. Specify table name in the call to MigrationBuilder.RenameIndex.
        /// </summary>
        public static string IndexTableRequired
            => GetString("IndexTableRequired");

        /// <summary>
        ///     To set memory-optimized on a table on or off the table needs to be dropped and recreated.
        /// </summary>
        public static string AlterMemoryOptimizedTable
            => GetString("AlterMemoryOptimizedTable");

        /// <summary>
        ///     To change the IDENTITY property of a column, the column needs to be dropped and recreated.
        /// </summary>
        public static string AlterIdentityColumn
            => GetString("AlterIdentityColumn");

        /// <summary>
        ///     An exception has been raised that is likely due to a transient failure. If you are connecting to a SQL Azure database consider using SqlAzureExecutionStrategy.
        /// </summary>
        public static string TransientExceptionDetected
            => GetString("TransientExceptionDetected");

        /// <summary>
        ///     No type was specified for the decimal column '{property}' on entity type '{entityType}'. This will cause values to be silently truncated if they do not fit in the default precision and scale. Explicitly specify the SQL server column type that can accomadate all the values using 'ForSqlServerHasColumnType()'.
        /// </summary>
        public static readonly EventDefinition<string, string> LogDefaultDecimalTypeColumn
            = new EventDefinition<string, string>(
                SqlServerEventId.DecimalTypeDefaultWarning,
                LogLevel.Warning,
                LoggerMessage.Define<string, string>(
                    LogLevel.Warning,
                    SqlServerEventId.DecimalTypeDefaultWarning,
                    _resourceManager.GetString("LogDefaultDecimalTypeColumn")));

        /// <summary>
        ///     The property '{property}' on entity type '{entityType}' is of type 'byte', but is set up to use a SQL Server identity column. This requires that values starting at 255 and counting down will be used for temporary key values. A temporary key value is needed for every entity inserted in a single call to 'SaveChanges'. Care must be taken that these values do not collide with real key values.
        /// </summary>
        public static readonly EventDefinition<string, string> LogByteIdentityColumn
            = new EventDefinition<string, string>(
                SqlServerEventId.ByteIdentityColumnWarning,
                LogLevel.Warning,
                LoggerMessage.Define<string, string>(
                    LogLevel.Warning,
                    SqlServerEventId.ByteIdentityColumnWarning,
                    _resourceManager.GetString("LogByteIdentityColumn")));

        /// <summary>
        ///     The property '{property}' on entity type '{entityType}' is configured to use 'Identity' or 'SequenceHiLo' value generator, which are only intended for keys. If this was intentional configure an alternate key on the property, otherwise call `ValueGeneratedNever` or configure store generation for this property.
        /// </summary>
        public static string NonKeyValueGeneration([CanBeNull] object property, [CanBeNull] object entityType)
            => string.Format(
                GetString("NonKeyValueGeneration", nameof(property), nameof(entityType)),
                property, entityType);

        /// <summary>
        ///     Cannot use table '{table}' for entity type '{entityType}' since it is being used for entity type '{otherEntityType}' and entity type '{memoryOptimizedEntityType}' is marked as memory-optimized, but entity type '{nonMemoryOptimizedEntityType}' is not.
        /// </summary>
        public static string IncompatibleTableMemoryOptimizedMismatch([CanBeNull] object table, [CanBeNull] object entityType, [CanBeNull] object otherEntityType, [CanBeNull] object memoryOptimizedEntityType, [CanBeNull] object nonMemoryOptimizedEntityType)
            => string.Format(
                GetString("IncompatibleTableMemoryOptimizedMismatch", nameof(table), nameof(entityType), nameof(otherEntityType), nameof(memoryOptimizedEntityType), nameof(nonMemoryOptimizedEntityType)),
                table, entityType, otherEntityType, memoryOptimizedEntityType, nonMemoryOptimizedEntityType);

        /// <summary>
        ///     For column {columnId}. This column is set up as an Identity column, but the SQL Server data type is {sqlServerDataType}. This will be mapped to CLR type byte which does not allow the SqlServerValueGenerationStrategy.IdentityColumn setting. Generating a matching Property but ignoring the Identity setting.
        /// </summary>
        public static readonly EventDefinition<string, string> LogDataTypeDoesNotAllowSqlServerIdentityStrategy
            = new EventDefinition<string, string>(
                SqlServerEventId.DataTypeDoesNotAllowSqlServerIdentityStrategyWarning,
                LogLevel.Warning,
                LoggerMessage.Define<string, string>(
                    LogLevel.Warning,
                    SqlServerEventId.DataTypeDoesNotAllowSqlServerIdentityStrategyWarning,
                    _resourceManager.GetString("LogDataTypeDoesNotAllowSqlServerIdentityStrategy")));

        /// <summary>
        ///     Found default schema {defaultSchema}.
        /// </summary>
        public static readonly EventDefinition<string> LogFoundDefaultSchema
            = new EventDefinition<string>(
                SqlServerEventId.DefaultSchemaFound,
                LogLevel.Debug,
                LoggerMessage.Define<string>(
                    LogLevel.Debug,
                    SqlServerEventId.DefaultSchemaFound,
                    _resourceManager.GetString("LogFoundDefaultSchema")));

        /// <summary>
        ///     Found type alias with name: {alias} which maps to underlying data type {dataType}.
        /// </summary>
        public static readonly EventDefinition<string, string> LogFoundTypeAlias
            = new EventDefinition<string, string>(
                SqlServerEventId.TypeAliasFound,
                LogLevel.Debug,
                LoggerMessage.Define<string, string>(
                    LogLevel.Debug,
                    SqlServerEventId.TypeAliasFound,
                    _resourceManager.GetString("LogFoundTypeAlias")));

        /// <summary>
        ///     Found column with table: {tableName}, column name: {columnName}, data type: {dataType}, ordinal: {ordinal}, nullable: {isNullable}, primary key ordinal: {primaryKeyOrdinal}, default value: {defaultValue}, computed value: {computedValue}, precision: {precision}, scale: {scale}, maximum length: {maxLength}, identity: {isIdentity}, computed: {isComputed}.
        /// </summary>
        public static readonly FallbackEventDefinition LogFoundColumn
            = new FallbackEventDefinition(
                SqlServerEventId.ColumnFound,
                LogLevel.Debug,
                _resourceManager.GetString("LogFoundColumn"));

        /// <summary>
        ///     Found foreign key column with table: {tableName}, foreign key name: {fkName}, principal table: {principalTableName}, column name: {columnName}, principal column name: {principalColumnName}, update action: {updateAction}, delete action: {deleteAction}, ordinal: {ordinal}.
        /// </summary>
        public static readonly FallbackEventDefinition LogFoundForeignKeyColumn
            = new FallbackEventDefinition(
                SqlServerEventId.ForeignKeyColumnFound,
                LogLevel.Debug,
                _resourceManager.GetString("LogFoundForeignKeyColumn"));

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);
            for (var i = 0; i < formatterNames.Length; i++)
            {
                value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }
}
