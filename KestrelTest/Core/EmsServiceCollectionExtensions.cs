namespace KestrelTest.Core
{
    public static class EmsServiceCollectionExtensions
    {
        public static IServiceCollection AddEmsService(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var emsServiceConfiguration = configurationManager.GetSection("EmsService").Get<EmsServiceConfiguration>();

            services.AddSingleton(emsServiceConfiguration);
            services.AddScoped<EmsService>();

            return services;
        }
    }
}
