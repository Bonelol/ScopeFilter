using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ScopeFilter
{
    public class ScopeFilterStore : IScopeFilterStore
    {
        public Dictionary<Type, LambdaExpression> Dictionary { get; set; } = new Dictionary<Type, LambdaExpression>();
        public void AddFilter<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            Dictionary.Add(typeof(TEntity), filter);
        }
    }
}
