// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query
{
    public abstract class AsyncEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        protected AsyncEntityQueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
            : base(new AsyncLinqOperatorProvider(), parentQueryModelVisitor)
        {
        }

        public new Func<QueryContext, IAsyncEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.Call(
                        _taskToSequenceShim.MakeGenericMethod(typeof(TResult)),
                        _expression);
            }

            return Expression
                .Lambda<Func<QueryContext, IAsyncEnumerable<TResult>>>(_expression, _queryContextParameter)
                .Compile();
        }

        private static readonly MethodInfo _taskToSequenceShim
            = typeof(AsyncEntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod("TaskToSequenceShim");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> TaskToSequenceShim<T>(Task<T> task)
        {
            return new TaskResultAsyncEnumerable<T>(task);
        }

        private static readonly Dictionary<Type, Func<Expression, Type, ResultOperatorBase, Expression>>
            _asyncHandlers = new Dictionary<Type, Func<Expression, Type, ResultOperatorBase, Expression>>
                {
                    { typeof(AnyResultOperator), (e, t, r) => ProcessResultOperator(e, t, (AnyResultOperator)r) },
                    { typeof(CountResultOperator), (e, t, r) => ProcessResultOperator(e, t, (CountResultOperator)r) }
                };

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            var streamedDataInfo
                = resultOperator.GetOutputDataInfo(_streamedSequenceInfo);

            Func<Expression, Type, ResultOperatorBase, Expression> asyncHandler;
            if (!_asyncHandlers.TryGetValue(resultOperator.GetType(), out asyncHandler))
            {
                // TODO: Implement the rest...
                throw new NotImplementedException();
            }

            _expression
                = asyncHandler(_expression, _streamedSequenceInfo.ResultItemType, resultOperator);

            _streamedSequenceInfo = streamedDataInfo as StreamedSequenceInfo;
        }

        private static Expression ProcessResultOperator(Expression expression, Type expressionItemType, AnyResultOperator _)
        {
            return Expression.Call(_anyAsyncShim.MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _anyAsyncShim
            = typeof(AsyncEntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod("AnyAsyncShim");

        [UsedImplicitly]
        private static Task<bool> AnyAsyncShim<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Any();
        }

        private static Expression ProcessResultOperator(Expression expression, Type expressionItemType, CountResultOperator _)
        {
            return Expression.Call(_countAsyncShim.MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _countAsyncShim
            = typeof(AsyncEntityQueryModelVisitor)
                .GetTypeInfo().GetDeclaredMethod("CountAsyncShim");

        [UsedImplicitly]
        private static Task<int> CountAsyncShim<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Count();
        }
    }
}
