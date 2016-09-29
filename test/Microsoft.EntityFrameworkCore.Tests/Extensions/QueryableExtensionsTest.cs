// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Moq;
using Xunit;

// ReSharper disable RedundantArgumentDefaultValue
namespace Microsoft.EntityFrameworkCore.Tests.Extensions
{
    public class QueryableExtensionsTest
    {
        [Fact]
        public void Include_on_non_ef_queryable_is_no_op()
        {
            var q = new List<Customer>().AsQueryable();
            var q2 = q.Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ToList();

            Assert.Equal(0, q2.Count);
        }

        // ReSharper disable MethodSupportsCancellation

        [Fact]
        public void Extension_methods_call_provider_ExecuteAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(default(CancellationToken)));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, default(CancellationToken)));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.CountAsync(default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.CountAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.CountAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MaxAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MinAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MinAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SumAsync(default(CancellationToken)));
            VerifyProducedExpression<int, int>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long, long>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long, long>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(default(CancellationToken)));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        }

        private static void VerifyProducedExpression<TElement, TResult>(
            Expression<Func<IQueryable<TElement>, Task<TResult>>> testExpression)
        {
            var queryableMock = new Mock<IQueryable<TElement>>();
            var providerMock = new Mock<IAsyncQueryProvider>();

            providerMock
                .Setup(m => m.ExecuteAsync<TResult>(It.IsAny<Expression>(), It.IsAny<CancellationToken>()))
                .Returns<Expression, CancellationToken>(
                    (e, ct) =>
                        {
                            var expectedMethodCall = (MethodCallExpression)testExpression.Body;
                            var actualMethodCall = (MethodCallExpression)e;

                            Assert.Equal(
                                expectedMethodCall.Method.Name,
                                actualMethodCall.Method.Name + "Async");

                            var lastArgument =
                                expectedMethodCall.Arguments[expectedMethodCall.Arguments.Count - 1] as MemberExpression;

                            var cancellationTokenPresent
                                = (lastArgument != null) && (lastArgument.Type == typeof(CancellationToken));

                            if (cancellationTokenPresent)
                            {
                                Assert.NotEqual(ct, CancellationToken.None);
                            }
                            else
                            {
                                Assert.Equal(ct, CancellationToken.None);
                            }

                            for (var i = 1; i < expectedMethodCall.Arguments.Count - 1; i++)
                            {
                                var expectedArgument = expectedMethodCall.Arguments[i];
                                var actualArgument = actualMethodCall.Arguments[i];

                                Assert.Equal(expectedArgument.ToString(), actualArgument.ToString());
                            }

                            return Task.FromResult(default(TResult));
                        });

            queryableMock
                .Setup(m => m.Provider)
                .Returns(providerMock.Object);

            queryableMock
                .Setup(m => m.Expression)
                .Returns(Expression.Constant(queryableMock.Object, typeof(IQueryable<TElement>)));

            testExpression.Compile()(queryableMock.Object);
        }

        [Fact]
        public async Task Extension_methods_do_not_throw_on_non_async_source()
        {
            await SourceFromList().AllAsync(e => true);
            await SourceFromList().AllAsync(e => true, new CancellationToken());
            await SourceFromList().AnyAsync();
            await SourceFromList().AnyAsync(e => true);
            await SourceFromList<int>().AverageAsync();
            await SourceFromList<int>().AverageAsync(new CancellationToken());
            await SourceFromList<int>().AverageAsync(e => e);
            await SourceFromList<int>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<int?>().AverageAsync();
            await SourceFromList<int?>().AverageAsync(new CancellationToken());
            await SourceFromList<int?>().AverageAsync(e => e);
            await SourceFromList<int?>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<long>().AverageAsync();
            await SourceFromList<long>().AverageAsync(new CancellationToken());
            await SourceFromList<long>().AverageAsync(e => e);
            await SourceFromList<long>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<long?>().AverageAsync();
            await SourceFromList<long?>().AverageAsync(new CancellationToken());
            await SourceFromList<long?>().AverageAsync(e => e);
            await SourceFromList<long?>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<float>().AverageAsync();
            await SourceFromList<float>().AverageAsync(new CancellationToken());
            await SourceFromList<float>().AverageAsync(e => e);
            await SourceFromList<float>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<float?>().AverageAsync();
            await SourceFromList<float?>().AverageAsync(new CancellationToken());
            await SourceFromList<float?>().AverageAsync(e => e);
            await SourceFromList<float?>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<double>().AverageAsync();
            await SourceFromList<double>().AverageAsync(new CancellationToken());
            await SourceFromList<double>().AverageAsync(e => e);
            await SourceFromList<double>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<double?>().AverageAsync();
            await SourceFromList<double?>().AverageAsync(new CancellationToken());
            await SourceFromList<double?>().AverageAsync(e => e);
            await SourceFromList<double?>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<decimal>().AverageAsync();
            await SourceFromList<decimal>().AverageAsync(new CancellationToken());
            await SourceFromList<decimal>().AverageAsync(e => e);
            await SourceFromList<decimal>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList<decimal?>().AverageAsync();
            await SourceFromList<decimal?>().AverageAsync(new CancellationToken());
            await SourceFromList<decimal?>().AverageAsync(e => e);
            await SourceFromList<decimal?>().AverageAsync(e => e, new CancellationToken());
            await SourceFromList().ContainsAsync(0);
            await SourceFromList().ContainsAsync(0, new CancellationToken());
            await SourceFromList().CountAsync();
            await SourceFromList().CountAsync(e => true);
            await SourceFromList().FirstAsync();
            await SourceFromList().FirstAsync(new CancellationToken());
            await SourceFromList().FirstAsync(e => true);
            await SourceFromList().FirstAsync(e => true, new CancellationToken());
            await SourceFromList().FirstOrDefaultAsync();
            await SourceFromList().FirstOrDefaultAsync(new CancellationToken());
            await SourceFromList().FirstOrDefaultAsync(e => true);
            await SourceFromList().FirstOrDefaultAsync(e => true, new CancellationToken());
            await SourceFromList().ForEachAsync(e => { });
            await SourceFromList().ForEachAsync(e => { }, new CancellationToken());
            await SourceFromList().LoadAsync();
            await SourceFromList().LoadAsync(new CancellationToken());
            await SourceFromList().LongCountAsync();
            await SourceFromList().LongCountAsync(new CancellationToken());
            await SourceFromList().LongCountAsync(e => true);
            await SourceFromList().LongCountAsync(e => true, new CancellationToken());
            await SourceFromList().MaxAsync();
            await SourceFromList().MaxAsync(new CancellationToken());
            await SourceFromList().MaxAsync(e => e);
            await SourceFromList().MaxAsync(e => e, new CancellationToken());
            await SourceFromList().MinAsync();
            await SourceFromList().MinAsync(new CancellationToken());
            await SourceFromList().MinAsync(e => e);
            await SourceFromList().MinAsync(e => e, new CancellationToken());
            await SourceFromList().SingleAsync();
            await SourceFromList().SingleAsync(e => true);
            await SourceFromList().SingleOrDefaultAsync();
            await SourceFromList().SingleOrDefaultAsync(e => true);
            await SourceFromList<int>().SumAsync();
            await SourceFromList<int>().SumAsync(e => e);
            await SourceFromList<int?>().SumAsync();
            await SourceFromList<int?>().SumAsync(e => e);
            await SourceFromList<long>().SumAsync();
            await SourceFromList<long>().SumAsync(e => e);
            await SourceFromList<long?>().SumAsync();
            await SourceFromList<long?>().SumAsync(e => e);
            await SourceFromList<float>().SumAsync();
            await SourceFromList<float>().SumAsync(e => e);
            await SourceFromList<float?>().SumAsync();
            await SourceFromList<float?>().SumAsync(e => e);
            await SourceFromList<double>().SumAsync();
            await SourceFromList<double>().SumAsync(e => e);
            await SourceFromList<double?>().SumAsync();
            await SourceFromList<double?>().SumAsync(e => e);
            await SourceFromList<decimal>().SumAsync();
            await SourceFromList<decimal>().SumAsync(e => e);
            await SourceFromList<decimal?>().SumAsync();
            await SourceFromList<decimal?>().SumAsync(e => e);
            await SourceFromList().ToDictionaryAsync(e => e);
            await SourceFromList().ToDictionaryAsync(e => e, e => e);
            await SourceFromList().ToDictionaryAsync(e => e, new Mock<IEqualityComparer<int>>().Object);
            await SourceFromList().ToDictionaryAsync(e => e, new Mock<IEqualityComparer<int>>().Object, new CancellationToken());
            await SourceFromList().ToDictionaryAsync(e => e, e => e, new Mock<IEqualityComparer<int>>().Object);
            await SourceFromList().ToDictionaryAsync(e => e, e => e, new Mock<IEqualityComparer<int>>().Object, new CancellationToken());
            await SourceFromList().ToListAsync();
        }


        [Fact]
        public void Extension_methods_throw_on_non_async_source()
        {

            Assert.Equal(
                CoreStrings.IQueryableNotAsync(typeof(int)),
                Assert.Throws<InvalidOperationException>(() => Source().AsAsyncEnumerable()).Message);
        }

        private static IQueryable<T> Source<T>() => new Mock<IQueryable<T>>().Object;

        private static IQueryable<int> Source() => Source<int>();

        private static IQueryable<T> SourceFromList<T>()
        {
            if (ReferenceEquals(default(T), null))
            {
                return new List<T>().AsQueryable();
            }

            return new List<T> { default(T) }.AsQueryable();
        }

        private static IQueryable<int> SourceFromList() => SourceFromList<int>();

        private static async Task SourceNonAsyncQueryableTest(Func<Task> test) =>
            Assert.Equal(
                CoreStrings.IQueryableProviderNotAsync,
                (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message);

        private static async Task SourceNonAsyncEnumerableTest<T>(Func<Task> test)
            => Assert.Equal(
                CoreStrings.IQueryableNotAsync(typeof(T)),
                (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message);

        [Fact]
        public async Task Extension_methods_validate_arguments()
        {
            // ReSharper disable AssignNullToNotNullAttribute

            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.FirstAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.FirstAsync<int>(null, s => true));
            await ArgumentNullTest("predicate", () => Source().FirstAsync(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.FirstOrDefaultAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.FirstOrDefaultAsync<int>(null, s => true));
            await ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.SingleAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.SingleAsync<int>(null, s => true));
            await ArgumentNullTest("predicate", () => Source().SingleAsync(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.SingleOrDefaultAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.SingleOrDefaultAsync<int>(null, s => true));
            await ArgumentNullTest("predicate", () => Source().SingleOrDefaultAsync(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.ContainsAsync(null, 1));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.ContainsAsync(null, 1, new CancellationToken()));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.AnyAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.AnyAsync<int>(null, s => true));
            await ArgumentNullTest("predicate", () => Source().AnyAsync(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.AllAsync<int>(null, s => true));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.AllAsync<int>(null, s => true, new CancellationToken()));
            await ArgumentNullTest("predicate", () => Source().AllAsync(null));
            await ArgumentNullTest("predicate", () => Source().AllAsync(null, new CancellationToken()));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.CountAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.CountAsync<int>(null, s => true));
            await ArgumentNullTest("predicate", () => Source().CountAsync(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.LongCountAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.LongCountAsync<int>(null, new CancellationToken()));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.LongCountAsync<int>(null, s => true));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.LongCountAsync<int>(null, s => true, new CancellationToken()));
            await ArgumentNullTest("predicate", () => Source().LongCountAsync(null));
            await ArgumentNullTest("predicate", () => Source().LongCountAsync(null, new CancellationToken()));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MinAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MinAsync<int>(null, new CancellationToken()));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MinAsync<int, bool>(null, s => true));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MinAsync<int, bool>(null, s => true, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null));
            await ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null, new CancellationToken()));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MaxAsync<int>(null));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MaxAsync<int>(null, new CancellationToken()));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MaxAsync<int, bool>(null, s => true));
            await ArgumentNullTest("source", () => EntityFrameworkQueryableExtensions.MaxAsync<int, bool>(null, s => true, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null));
            await ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync());
            await ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<int>().SumAsync((Expression<Func<int, int>>)null));
            await ArgumentNullTest("selector", () => Source<int?>().SumAsync((Expression<Func<int?, int>>)null));
            await ArgumentNullTest("selector", () => Source<long>().SumAsync((Expression<Func<long, int>>)null));
            await ArgumentNullTest("selector", () => Source<long?>().SumAsync((Expression<Func<long?, int>>)null));
            await ArgumentNullTest("selector", () => Source<float>().SumAsync((Expression<Func<float, int>>)null));
            await ArgumentNullTest("selector", () => Source<float?>().SumAsync((Expression<Func<float?, int>>)null));
            await ArgumentNullTest("selector", () => Source<double>().SumAsync((Expression<Func<double, int>>)null));
            await ArgumentNullTest("selector", () => Source<double?>().SumAsync((Expression<Func<double?, int>>)null));
            await ArgumentNullTest("selector", () => Source<decimal>().SumAsync((Expression<Func<decimal, int>>)null));
            await ArgumentNullTest("selector", () => Source<decimal?>().SumAsync((Expression<Func<decimal?, int>>)null));
            await ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync());
            await ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0));
            await ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null));
            await ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null));
            await ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null));
            await ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null));
            await ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null));
            await ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null));
            await ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null));
            await ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null));
            await ArgumentNullTest("selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null));
            await ArgumentNullTest("selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null, new CancellationToken()));
            await ArgumentNullTest("selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null));
            await ArgumentNullTest("selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null, new CancellationToken()));

            // ReSharper restore AssignNullToNotNullAttribute
        }

        private static async Task ArgumentNullTest(string paramName, Func<Task> test)
            => Assert.Equal(paramName, (await Assert.ThrowsAsync<ArgumentNullException>(test)).ParamName);

        // ReSharper restore MethodSupportsCancellation
    }
}
