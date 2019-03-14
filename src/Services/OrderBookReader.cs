using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class OrderBookReader : IOrderBookReader
    {
        private readonly IOrderBooksHandler _orderBooksHandler;
        private readonly RabbitMqSubscriber<OrderBook> _subscriber;

        public DateTime LastReceivedTimeStamp { get; private set; }

        public OrderBookReader(RabbitMqSubscriptionSettings settings,
            IOrderBooksHandler orderBooksHandler,
            ILogFactory logFactory)
        {
            _orderBooksHandler = orderBooksHandler;

            var strategy = new ResilientErrorHandlingStrategy(logFactory, settings, TimeSpan.FromSeconds(10),
                next: new DefaultErrorHandlingStrategy(logFactory, settings));
            _subscriber = new RabbitMqSubscriber<OrderBook>(logFactory, settings, strategy)
                .SetMessageDeserializer(new JsonMessageDeserializer<OrderBook>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .SetPrefetchCount(300)
                .Subscribe(HandleData);
        }

        public void StartRead()
        {
            _subscriber.Start();
        }

        private async Task HandleData(OrderBook orderBook)
        {
            LastReceivedTimeStamp = DateTime.UtcNow;

            await _orderBooksHandler.HandleOrderBook(orderBook);
        }
    }
}
