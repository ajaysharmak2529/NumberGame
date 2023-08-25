using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NumGameWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();
            builder.Services.AddProgressiveWebApp();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(options =>
                            {
                                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                                options.SlidingExpiration = true;
                                options.AccessDeniedPath = "/Account/AccessDenied";
                                options.LogoutPath = "/Account/Login";
                                options.Cookie.Path = "/";
                                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure secure cookies
                                options.Cookie.HttpOnly = true; // Ensure HttpOnly cookies
                            });
            builder.Services.AddAuthorization();


            builder.Services.AddSignalR();
            builder.Services.AddHttpContextAccessor();
            //builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.TryAddScoped<IServices,Services>();
            builder.Services.TryAddScoped<BackgroundJobs>();
            builder.Services.AddHostedService<BackgroundJobs>();
           


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapDefaultControllerRoute();
            app.MapHub<UpdateHub>("/UpdateHub");

            app.Run();
        }
    }
}