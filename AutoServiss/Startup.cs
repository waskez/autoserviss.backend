using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using AutoServiss.Services.Auth;
using AutoServiss.Services.Email;
using AutoServiss.Repositories.Admin;
using AutoServiss.Repositories.Klienti;
using AutoServiss.Repositories.Serviss;
using Microsoft.EntityFrameworkCore;
using AutoServiss.Database;
using AutoServiss.Repositories.Uznemumi;
using AutoServiss.Services.Backup;
using AutoServiss.Repositories.Statuss;

namespace AutoServiss
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }        

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            var dbPath = "Data Source=" + Path.Combine("wwwroot", "data", "autoserviss.db"); // TODO: lai ietu arī uz linux (pagaidām šādi)
            services.AddDbContext<AutoServissDbContext>(options => options.UseSqlite(dbPath));

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
            services.AddTransient<IStatussRepository, StatussRepository>();
            services.AddTransient<IAdminRepository, AdminRepository>();
            services.AddTransient<IKlientiRepository, KlientiRepository>();
            services.AddTransient<IServissRepository, ServissRepository>();
            services.AddTransient<IUznemumiRepository, UznemumiRepository>();
            services.AddTransient<IBackupService, BackupService>();

            var builder = services.AddMvcCore();
            builder.AddFormatterMappings();
            builder.AddJsonFormatters();
            builder.AddDataAnnotations();
            if (_env.IsDevelopment())
            {
                builder.AddCors();
            }
        }

        public void Configure(IApplicationBuilder app, IOptions<AppSettings> settings, AutoServissDbContext dbContext)
        {
            app.UseMiddleware(typeof(CustomExceptionHandlerMiddleware));

            if (_env.IsDevelopment())
            {
                app.UseCors(policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowCredentials();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //// nginx reverse-proxy, jābūt pirms UseAuthentication
            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            //});

            app.UseAuthentication();

            app.UseMvc();

            //SeedData.Initialize(dbContext, _env);
        }
    }
}