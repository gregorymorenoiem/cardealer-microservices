using FluentValidation;
using MediaService.Application.Common.Behaviours;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediaService.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Use the Application assembly (where handlers/validators live),
            // NOT Assembly.GetExecutingAssembly() which returns the Api assembly
            // since this file physically resides in MediaService.Api project.
            // LoggingBehaviour<,> is defined in MediaService.Application, so its
            // assembly correctly points to the Application DLL.
            var applicationAssembly = typeof(LoggingBehaviour<,>).Assembly;

            // Registra MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            });

            // Registra los validadores de FluentValidation 
            services.AddValidatorsFromAssembly(applicationAssembly);

            // Configura FluentValidation para ASP.NET Core
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }
    }
}