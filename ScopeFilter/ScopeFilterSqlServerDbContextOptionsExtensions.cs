using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ScopeFilter
{
    public static class ScopeFilterSqlServerDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder HasScopeFilter(this DbContextOptionsBuilder builder)
        {
            var provider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .Replace(new ServiceDescriptor(typeof(IEntityQueryModelVisitorFactory), typeof(ScopeFilterRelationalQueryModelVisitorFactory), ServiceLifetime.Scoped))
                .AddScoped<ScopeFilterStore>()
                .BuildServiceProvider();

            return builder.UseInternalServiceProvider(provider);
        }

        public static DbContextOptionsBuilder UseScopeFilter(this DbContextOptionsBuilder builder)
        {
            return builder.ReplaceService<IEntityQueryModelVisitorFactory, ApplicationContextScopeFilterRelationalQueryModelVisitorFactory>();
        }
    }
}
