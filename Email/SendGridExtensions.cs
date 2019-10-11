using Microsoft.Extensions.DependencyInjection;
using YounesCo_Backend.Services;

namespace YounesCo_Backend.Email
{
    public static class SendGridExtensions
    {
        public static IServiceCollection AddSendGridEmailSender(this IServiceCollection services)
        {
            services.AddTransient<IEmailSender, SendGridEmailSender>();

            return services;
        }

    }
}
