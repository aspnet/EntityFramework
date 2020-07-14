﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationshipsQueryRelationalTestBase<TFixture> : InheritanceRelationshipsQueryTestBase<TFixture>
        where TFixture : InheritanceRelationshipsQueryRelationalFixture, new()
    {
        public InheritanceRelationshipsQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase),
                elementAsserter: (e, a) => AssertInclude(e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseCollectionOnBase>().Include(e => e.BaseParent),
                elementAsserter: (e, a) => AssertInclude(e, a,
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_with_filter_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).Where(e => e.Name != "Bar"),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseCollectionOnBase>().Include(e => e.BaseParent).Where(e => e.Name != "Bar"),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_without_inheritance_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.CollectionOnBase),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.CollectionOnBase)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_without_inheritance_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<CollectionOnBase>().Include(e => e.Parent),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<CollectionOnBase>(x => x.Parent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_without_inheritance_with_filter_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.CollectionOnBase).Where(e => e.Name != "Bar"),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.CollectionOnBase)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<CollectionOnBase>().Include(e => e.Parent).Where(e => e.Name != "Bar"),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<CollectionOnBase>(x => x.Parent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_on_derived1_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_on_derived2_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnDerived),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseCollectionOnDerived)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_on_derived3_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.DerivedCollectionOnDerived),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.DerivedCollectionOnDerived)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseCollectionOnDerived>().Include(e => e.BaseParent),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseCollectionOnDerived>(x => x.BaseParent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_with_inheritance_reference_collection_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedCollection),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                    new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedCollection)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedCollection),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                    new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedCollection)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NestedCollectionBase>().Include(e => e.ParentReference.BaseParent),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<NestedCollectionBase>(x => x.ParentReference),
                    new ExpectedInclude<BaseReferenceOnBase>(x => x.BaseParent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_with_inheritance_collection_reference_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedReference),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase),
                    new ExpectedInclude<BaseCollectionOnBase>(x => x.NestedReference)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NestedReferenceBase>().Include(e => e.ParentCollection.BaseParent),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<NestedReferenceBase>(x => x.ParentCollection),
                    new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_with_inheritance_collection_collection_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedCollection),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase),
                    new ExpectedInclude<BaseCollectionOnBase>(x => x.NestedCollection)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NestedCollectionBase>().Include(e => e.ParentCollection.BaseParent),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<NestedCollectionBase>(x => x.ParentCollection),
                    new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_include_collection_reference_on_non_entity_base_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ReferencedEntity>().Include(e => e.Principals).ThenInclude(e => e.Reference),
                elementAsserter: (e, a) => AssertInclude(e, a,
                    new ExpectedInclude<ReferencedEntity>(x => x.Principals),
                    new ExpectedInclude<PrincipalEntity>(x => x.Reference)));
        }
    }
}
