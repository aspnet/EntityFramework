// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents an entity type in an <see cref="IModel" />.
    /// </summary>
    public interface IEntityType : ITypeBase
    {
        /// <summary>
        ///     Gets the base type of the entity. Returns null if this is not a derived type in an inheritance hierarchy.
        /// </summary>
        IEntityType BaseType { get; }

        /// <summary>
        ///     <para>
        ///         Gets primary key for this entity. Returns null if no primary key is defined.
        ///     </para>
        ///     <para>
        ///         To be a valid model, each entity type must have a primary key defined. Therefore, the primary key may be
        ///         null while the model is being created, but will be present by the time the model is used with a <see cref="DbContext" />.
        ///     </para>
        /// </summary>
        /// <returns> The primary key, or null if none is defined. </returns>
        IKey FindPrimaryKey();

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given properties. Returns null if no key is defined
        ///     for the given properties.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key, or null if none is defined. </returns>
        IKey FindKey([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Gets the primary and alternate keys for this entity.
        /// </summary>
        /// <returns> The primary and alternate keys. </returns>
        IEnumerable<IKey> GetKeys();

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns null
        ///     if no foreign key is found.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key, or null if none is defined. </returns>
        IForeignKey FindForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign keys defined on this entity.
        /// </summary>
        /// <returns> The foreign keys defined on this entity. </returns>
        IEnumerable<IForeignKey> GetForeignKeys();

        /// <summary>
        ///     Gets the index defined on the given properties. Returns null if no index is defined.
        /// </summary>
        /// <param name="properties"> The properties to find the index on. </param>
        /// <returns> The index, or null if none is found. </returns>
        IIndex FindIndex([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Gets the indexes defined on this entity.
        /// </summary>
        /// <returns> The indexes defined on this entity. </returns>
        IEnumerable<IIndex> GetIndexes();

        /// <summary>
        ///     <para>
        ///         Gets the property with a given name. Returns null if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="EntityTypeExtensions.FindNavigation(IEntityType, string)" /> to find a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property, or null if none is found. </returns>
        IProperty FindProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets the properties defined on this entity.
        ///     </para>
        ///     <para>
        ///         This API only returns scalar properties and does not return navigation properties. Use
        ///         <see cref="EntityTypeExtensions.GetNavigations(IEntityType)" /> to get navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The properties defined on this entity. </returns>
        IEnumerable<IProperty> GetProperties();

        /// <summary>
        ///     Gets the model that this type belongs to.
        /// </summary>
        new IModel Model { get; } // Defined here to maintain binary compat with 1.0

        /// <summary>
        ///     Gets the name of this type.
        /// </summary>
        new string Name { get; } // Defined here to maintain binary compat with 1.0

        /// <summary>
        ///     <para>
        ///         Gets the CLR class that is used to represent instances of this type. Returns null if the type does not have a
        ///         corresponding CLR class (known as a shadow type).
        ///     </para>
        ///     <para>
        ///         Shadow types are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
        ///         Therefore, shadow types will only exist in migration model snapshots, etc.
        ///     </para>
        /// </summary>
        new Type ClrType { get; } // Defined here to maintain binary compat with 1.0
    }
}
