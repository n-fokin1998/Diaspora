using Diaspora.Identity.Application.Authentication.Login;
using Diaspora.Identity.Application.Authentication.Register;
using Diaspora.Identity.Application.Common.Abstractions;
using Diaspora.Identity.Application.Common.Behaviors;
using Identity.Application.Authentication.Register;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Diaspora.Identity.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddTransient<IValidator<LoginCommand>, LoginCommandValidator>();
            services.AddTransient<IValidator<RegisterCommand>, RegisterCommandValidator>();

            return services;
        }
    }
}
