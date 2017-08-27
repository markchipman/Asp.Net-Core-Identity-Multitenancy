using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AspNetCoreMultitenancy.Data;
using AspNetCoreMultitenancy.Models;
using AspNetCoreMultitenancy.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Core;
using OpenIddict.Models;

namespace AspNetCoreMultitenancy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict<Guid>();
            });

            // Register the Identity services.
            services.AddScoped<IRoleStore<ApplicationRole>, ApplicationRoleStore>();
            services.AddScoped<IUserStore<ApplicationUser>, ApplicationUserStore>();
            // change "TenantId-1" to name your tenant...
            services.AddScoped(serviceProvider => new ApplicationTenantIdProvider("TenantId-2"));
            services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
                {
                    o.User.RequireUniqueEmail = false;
                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredLength = 6;
                }).AddUserStore<ApplicationUserStore>()
                .AddDefaultTokenProviders();

            // Register the OAuth2 validation handler.
            services.AddAuthentication()
                .AddOAuthValidation();

            // Register the OpenIddict services.
            // Note: use the generic overload if you need
            // to replace the default OpenIddict entities.
            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the token endpoint (required to use the password flow).
                options.EnableTokenEndpoint("/connect/token");

                // Allow client applications to use the grant_type=password flow.
                options.AllowPasswordFlow();

                //// Enable the authorization and token endpoints (required to use the code flow).
                //options.EnableAuthorizationEndpoint("/connect/authorize")
                //    .EnableTokenEndpoint("/connect/token");

                //// Allow client applications to use the code flow.
                //options.AllowAuthorizationCodeFlow();

                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();
            });
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            InitializeAsync(app.ApplicationServices, CancellationToken.None).GetAwaiter().GetResult();
        }
        private async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync(cancellationToken);
                try
                {
                    await context.Database.ExecuteSqlCommandAsync("drop index UserNameIndex on AspNetUsers", cancellationToken);
                }
                catch
                {
                    // ignored
                }
                try
                {
                    await context.Database.ExecuteSqlCommandAsync("drop index EmailIndex on AspNetUsers", cancellationToken);
                }
                catch
                {
                    // ignored
                }
                try
                {
                    await context.Database.ExecuteSqlCommandAsync("drop index RoleNameIndex on AspNetRoles", cancellationToken);
                }
                catch
                {
                    // ignored
                }
                // Seed the database with the sample data....
            }
        }
    }
}
