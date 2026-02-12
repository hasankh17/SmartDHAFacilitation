using System.Text;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Common.Models;
using DHAFacilitationAPIs.Application.Common.Settings;
using DHAFacilitationAPIs.Application.Interface.Repository;
using DHAFacilitationAPIs.Infrastructure.Data;
using DHAFacilitationAPIs.Infrastructure.Data.Interceptors;
using DHAFacilitationAPIs.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;


namespace DHAFacilitationAPIs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        // services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(connectionString);
            //options.UseMySql(
            //              ServerVersion.AutoDetect(connectionString),
            //              options => options.EnableRetryOnFailure(
            //        maxRetryCount: 5,
            //        maxRetryDelay: System.TimeSpan.FromSeconds(30),
            //        errorNumbersToAdd: null));
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();

        var smartDHAConnection = configuration.GetConnectionString("OLMRSConnection");
        Guard.Against.Null(smartDHAConnection, message: "Connection string 'smartDHAConnection' not found.");

        services.AddDbContext<SmartdhaDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(smartDHAConnection);
        });

        services.AddScoped<ISmartdhaDbContext>(provider =>
            provider.GetRequiredService<SmartdhaDbContext>());


        services.AddScoped<IProcedureService, StoredProcedures>();

        services.AddIdentityCore<ApplicationUser>(opt =>
        {
            opt.Password.RequiredLength = 8;
            opt.Password.RequireDigit = true;
            opt.Password.RequireUppercase = true;

        }).AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
           .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddCookie("Identity.Application", options =>
        { // all your options
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.SameSite = SameSiteMode.Lax;
        }).AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = configuration["JwtSettings:Issuer"],
                        ValidAudience = configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? string.Empty))
                    };

                    o.Events = new JwtBearerEvents()
                    {
                        OnChallenge = context =>
                        {
                            context.Response.OnStarting(async () =>
                            {
                                context.HandleResponse();
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("401 Not authorized");
                            });

                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            context.Response.OnStarting(async () =>
                            {
                                context.Response.StatusCode = 403;
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("403 forbidden");
                            });

                            return Task.CompletedTask;
                        },
                    };
                });

        services.AddAuthorizationBuilder();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization();

        services
            .Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)));

        return services;
    }
}
