﻿// <auto-generated />

using System;
using System.Reflection;
using System.Resources;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Cosmos.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class CosmosStrings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.EntityFrameworkCore.Cosmos.Properties.CosmosStrings", typeof(CosmosStrings).Assembly);

        /// <summary>
        ///     Both the connection string and account key or account endpoint were specified. Only specify one set of connection details.
        /// </summary>
        public static string ConnectionStringConflictingConfiguration
            => GetString("ConnectionStringConflictingConfiguration");

        /// <summary>
        ///     Cosmos-specific methods can only be used when the context is using the Cosmos provider.
        /// </summary>
        public static string CosmosNotInUse
            => GetString("CosmosNotInUse");

        /// <summary>
        ///     The discriminator value for '{entityType1}' is '{discriminatorValue}' which is the same for '{entityType2}'. Every concrete entity type mapped to the container '{container}' needs to have a unique discriminator value.
        /// </summary>
        public static string DuplicateDiscriminatorValue([CanBeNull] object entityType1, [CanBeNull] object discriminatorValue, [CanBeNull] object entityType2, [CanBeNull] object container)
            => string.Format(
                GetString("DuplicateDiscriminatorValue", nameof(entityType1), nameof(discriminatorValue), nameof(entityType2), nameof(container)),
                entityType1, discriminatorValue, entityType2, container);

        /// <summary>
        ///     The type of the etag property '{property}' on '{entityType}' is '{propertyType}'. All etag properties need to be strings or have a string converter.
        /// </summary>
        public static string ETagNonStringStoreType([CanBeNull] object property, [CanBeNull] object entityType, [CanBeNull] object propertyType)
            => string.Format(
                GetString("ETagNonStringStoreType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

        /// <summary>
        ///     The type of the '{idProperty}' property on '{entityType}' is '{propertyType}'. All 'id' properties need to be strings or have a string converter.
        /// </summary>
        public static string IdNonStringStoreType([CanBeNull] object idProperty, [CanBeNull] object entityType, [CanBeNull] object propertyType)
            => string.Format(
                GetString("IdNonStringStoreType", nameof(idProperty), nameof(entityType), nameof(propertyType)),
                idProperty, entityType, propertyType);

        /// <summary>
        ///     'UpdateEntityType' called with '{derivedType}' which is not derived type of '{entityType}'.
        /// </summary>
        public static string InvalidDerivedTypeInEntityProjection([CanBeNull] object derivedType, [CanBeNull] object entityType)
            => string.Format(
                GetString("InvalidDerivedTypeInEntityProjection", nameof(derivedType), nameof(entityType)),
                derivedType, entityType);

        /// <summary>
        ///     Invalid 'id' value. Supply a string value that's not null or empty.
        /// </summary>
        public static string InvalidResourceId
            => GetString("InvalidResourceId");

        /// <summary>
        ///     Both properties '{property1}' and '{property2}' on entity type '{entityType}' are mapped to '{storeName}'. Map one of the properties to a different JSON property.
        /// </summary>
        public static string JsonPropertyCollision([CanBeNull] object property1, [CanBeNull] object property2, [CanBeNull] object entityType, [CanBeNull] object storeName)
            => string.Format(
                GetString("JsonPropertyCollision", nameof(property1), nameof(property2), nameof(entityType), nameof(storeName)),
                property1, property2, entityType, storeName);

        /// <summary>
        ///     Reverse could not be translated to the server because there is no ordering on the server side.
        /// </summary>
        public static string MissingOrderingInSqlExpression
            => GetString("MissingOrderingInSqlExpression");

        /// <summary>
        ///     Navigation '{entityType}.{navigationName}' doesn't point to an embedded entity.
        /// </summary>
        public static string NavigationPropertyIsNotAnEmbeddedEntity([CanBeNull] object entityType, [CanBeNull] object navigationName)
            => string.Format(
                GetString("NavigationPropertyIsNotAnEmbeddedEntity", nameof(entityType), nameof(navigationName)),
                entityType, navigationName);

        /// <summary>
        ///     The entity type '{entityType}' is sharing the container '{container}' with other types, but does not have a discriminator property configured.
        /// </summary>
        public static string NoDiscriminatorProperty([CanBeNull] object entityType, [CanBeNull] object container)
            => string.Format(
                GetString("NoDiscriminatorProperty", nameof(entityType), nameof(container)),
                entityType, container);

        /// <summary>
        ///     The entity type '{entityType}' is sharing the container '{container}' with other types, but does not have a discriminator value configured.
        /// </summary>
        public static string NoDiscriminatorValue([CanBeNull] object entityType, [CanBeNull] object container)
            => string.Format(
                GetString("NoDiscriminatorValue", nameof(entityType), nameof(container)),
                entityType, container);

        /// <summary>
        ///     The entity type '{entityType}' does not have a key declared on the '{idProperty}' property. Add a key to '{entityType}' that contains '{idProperty}'.
        /// </summary>
        public static string NoIdKey([CanBeNull] object entityType, [CanBeNull] object idProperty)
            => string.Format(
                GetString("NoIdKey", nameof(entityType), nameof(idProperty)),
                entityType, idProperty);

        /// <summary>
        ///     The entity type '{entityType}' does not have a property mapped to the 'id' property in the database. Add a property mapped as 'id'.
        /// </summary>
        public static string NoIdProperty([CanBeNull] object entityType)
            => string.Format(
                GetString("NoIdProperty", nameof(entityType)),
                entityType);

        /// <summary>
        ///     Non-embedded IncludeExpression is not supported: {expression}
        /// </summary>
        public static string NonEmbeddedIncludeNotSupported([CanBeNull] object expression)
            => string.Format(
                GetString("NonEmbeddedIncludeNotSupported", nameof(expression)),
                expression);

        /// <summary>
        ///     The entity type '{entityType}' has property '{property}' as its concurrency token, but only '_etag' is supported. Consider using 'EntityTypeBuilder.UseETagConcurrency'.
        /// </summary>
        public static string NonETagConcurrencyToken([CanBeNull] object entityType, [CanBeNull] object property)
            => string.Format(
                GetString("NonETagConcurrencyToken", nameof(entityType), nameof(property)),
                entityType, property);

        /// <summary>
        ///     The entity type '{entityType}' does not have a partition key set, but it is mapped to the container '{container}' shared by entity types with partition keys. Configure a partition key on '{entityType}'.
        /// </summary>
        public static string NoPartitionKey([CanBeNull] object entityType, [CanBeNull] object container)
            => string.Format(
                GetString("NoPartitionKey", nameof(entityType), nameof(container)),
                entityType, container);

        /// <summary>
        ///     The entity type '{entityType}' does not have a key declared on '{partitionKey}' and '{idProperty}' properties. Add a key to '{entityType}' that contains '{partitionKey}' and '{idProperty}'.
        /// </summary>
        public static string NoPartitionKeyKey([CanBeNull] object entityType, [CanBeNull] object partitionKey, [CanBeNull] object idProperty)
            => string.Format(
                GetString("NoPartitionKeyKey", nameof(entityType), nameof(partitionKey), nameof(idProperty)),
                entityType, partitionKey, idProperty);

        /// <summary>
        ///     There is no string-based representation of this query as it's executed using 'ReadItemQueryAsync({resourceId}, {partitionKey})'.
        /// </summary>
        public static string NoReadItemQueryString([CanBeNull] object resourceId, [CanBeNull] object partitionKey)
            => string.Format(
                GetString("NoReadItemQueryString", nameof(resourceId), nameof(partitionKey)),
                resourceId, partitionKey);

        /// <summary>
        ///     Expression '{sqlExpression}' in SQL tree does not have type mapping assigned.
        /// </summary>
        public static string NullTypeMappingInSqlTree([CanBeNull] object sqlExpression)
            => string.Format(
                GetString("NullTypeMappingInSqlTree", nameof(sqlExpression)),
                sqlExpression);

        /// <summary>
        ///     Offset is not supported without Limit.
        /// </summary>
        public static string OffsetRequiresLimit
            => GetString("OffsetRequiresLimit");

        /// <summary>
        ///     The entity of type '{entityType}' is mapped as a part of the document mapped to '{missingEntityType}', but there is no tracked entity of this type with the corresponding key value. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the key values.
        /// </summary>
        public static string OrphanedNestedDocument([CanBeNull] object entityType, [CanBeNull] object missingEntityType)
            => string.Format(
                GetString("OrphanedNestedDocument", nameof(entityType), nameof(missingEntityType)),
                entityType, missingEntityType);

        /// <summary>
        ///     The entity of type '{entityType}' is mapped as a part of the document mapped to '{missingEntityType}', but there is no tracked entity of this type with the key value '{keyValue}'.
        /// </summary>
        public static string OrphanedNestedDocumentSensitive([CanBeNull] object entityType, [CanBeNull] object missingEntityType, [CanBeNull] object keyValue)
            => string.Format(
                GetString("OrphanedNestedDocumentSensitive", nameof(entityType), nameof(missingEntityType), nameof(keyValue)),
                entityType, missingEntityType, keyValue);

        /// <summary>
        ///     A ReadItem query was detected, but the partition key value is missing.
        /// </summary>
        public static string ParitionKeyMissing
            => GetString("ParitionKeyMissing");

        /// <summary>
        ///     Partition key specified in the WithPartitionKey call '{paritionKey1}' and the partition key specified in the Where predicate '{paritionKey2}' must be identical. Remove one of them .
        /// </summary>
        public static string PartitionKeyMismatch([CanBeNull] object paritionKey1, [CanBeNull] object paritionKey2)
            => string.Format(
                GetString("PartitionKeyMismatch", nameof(paritionKey1), nameof(paritionKey2)),
                paritionKey1, paritionKey2);

        /// <summary>
        ///     The partition key for entity type '{entityType}' is set to '{property}', but there is no property with that name.
        /// </summary>
        public static string PartitionKeyMissingProperty([CanBeNull] object entityType, [CanBeNull] object property)
            => string.Format(
                GetString("PartitionKeyMissingProperty", nameof(entityType), nameof(property)),
                entityType, property);

        /// <summary>
        ///     The type of the partition key property '{property}' on '{entityType}' is '{propertyType}'. All partition key properties need to be strings or have a string converter.
        /// </summary>
        public static string PartitionKeyNonStringStoreType([CanBeNull] object property, [CanBeNull] object entityType, [CanBeNull] object propertyType)
            => string.Format(
                GetString("PartitionKeyNonStringStoreType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

        /// <summary>
        ///     The partition key property '{property1}' on '{entityType1}' is mapped as '{storeName1}', but the partition key property '{property2}' on '{entityType2}' is mapped as '{storeName2}'. All partition key properties need to be mapped to the same store property.
        /// </summary>
        public static string PartitionKeyStoreNameMismatch([CanBeNull] object property1, [CanBeNull] object entityType1, [CanBeNull] object storeName1, [CanBeNull] object property2, [CanBeNull] object entityType2, [CanBeNull] object storeName2)
            => string.Format(
                GetString("PartitionKeyStoreNameMismatch", nameof(property1), nameof(entityType1), nameof(storeName1), nameof(property2), nameof(entityType2), nameof(storeName2)),
                property1, entityType1, storeName1, property2, entityType2, storeName2);

        /// <summary>
        ///     A ReadItem query was detected, but the 'id' value is missing and cannot be generated.
        /// </summary>
        public static string ResourceIdMissing
            => GetString("ResourceIdMissing");

        /// <summary>
        ///     Reverse is not supported without Limit or Offset.
        /// </summary>
        public static string ReverseRequiresOffsetOrLimit
            => GetString("ReverseRequiresOffsetOrLimit");

        /// <summary>
        ///     Unable to bind '{memberType}' '{member}' to entity projection of '{entityType}'.
        /// </summary>
        public static string UnableToBindMemberToEntityProjection([CanBeNull] object memberType, [CanBeNull] object member, [CanBeNull] object entityType)
            => string.Format(
                GetString("UnableToBindMemberToEntityProjection", nameof(memberType), nameof(member), nameof(entityType)),
                memberType, member, entityType);

        /// <summary>
        ///     Unsupported operator '{nodeType}' specified for expression of type '{expressionType}'.
        /// </summary>
        public static string UnsupportedOperatorForSqlExpression([CanBeNull] object nodeType, [CanBeNull] object expressionType)
            => string.Format(
                GetString("UnsupportedOperatorForSqlExpression", nameof(nodeType), nameof(expressionType)),
                nodeType, expressionType);

        /// <summary>
        ///     Conflicts were detected for item with id '{itemId}'.
        /// </summary>
        public static string UpdateConflict([CanBeNull] object itemId)
            => string.Format(
                GetString("UpdateConflict", nameof(itemId)),
                itemId);

        /// <summary>
        ///     VisitChildren must be overridden in class deriving from SqlExpression.
        /// </summary>
        public static string VisitChildrenMustBeOverridden
            => GetString("VisitChildrenMustBeOverridden");

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

