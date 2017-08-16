using System.Text;
using Common;
using Core.Domain;
using Lykke.RabbitMqBroker.Subscriber;

namespace Services.Tools
{
    public class OrderBookDeserializer : IMessageDeserializer<OrderBook>
    {
        public OrderBook Deserialize(byte[] data)
        {
            var msg = Encoding.ASCII.GetString(data);
            return msg.DeserializeJson<OrderBook>();
        }
    }
}
