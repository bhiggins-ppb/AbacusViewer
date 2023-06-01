using AbacusViewer.Services;
//using Autofac;
//using Autofac.Integration.Mvc;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddSession(s => s.IdleTimeout = TimeSpan.FromMinutes(30));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();


//WebHost.CreateDefaultBuilder(args)
//    .UseContentRoot(Directory.GetCurrentDirectory())
//    .UseStartup<Startup>()
//.Build().Run();

//var builder = new ContainerBuilder();
//builder.RegisterType<SingletonEventCollectorFactory>().WithParameter("maxQueueDepth", 50).SingleInstance();


//DependencyResolver.SetResolver(new AutofacDependencyResolver(builder.Build()));

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {

        /*services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });*/
        services.AddSession(s => s.IdleTimeout = TimeSpan.FromMinutes(30));
        services.AddMvc(option => option.EnableEndpointRouting = false);
        //services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddHttpContextAccessor();
        services.AddSingleton(x => new SingletonEventCollectorFactory(50));
    }

    public void Configure(IApplicationBuilder app, Microsoft.Extensions.Hosting.IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSession();
        app.UseStaticFiles();
        /*app.UseCors("AllowAll");
        app.UseAuthentication();*/

        app.UseMvc(routes => {
          routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
        //app.UseMvcWithDefaultRoute();

    }
}