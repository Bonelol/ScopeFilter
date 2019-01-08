# ScopeFilter

Apply filters to queries just like global filters, but filters can be not same within different requests.

How to use:

In Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{ 
    ...
    services.AddScoped<IScopeFilterStore, ScopeFilterStore>();
    services.AddDbContext<DbContext>(options => options.UseSqlServer("connection_string")
      .UseScopeFilter());

    ...
}
``` 

In controller
```csharp
var children = dbContext.Parent.IncludeWithFilter(p=>p.Children, c=>c.Active)
                               .ThenIncludeWithFilter(c=>c.Items, i=>i.ID > 100);
                               
public class CustomController : Controller
{
        private readonly CustomContext _context;
        private readonly IScopeFilterStore _store;
        
        public CustomController(CustomContext context, IScopeFilterStore store)
        {
            _context = context;
            _store = store;
            
            if(!User.IsInRole("Admin")
            {              
                _store.AddFilter<Message>(m => m.CreatedBy == User.Identity.Name);
            }
        }
        
        ...
        
        public IActionResult Get()
        {
            //if user is not admin, json result will only return messages created by user
            return Json(_context.Message.Take(10));
        }
        
        ...
}        
```                               

