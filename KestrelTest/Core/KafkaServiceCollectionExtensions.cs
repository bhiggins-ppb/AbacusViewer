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

            KafkaConsumerConfiguration GetKafkaConfiguration(string sectionName)
                => configurationManager.GetSection(sectionName).Get<KafkaConsumerConfiguration>();

            // A "factory method" to create a prematch or in-play topic consumer depending on the boolean provided
            services.AddScoped<Func<bool, KafkaConsumer>>(provider => inPlay => inPlay switch
            {
                false => new KafkaConsumer(GetKafkaConfiguration("KafkaConsumerPreMatch")),
                true => new KafkaConsumer(GetKafkaConfiguration("KafkaConsumerInPlay"))
            });

            return services;
        }
    }
}
