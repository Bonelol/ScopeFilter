using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace ScopeFilter
{
    /// <inheritdoc />
    /// <summary>
    /// Modified based on ModelExpressionApplyingExpressionVisitor in Microsoft's EntityFramework Core
    /// </summary>
    public class ScopeFilterModelExpressionApplyingExpressionVisitor : ModelExpressionApplyingExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQueryModelGenerator _queryModelGenerator;
        private readonly IScopeFilterStore _store;
        private static readonly MethodInfo _whereMethod
            = typeof(Queryable)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(Queryable.Where))
                .Single(
                    mi => mi.GetParameters().Length == 2
                          && mi.GetParameters()[1].ParameterType
                              .GetGenericArguments()[0]
                              .GetGenericArguments().Length == 2);

        public ScopeFilterModelExpressionApplyingExpressionVisitor(QueryCompilationContext queryCompilationContext,
            IQueryModelGenerator queryModelGenerator, EntityQueryModelVisitor entityQueryModelVisitor, IScopeFilterStore store)
            : base(queryCompilationContext, queryModelGenerator, entityQueryModelVisitor)
        {
            _queryCompilationContext = queryCompilationContext;
            _queryModelGenerator = queryModelGenerator;
            _store = store;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (_store != null && constantExpression.IsEntityQueryable())
            {
                var type = ((IQueryable)constantExpression.Value).ElementType;
                var entityType = _queryCompilationContext.Model.FindEntityType(type)?.RootType();

                if (entityType != null && _store.Dictionary.TryGetValue(type, out var lambda))
                {
                    Expression newExpression = constantExpression;

                    var parameterizedFilter
                        = (LambdaExpression)_queryModelGenerator
                            .ExtractParameters(
                                _queryCompilationContext.Logger,
                                lambda,
                                new Parameters(this.ContextParameters),
                                parameterize: false,
                                generateContextAccessors: true);

                    var oldParameterExpression = parameterizedFilter.Parameters[0];
                    var newParameterExpression = Expression.Parameter(type, oldParameterExpression.Name);

                    var predicateExpression
                        = ReplacingExpressionVisitor
                            .Replace(
                                oldParameterExpression,
                                newParameterExpression,
                                parameterizedFilter.Body);

                    var whereExpression
                        = Expression.Call(
                            _whereMethod.MakeGenericMethod(type),
                            newExpression,
                            Expression.Lambda(
                                predicateExpression,
                                newParameterExpression));

                    var subQueryModel = _queryModelGenerator.ParseQuery(whereExpression);

                    newExpression = new SubQueryExpression(subQueryModel);

                    return newExpression;
                }
            }

            return base.VisitConstant(constantExpression);
        }

        private sealed class Parameters : IParameterValues
        {
            private readonly IDictionary<string, object> _parameterValues;

            public Parameters(IReadOnlyDictionary<string, object> parameterValues)
            {
                _parameterValues = (IDictionary<string, object>)parameterValues;
            }

            public IReadOnlyDictionary<string, object> ParameterValues
                => (IReadOnlyDictionary<string, object>)_parameterValues;

            public void AddParameter(string name, object value)
            {
                _parameterValues.Add(name, value);
            }

            public object RemoveParameter(string name)
            {
                var value = _parameterValues[name];

                _parameterValues.Remove(name);

                return value;
            }

            public void SetParameter(string name, object value)
            {
                _parameterValues[name] = value;
            }
        }
    }
}
