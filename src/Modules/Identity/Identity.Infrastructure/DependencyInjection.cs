using Identity.Application.Common.Abstractions;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityDbContext>((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("DiasporaDb")
                    ?? throw new InvalidOperationException("Missing required configuration: ConnectionStrings:DiasporaDb.");

                options.UseNpgsql(connectionString);
            });

            services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                return new JwtOptions
                {
                    Key = config["Jwt:Key"]
                        ?? throw new InvalidOperationException("Missing required configuration: Jwt:Key."),
                    Issuer = config["Jwt:Issuer"]
                        ?? throw new InvalidOperationException("Missing required configuration: Jwt:Issuer."),
                    Audience = config["Jwt:Audience"]
                        ?? throw new InvalidOperationException("Missing required configuration: Jwt:Audience."),
                    AccessTokenMinutes = int.TryParse(config["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 60,
                };
            });

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
