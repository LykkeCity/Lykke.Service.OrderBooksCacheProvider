using Autofac;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Lykke.Job.OrderBooksCacheProvider.Services;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using RestSharp;

namespace Lykke.Job.OrderBooksCacheProvider.Binders
{
    public class JobModule : Module
    {
        private readonly OrderBooksCacheProviderSettings _settings;
        private readonly ILog _log;

        public JobModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings.CurrentValue.OrderBooksCacheProvider;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterInstance(_log);
            builder.RegisterInstance(_settings);

            var redis = new RedisCache(new RedisCacheOptions
            {
                Configuration = _settings.CacheSettings.RedisConfiguration,
                InstanceName = _settings.CacheSettings.FinanceDataCacheInstance
            });
            builder.RegisterInstance(redis).As<IDistributedCache>();

            var exchangeName = _settings.MatchingEngine.RabbitMq.ExchangeOrderbook;
            var rabbitSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _settings.MatchingEngine.RabbitMq.GetConnectionString(),
                QueueName = $"{exchangeName}.OrderBooksCacheProvider",
                ExchangeName = exchangeName
            };

            builder.Register(x => new RestClient()).As<IRestClient>();
            builder.RegisterType<OrderBookInitializer>().As<IOrderBookInitializer>();

            builder.RegisterInstance(rabbitSettings);
            builder.RegisterType<OrderBookReader>().As<IOrderBookReader>().SingleInstance();
            builder.RegisterType<OrderBooksHandler>().As<IOrderBooksHandler>().SingleInstance();
        }
    }
}
