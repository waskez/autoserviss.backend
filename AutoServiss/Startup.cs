using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using AutoServiss.Services.Auth;
using AutoServiss.Services.Email;
using AutoServiss.Repositories.Admin;
using AutoServiss.Repositories.Darbinieki;
using AutoServiss.Repositories.Klienti;
using AutoServiss.Repositories.Serviss;
using Microsoft.EntityFrameworkCore;
using AutoServiss.Database;
using AutoServiss.Repositories.Firma;

namespace AutoServiss
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Information()
                .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "Logs\\log-{Date}.txt"), 
                    outputTemplate: "{Timestamp:HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddDbContext<AutoServissDbContext>(options => options.UseSqlite("Data Source=wwwroot\\data\\autoserviss.db"));

            services.AddMemoryCache();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["AppSettings:SecretKey"])),
                    // Validate the JWT Issuer (iss) claim
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["AppSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = Configuration["AppSettings:Audience"],
                    // Validate the token expiry
                    ValidateLifetime = true,
                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policyBuilder => policyBuilder.RequireClaim("admin", "true"));
            });            

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IAdminRepository, AdminRepository>();
            services.AddTransient<IDarbiniekiRepository, DarbiniekiRepository>();
            services.AddTransient<IKlientiRepository, KlientiRepository>();
            services.AddTransient<IServissRepository, ServissRepository>();
            services.AddTransient<IFirmaRepository, FirmaRepository>();

            services.AddMvc();
        }

        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory, 
            IApplicationLifetime appLifetime,
            IOptions<AppSettings> settings,
            AutoServissDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole();
                loggerFactory.AddDebug();
            }

            loggerFactory.AddSerilog();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseMiddleware(typeof(CustomExceptionHandlerMiddleware));

            if (env.IsDevelopment())
            {
                app.UseCors(policy =>
                {
                    policy.AllowAnyOrigin();
                    //policy.AllowCredentials();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();

            SeedData.Initialize(dbContext, env);
        }
    }
}