// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrCollectionAccessorSource : IClrCollectionAccessorSource
    {
        private readonly ICollectionTypeFactory _collectionTypeFactory;

        private static readonly MethodInfo _genericCreate
            = typeof(ClrCollectionAccessorSource).GetTypeInfo().GetDeclaredMethods("CreateGeneric").Single();

        private static readonly MethodInfo _createAndSet
            = typeof(ClrCollectionAccessorSource).GetTypeInfo().GetDeclaredMethods("CreateAndSet").Single();

        private readonly ThreadSafeDictionaryCache<Tuple<Type, string>, IClrCollectionAccessor> _cache
            = new ThreadSafeDictionaryCache<Tuple<Type, string>, IClrCollectionAccessor>();

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ClrCollectionAccessorSource()
        {
        }

        public ClrCollectionAccessorSource([NotNull] ICollectionTypeFactory collectionTypeFactory)
        {
            Check.NotNull(collectionTypeFactory, nameof(collectionTypeFactory));

            _collectionTypeFactory = collectionTypeFactory;
        }

        public virtual IClrCollectionAccessor GetAccessor(INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            var accessor = navigation as IClrCollectionAccessor;

            if (accessor != null)
            {
                return accessor;
            }

            return _cache.GetOrAdd(
                Tuple.Create(navigation.EntityType.Type, navigation.Name),
                k => Create(navigation));
        }

        private IClrCollectionAccessor Create(INavigation navigation)
        {
            var property = navigation.EntityType.Type.GetAnyProperty(navigation.Name);
            var elementType = property.PropertyType.TryGetElementType(typeof(ICollection<>));

            // TODO: Only ICollections supported; add support for enumerables with add/remove methods
            // Issue #752
            if (elementType == null)
            {
                throw new NotSupportedException(
                    Strings.NavigationBadType(
                        navigation.Name, navigation.EntityType.FullName, property.PropertyType.FullName, navigation.GetTargetType().FullName));
            }

            if (property.PropertyType.IsArray)
            {
                throw new NotSupportedException(
                    Strings.NavigationArray(navigation.Name, navigation.EntityType.FullName, property.PropertyType.FullName));
            }

            if (property.GetMethod == null)
            {
                throw new NotSupportedException(Strings.NavigationNoGetter(navigation.Name, navigation.EntityType.FullName));
            }

            var boundMethod = _genericCreate.MakeGenericMethod(
                property.DeclaringType, property.PropertyType, elementType);

            return (IClrCollectionAccessor)boundMethod.Invoke(this, new object[] { property });
        }

        // ReSharper disable once UnusedMember.Local
        private IClrCollectionAccessor CreateGeneric<TEntity, TCollection, TElement>(PropertyInfo property)
            where TEntity : class
            where TCollection : class, ICollection<TElement>
        {
            var getterDelegate = (Func<TEntity, TCollection>)property.GetMethod.CreateDelegate(typeof(Func<TEntity, TCollection>));

            Action<TEntity, TCollection> setterDelegate = null;
            Func<TEntity, Action<TEntity, TCollection>, TCollection> createAndSetDelegate = null;

            var setter = property.SetMethod;
            if (setter != null)
            {
                setterDelegate = (Action<TEntity, TCollection>)setter.CreateDelegate(typeof(Action<TEntity, TCollection>));

                var concreteType = _collectionTypeFactory.TryFindTypeToInstantiate(typeof(TCollection));

                if (concreteType != null)
                {
                    createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)_createAndSet
                        .MakeGenericMethod(typeof(TEntity), typeof(TCollection), concreteType)
                        .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));
                }
            }

            return new ClrICollectionAccessor<TEntity, TCollection, TElement>(
                property.Name, getterDelegate, setterDelegate, createAndSetDelegate);
        }

        // ReSharper disable once UnusedMember.Local
        private static TCollection CreateAndSet<TEntity, TCollection, TConcreteCollection>(
            TEntity entity,
            Action<TEntity, TCollection> setterDelegate)
            where TEntity : class
            where TCollection : class
            where TConcreteCollection : TCollection, new()
        {
            var collection = new TConcreteCollection();
            setterDelegate(entity, collection);
            return collection;
        }
    }
}
