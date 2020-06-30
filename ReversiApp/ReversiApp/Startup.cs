using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReversiApp.Models;
using ReversiApp.Services;
using ReversiApp.DAL;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ReversiApp
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
            services.AddControllersWithViews();
            services.AddDbContext<ReversiContext>(options =>
                options.ConfigureWarnings(b => b.Log(CoreEventId.ManyServiceProvidersCreatedWarning))
                    .UseSqlServer(Configuration.GetConnectionString("ReversiContextConnection")));

            services.AddIdentityCore<Speler>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ReversiContext>()
                .AddSignInManager<EmailSignInManager>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddCookie(o => {
                o.AccessDeniedPath = new PathString("/Account/AccessDenied");
            })
            .AddIdentityCookies(o => { });

            services.AddTransient<IEmailSender, AuthMessageSender>(i =>
                new AuthMessageSender(
                Configuration["EmailSender:Host"],
                Configuration.GetValue<int>("EmailSender:Port"),
                Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                Configuration["EmailSender:UserName"],
                Configuration["EmailSender:Password"]
            ));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<Speler> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            UpdateDatabase(app);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            DataSeeder.SeedData(userManager, roleManager);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Speler}/{action=Login}/{id?}");
                endpoints.MapControllerRoute(
                    name: "spel",
                    pattern: "{controller=Spel}/{action=Play}/{spel}"
                );
            });
        }
        private static void UpdateDatabase(IApplicationBuilder app)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    using (var context = serviceScope.ServiceProvider.GetService<ReversiContext>())
                    {
                        context.Database.Migrate();
                    }
                }

            }
            catch
            {
                Console.WriteLine("Database server not available or database corrupted.");
            }

        }
    }
}
