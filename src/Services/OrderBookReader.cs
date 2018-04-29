using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class OrderBookReader : IOrderBookReader
    {
        private readonly IOrderBooksHandler _orderBooksHandler;
        private readonly RabbitMqSubscriber<OrderBook> _subscriber;

        public DateTime LastReceivedTimeStamp { get; private set; }

        public OrderBookReader(RabbitMqSubscriptionSettings settings,
            IOrderBooksHandler orderBooksHandler,
            ILog log)
        {
            _orderBooksHandler = orderBooksHandler;

            _subscriber =
                new RabbitMqSubscriber<OrderBook>(settings,
                        new ResilientErrorHandlingStrategy(log, settings, TimeSpan.FromSeconds(10),
                            next: new DefaultErrorHandlingStrategy(log, settings)))
                    .SetMessageDeserializer(new JsonMessageDeserializer<OrderBook>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(HandleData)
                    .SetLogger(log);
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
