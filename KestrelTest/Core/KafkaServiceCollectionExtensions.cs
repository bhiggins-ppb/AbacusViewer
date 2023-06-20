namespace KestrelTest.Core
{
    public static class KafkaServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var kafkaConsumerConfiguration = configurationManager.GetSection("KafkaConsumer").Get<KafkaConsumerConfiguration>();
            services.AddSingleton(kafkaConsumerConfiguration);
            services.AddScoped<KafkaConsumer>();

            return services;
        }
    }
}
