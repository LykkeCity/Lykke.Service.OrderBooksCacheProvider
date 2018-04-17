using Autofac;
using Lykke.Job.OrderBooksCacheProvider.Core;
using StackExchange.Redis;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class RedisModule : Module
    {
        private readonly CacheSettings _settings;

        public RedisModule(CacheSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => ConnectionMultiplexer.Connect(_settings.RedisConfiguration)).SingleInstance();
        }
    }
}
