using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.DAL;
using EventManagerWebRazorPage.Data;
using EventManagerWebRazorPage.Hubs;
using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services;
using EventManagerWebRazorPage.Services.Other;
using EventManagerWebRazorPage.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

namespace EventManagerWebRazorPage
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false).AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();

            builder.Services.AddScoped<SignInManager<User>, CustomSignInManager>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IPathProvider, PathProvider>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
            builder.Services.AddScoped<IValidator<EventDetail>, EventDetailValidator>();
            builder.Services.AddScoped<IValidator<EventItem>, EventItemValidator>();
            builder.Services.AddScoped<IValidator<EventDetailViewModel>, EventDetailViewModelValidator>();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = String.Concat(options.User.AllowedUserNameCharacters, " ");
                //Password settings
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                //Lockout settings
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
                options.Lockout.MaxFailedAccessAttempts = 3;
            });

            builder.Services.AddQuartz();
            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            builder.Services.AddSerilog(options =>
            {
                options.MinimumLevel.Information();
                options.WriteTo.Console();
                options.WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day);
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.MapHub<ConfirmStatusHub>("/confirmStatusHub");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            await SampleData.Seed(app.Services);

            await app.RunAsync();
        }
    }
}
