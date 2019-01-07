using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ScopeFilter
{
    public interface IScopeFilterStore
    {
        Dictionary<Type, LambdaExpression> Dictionary { get; set; }

        void AddFilter<TEntity>(Expression<Func<TEntity, bool>> filter);
    }
}