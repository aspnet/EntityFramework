﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public static class QueryableMethodProvider
    {
        private static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2)
            => type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Expression<>)
                && type.GetGenericArguments()[0].IsGenericType
                && type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;

        public static MethodInfo AsQueryableMethodInfo { get; }
        public static MethodInfo CastMethodInfo { get; }
        public static MethodInfo OfTypeMethodInfo { get; }

        public static MethodInfo AllMethodInfo { get; }
        public static MethodInfo AnyWithoutPredicateMethodInfo { get; }
        public static MethodInfo AnyWithPredicateMethodInfo { get; }
        public static MethodInfo ContainsMethodInfo { get; }

        public static MethodInfo ConcatMethodInfo { get; }
        public static MethodInfo ExceptMethodInfo { get; }
        public static MethodInfo IntersectMethodInfo { get; }
        public static MethodInfo UnionMethodInfo { get; }

        public static MethodInfo CountWithoutPredicateMethodInfo { get; }
        public static MethodInfo CountWithPredicateMethodInfo { get; }
        public static MethodInfo LongCountWithoutPredicateMethodInfo { get; }
        public static MethodInfo LongCountWithPredicateMethodInfo { get; }
        public static MethodInfo MinWithSelectorMethodInfo { get; }
        public static MethodInfo MinWithoutSelectorMethodInfo { get; }
        public static MethodInfo MaxWithSelectorMethodInfo { get; }
        public static MethodInfo MaxWithoutSelectorMethodInfo { get; }

        public static MethodInfo ElementAtMethodInfo { get; }
        public static MethodInfo ElementAtOrDefaultMethodInfo { get; }
        public static MethodInfo FirstWithoutPredicateMethodInfo { get; }
        public static MethodInfo FirstWithPredicateMethodInfo { get; }
        public static MethodInfo FirstOrDefaultWithoutPredicateMethodInfo { get; }
        public static MethodInfo FirstOrDefaultWithPredicateMethodInfo { get; }
        public static MethodInfo SingleWithoutPredicateMethodInfo { get; }
        public static MethodInfo SingleWithPredicateMethodInfo { get; }
        public static MethodInfo SingleOrDefaultWithoutPredicateMethodInfo { get; }
        public static MethodInfo SingleOrDefaultWithPredicateMethodInfo { get; }
        public static MethodInfo LastWithoutPredicateMethodInfo { get; }
        public static MethodInfo LastWithPredicateMethodInfo { get; }
        public static MethodInfo LastOrDefaultWithoutPredicateMethodInfo { get; }
        public static MethodInfo LastOrDefaultWithPredicateMethodInfo { get; }

        public static MethodInfo DistinctMethodInfo { get; }
        public static MethodInfo ReverseMethodInfo { get; }
        public static MethodInfo WhereMethodInfo { get; }
        public static MethodInfo SelectMethodInfo { get; }
        public static MethodInfo SkipMethodInfo { get; }
        public static MethodInfo TakeMethodInfo { get; }
        public static MethodInfo SkipWhileMethodInfo { get; }
        public static MethodInfo TakeWhileMethodInfo { get; }
        public static MethodInfo OrderByMethodInfo { get; }
        public static MethodInfo OrderByDescendingMethodInfo { get; }
        public static MethodInfo ThenByMethodInfo { get; }
        public static MethodInfo ThenByDescendingMethodInfo { get; }
        public static MethodInfo DefaultIfEmptyWithoutArgumentMethodInfo { get; }
        public static MethodInfo DefaultIfEmptyWithArgumentMethodInfo { get; }

        public static MethodInfo JoinMethodInfo { get; }
        public static MethodInfo GroupJoinMethodInfo { get; }
        public static MethodInfo SelectManyWithCollectionSelectorMethodInfo { get; }
        public static MethodInfo SelectManyWithoutCollectionSelectorMethodInfo { get; }

        public static MethodInfo GroupByWithKeySelectorMethodInfo { get; }
        public static MethodInfo GroupByWithKeyElementSelectorMethodInfo { get; }
        public static MethodInfo GroupByWithKeyElementResultSelectorMethodInfo { get; }
        public static MethodInfo GroupByWithKeyResultSelectorMethodInfo { get; }

        public static IDictionary<Type, MethodInfo> SumWithoutSelectorMethodInfos { get; }
        public static IDictionary<Type, MethodInfo> SumWithSelectorMethodInfos { get; }
        public static IDictionary<Type, MethodInfo> AverageWithoutSelectorMethodInfos { get; }
        public static IDictionary<Type, MethodInfo> AverageWithSelectorMethodInfos { get; }

        public static bool IsSumWithoutSelectorMethodInfo(MethodInfo methodInfo)
            => SumWithoutSelectorMethodInfos.Values.Contains(methodInfo);
        public static bool IsSumWithSelectorMethodInfo(MethodInfo methodInfo)
            => methodInfo.IsGenericMethod
               && SumWithSelectorMethodInfos.Values.Contains(methodInfo.GetGenericMethodDefinition());

        public static bool IsAverageWithoutSelectorMethodInfo(MethodInfo methodInfo)
            => AverageWithoutSelectorMethodInfos.Values.Contains(methodInfo);
        public static bool IsAverageWithSelectorMethodInfo(MethodInfo methodInfo)
            => methodInfo.IsGenericMethod
               && AverageWithSelectorMethodInfos.Values.Contains(methodInfo.GetGenericMethodDefinition());

        static QueryableMethodProvider()
        {
            var queryableMethods = typeof(Queryable).GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

            AsQueryableMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.AsQueryable) && mi.IsGenericMethod && mi.GetParameters().Length == 1);
            CastMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Cast) && mi.GetParameters().Length == 1);
            OfTypeMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.OfType) && mi.GetParameters().Length == 1);

            AllMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.All) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            AnyWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Any) && mi.GetParameters().Length == 1);
            AnyWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Any) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            ContainsMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Contains) && mi.GetParameters().Length == 2);

            ConcatMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Concat) && mi.GetParameters().Length == 2);
            ExceptMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Except) && mi.GetParameters().Length == 2);
            IntersectMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Intersect) && mi.GetParameters().Length == 2);
            UnionMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Union) && mi.GetParameters().Length == 2);

            CountWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Count) && mi.GetParameters().Length == 1);
            CountWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Count) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LongCountWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LongCount) && mi.GetParameters().Length == 1);
            LongCountWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LongCount) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MinWithSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Min) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MinWithoutSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Min) && mi.GetParameters().Length == 1);
            MaxWithSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Max) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MaxWithoutSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Max) && mi.GetParameters().Length == 1);

            ElementAtMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ElementAt) && mi.GetParameters().Length == 2);
            ElementAtOrDefaultMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ElementAtOrDefault) && mi.GetParameters().Length == 2);
            FirstWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First) && mi.GetParameters().Length == 1);
            FirstWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            FirstOrDefaultWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault) && mi.GetParameters().Length == 1);
            FirstOrDefaultWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SingleWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Single) && mi.GetParameters().Length == 1);
            SingleWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Single) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SingleOrDefaultWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SingleOrDefault) && mi.GetParameters().Length == 1);
            SingleOrDefaultWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SingleOrDefault) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LastWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Last) && mi.GetParameters().Length == 1);
            LastWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Last) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LastOrDefaultWithoutPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LastOrDefault) && mi.GetParameters().Length == 1);
            LastOrDefaultWithPredicateMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LastOrDefault) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            DistinctMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Distinct) && mi.GetParameters().Length == 1);
            ReverseMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Reverse) && mi.GetParameters().Length == 1);
            WhereMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Where) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SelectMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Select) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SkipMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Skip) && mi.GetParameters().Length == 2);
            TakeMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Take) && mi.GetParameters().Length == 2);
            SkipWhileMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SkipWhile) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            TakeWhileMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.TakeWhile) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            OrderByMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.OrderBy) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            OrderByDescendingMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.OrderByDescending) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            ThenByMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ThenBy) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            ThenByDescendingMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.ThenByDescending) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            DefaultIfEmptyWithoutArgumentMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.DefaultIfEmpty) && mi.GetParameters().Length == 1);
            DefaultIfEmptyWithArgumentMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.DefaultIfEmpty) && mi.GetParameters().Length == 2);

            JoinMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Join) && mi.GetParameters().Length == 5);
            GroupJoinMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupJoin) && mi.GetParameters().Length == 5);
            SelectManyWithCollectionSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SelectMany) && mi.GetParameters().Length == 3 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SelectManyWithoutCollectionSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SelectMany) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            GroupByWithKeySelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy) && mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            GroupByWithKeyElementSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy) && mi.GetParameters().Length == 3 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType) && IsExpressionOfFunc(mi.GetParameters()[2].ParameterType));
            GroupByWithKeyElementResultSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy) && mi.GetParameters().Length == 4 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType) && IsExpressionOfFunc(mi.GetParameters()[2].ParameterType) && IsExpressionOfFunc(mi.GetParameters()[3].ParameterType, 3));
            GroupByWithKeyResultSelectorMethodInfo = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.GroupBy) && mi.GetParameters().Length == 3 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType) && IsExpressionOfFunc(mi.GetParameters()[2].ParameterType, 3));

            MethodInfo GetSumOrAverageWithoutSelector<T>(string methodName)
                => queryableMethods.Single(
                    mi => mi.Name == methodName
                          && mi.GetParameters().Length == 1
                          && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0] == typeof(T));

            bool HasSelector<T>(Type type)
                => type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Expression<>)
                   && type.GetGenericArguments()[0].IsGenericType
                   && type.GetGenericArguments()[0].GetGenericArguments().Length == 2
                   && type.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(T);

            MethodInfo GetSumOrAverageWithSelector<T>(string methodName)
                => queryableMethods.Single(
                    mi => mi.Name == methodName
                          && mi.GetParameters().Length == 2
                          && HasSelector<T>(mi.GetParameters()[1].ParameterType));

            SumWithoutSelectorMethodInfos = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithoutSelector<decimal>(nameof(Queryable.Sum)) },
                { typeof(long), GetSumOrAverageWithoutSelector<long>(nameof(Queryable.Sum)) },
                { typeof(int), GetSumOrAverageWithoutSelector<int>(nameof(Queryable.Sum)) },
                { typeof(double), GetSumOrAverageWithoutSelector<double>(nameof(Queryable.Sum)) },
                { typeof(float), GetSumOrAverageWithoutSelector<float>(nameof(Queryable.Sum)) },
                { typeof(decimal?), GetSumOrAverageWithoutSelector<decimal?>(nameof(Queryable.Sum)) },
                { typeof(long?), GetSumOrAverageWithoutSelector<long?>(nameof(Queryable.Sum)) },
                { typeof(int?), GetSumOrAverageWithoutSelector<int?>(nameof(Queryable.Sum)) },
                { typeof(double?), GetSumOrAverageWithoutSelector<double?>(nameof(Queryable.Sum)) },
                { typeof(float?), GetSumOrAverageWithoutSelector<float?>(nameof(Queryable.Sum)) },
            };

            SumWithSelectorMethodInfos = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithSelector<decimal>(nameof(Queryable.Sum)) },
                { typeof(long), GetSumOrAverageWithSelector<long>(nameof(Queryable.Sum)) },
                { typeof(int), GetSumOrAverageWithSelector<int>(nameof(Queryable.Sum)) },
                { typeof(double), GetSumOrAverageWithSelector<double>(nameof(Queryable.Sum)) },
                { typeof(float), GetSumOrAverageWithSelector<float>(nameof(Queryable.Sum)) },
                { typeof(decimal?), GetSumOrAverageWithSelector<decimal?>(nameof(Queryable.Sum)) },
                { typeof(long?), GetSumOrAverageWithSelector<long?>(nameof(Queryable.Sum)) },
                { typeof(int?), GetSumOrAverageWithSelector<int?>(nameof(Queryable.Sum)) },
                { typeof(double?), GetSumOrAverageWithSelector<double?>(nameof(Queryable.Sum)) },
                { typeof(float?), GetSumOrAverageWithSelector<float?>(nameof(Queryable.Sum)) },
            };

            AverageWithoutSelectorMethodInfos = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithoutSelector<decimal>(nameof(Queryable.Average)) },
                { typeof(long), GetSumOrAverageWithoutSelector<long>(nameof(Queryable.Average)) },
                { typeof(int), GetSumOrAverageWithoutSelector<int>(nameof(Queryable.Average)) },
                { typeof(double), GetSumOrAverageWithoutSelector<double>(nameof(Queryable.Average)) },
                { typeof(float), GetSumOrAverageWithoutSelector<float>(nameof(Queryable.Average)) },
                { typeof(decimal?), GetSumOrAverageWithoutSelector<decimal?>(nameof(Queryable.Average)) },
                { typeof(long?), GetSumOrAverageWithoutSelector<long?>(nameof(Queryable.Average)) },
                { typeof(int?), GetSumOrAverageWithoutSelector<int?>(nameof(Queryable.Average)) },
                { typeof(double?), GetSumOrAverageWithoutSelector<double?>(nameof(Queryable.Average)) },
                { typeof(float?), GetSumOrAverageWithoutSelector<float?>(nameof(Queryable.Average)) },
            };

            AverageWithSelectorMethodInfos = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithSelector<decimal>(nameof(Queryable.Average)) },
                { typeof(long), GetSumOrAverageWithSelector<long>(nameof(Queryable.Average)) },
                { typeof(int), GetSumOrAverageWithSelector<int>(nameof(Queryable.Average)) },
                { typeof(double), GetSumOrAverageWithSelector<double>(nameof(Queryable.Average)) },
                { typeof(float), GetSumOrAverageWithSelector<float>(nameof(Queryable.Average)) },
                { typeof(decimal?), GetSumOrAverageWithSelector<decimal?>(nameof(Queryable.Average)) },
                { typeof(long?), GetSumOrAverageWithSelector<long?>(nameof(Queryable.Average)) },
                { typeof(int?), GetSumOrAverageWithSelector<int?>(nameof(Queryable.Average)) },
                { typeof(double?), GetSumOrAverageWithSelector<double?>(nameof(Queryable.Average)) },
                { typeof(float?), GetSumOrAverageWithSelector<float?>(nameof(Queryable.Average)) },
            };
        }
    }
}
