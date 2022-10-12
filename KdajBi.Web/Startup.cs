using KdajBi.Core;
using KdajBi.Core.Models;
using KdajBi.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace KdajBi
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            
            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddRazorPages();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddAuthentication(options =>
            {
                //authorize with redirect to google
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                
            })
               .AddGoogle(GoogleDefaults.AuthenticationScheme, opts =>
               {
                   opts.ClientId = Configuration.GetSection("GoogleSettings")["ClientId"];
                   opts.ClientSecret = Configuration.GetSection("GoogleSettings")["ClientSecret"];
                   opts.SignInScheme =  IdentityConstants.ExternalScheme;
                   opts.CorrelationCookie.SameSite = SameSiteMode.None;
                   opts.CorrelationCookie.IsEssential = true;
                   opts.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                   opts.Scope.Add("https://www.googleapis.com/auth/calendar");
                   opts.AccessType = "offline";
                   opts.SaveTokens=true;
               });


            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p =>
                {
                    p.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                  .Build();
            });

            services.AddControllersWithViews();


            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddSingleton<IEmailSender, EmailSender>();

            services.Configure<ApiSettings>(Configuration.GetSection("ApiSettings"));
            services.AddSingleton<IApiTokenProvider, ApiTokenProvider>();

            services.Configure<CalendarV3ProviderSettings>(Configuration.GetSection("CalendarV3ProviderSettings"));
            services.AddSingleton<ICalendarV3Provider, CalendarV3Provider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            app.UseHttpsRedirection();
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "/LandingPage/index.html" },
                RequestPath = new PathString("/LandingPage")
            });
            app.UseStaticFiles();

            //app.UseCookiePolicy();


            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();
            });
        }
    }
}
