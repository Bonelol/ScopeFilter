using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;

namespace ScopeFilter
{
    public class ApplicationContextScopeFilterRelationalQueryModelVisitorFactory : RelationalQueryModelVisitorFactory
    {
        private readonly IScopeFilterStore _store;

        public ApplicationContextScopeFilterRelationalQueryModelVisitorFactory(
            EntityQueryModelVisitorDependencies dependencies,
            RelationalQueryModelVisitorDependencies relationalDependencies, ICurrentDbContext currentDbContext)
            : base(dependencies, relationalDependencies)
        {
            _store = currentDbContext.Context.GetService<IScopeFilterStore>();
            ;
        }

        public override EntityQueryModelVisitor Create(QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            var visitor = base.Create(queryCompilationContext, parentEntityQueryModelVisitor);
            var fieldInfo = typeof(EntityQueryModelVisitor).GetField("_modelExpressionApplyingExpressionVisitor",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(visitor,
                    new ScopeFilterModelExpressionApplyingExpressionVisitor(queryCompilationContext,
                        Dependencies.QueryModelGenerator, visitor, _store));
            }

            return visitor;
        }
    }
}
