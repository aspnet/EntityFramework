﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class TypeExtensionsTest
    {
        public class CtorFixture
        {
            public CtorFixture()
            {
            }

            // ReSharper disable once UnusedParameter.Local
            public CtorFixture(int frob)
            {
            }
        }

        [Fact]
        public void GetDeclaredConstructor_finds_ctor_no_args()
        {
            var constructorInfo = typeof(CtorFixture).GetDeclaredConstructor(null);

            Assert.NotNull(constructorInfo);
            Assert.Equal(0, constructorInfo.GetParameters().Length);
        }

        [Fact]
        public void GetDeclaredConstructor_returns_null_when_no_match()
        {
            Assert.Null(typeof(CtorFixture).GetDeclaredConstructor(new[] { typeof(string) }));
        }

        [Fact]
        public void GetDeclaredConstructor_finds_ctor_args()
        {
            var constructorInfo = typeof(CtorFixture).GetDeclaredConstructor(new[] { typeof(int) });

            Assert.NotNull(constructorInfo);
            Assert.Equal(1, constructorInfo.GetParameters().Length);
        }

        [Fact]
        public void IsNullableType_when_value_or_nullable_type()
        {
            Assert.True(typeof(string).IsNullableType());
            Assert.False(typeof(int).IsNullableType());
            Assert.False(typeof(Guid).IsNullableType());
            Assert.True(typeof(int?).IsNullableType());
        }

        [Fact]
        public void Element_type_should_return_element_type_from_sequence_type()
        {
            Assert.Equal(typeof(string), typeof(IEnumerable<string>).ElementType());
            Assert.Equal(typeof(string), typeof(IQueryable<string>).ElementType());
        }

        [Fact]
        public void Element_type_should_return_input_type_when_not_sequence_type()
        {
            Assert.Equal(typeof(string), typeof(string));
        }

        [Fact]
        public void Get_any_property_returns_any_property()
        {
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ElDiabloEnElOjo").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ANightIn").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("MySister").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TinyTears").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Seaweed").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("VertrauenII").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TalkToMe").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("NoMoreAffairs").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Singing").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TravellingLight").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("CherryBlossoms").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ShesGone").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Mistakes").DeclaringType);
            Assert.Null(typeof(TindersticksII).GetAnyProperty("VertrauenIII"));
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("SleepySong").DeclaringType);

            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ElDiabloEnElOjo").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ANightIn").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("MySister").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TinyTears").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("Seaweed").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("VertrauenII").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TalkToMe").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("NoMoreAffairs").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("Singing").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TravellingLight").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("CherryBlossoms").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ShesGone").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksIIVinyl).GetAnyProperty("Mistakes").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("VertrauenIII").DeclaringType);
            Assert.Throws<AmbiguousMatchException>(() => typeof(TindersticksIICd).GetAnyProperty("SleepySong"));

            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("ANightIn").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("MySister").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TinyTears").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Seaweed").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("VertrauenII").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TalkToMe").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("NoMoreAffairs").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Singing").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TravellingLight").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("CherryBlossoms").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIICd).GetAnyProperty("ShesGone").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Mistakes").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("VertrauenIII").DeclaringType);
            Assert.Throws<AmbiguousMatchException>(() => typeof(TindersticksIICd).GetAnyProperty("SleepySong"));
        }

        // ReSharper disable InconsistentNaming
        public class TindersticksII
        {
            public virtual int ElDiabloEnElOjo { get; set; }
            internal virtual int ANightIn { get; set; }
            private int MySister { get; set; }
            protected virtual int TinyTears { get; set; }
            public virtual int SnowyInFSharpMinor { get; private set; }
            public virtual int Seaweed { private get; set; }
            public virtual int VertrauenII { get; protected set; }
            public virtual int TalkToMe { protected get; set; }

            public virtual int NoMoreAffairs
            {
                get { return 1995; }
            }

            public virtual int Singing
            {
                set { }
            }

            public virtual int TravellingLight { get; set; }
            public int CherryBlossoms { get; set; }
            public int ShesGone { get; set; }
            public virtual int Mistakes { get; set; }
            public int SleepySong { get; set; }
        }

        public class TindersticksIIVinyl : TindersticksII
        {
            public override int ElDiabloEnElOjo { get; set; }
            internal override int ANightIn { get; set; }
            private int MySister { get; set; }
            protected override int TinyTears { get; set; }

            public override int SnowyInFSharpMinor
            {
                get { return 1995; }
            }

            public override int Seaweed
            {
                set { }
            }

            public override int VertrauenII { get; protected set; }
            public override int TalkToMe { protected get; set; }

            public override int NoMoreAffairs
            {
                get { return 1995; }
            }

            public override int Singing
            {
                set { }
            }

            public new virtual int TravellingLight { get; set; }
            public new virtual int CherryBlossoms { get; set; }
            public new int ShesGone { get; set; }
            public virtual int VertrauenIII { get; set; }
            public new static int SleepySong { get; set; }
        }

        public class TindersticksIICd : TindersticksIIVinyl
        {
            internal override int ANightIn { get; set; }
            private int MySister { get; set; }
            protected override int TinyTears { get; set; }

            public override int SnowyInFSharpMinor
            {
                get { return 1995; }
            }

            public override int Seaweed
            {
                set { }
            }

            public override int VertrauenII { get; protected set; }
            public override int TalkToMe { protected get; set; }

            public override int NoMoreAffairs
            {
                get { return 1995; }
            }

            public override int Singing
            {
                set { }
            }

            public override int TravellingLight { get; set; }
            public override int CherryBlossoms { get; set; }
            public override int Mistakes { get; set; }
            public override int VertrauenIII { get; set; }
            public new static int SleepySong { get; set; }
        }

        // ReSharper restore InconsistentNaming
    }
}
