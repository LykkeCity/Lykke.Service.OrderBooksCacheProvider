namespace Lykke.Job.OrderBooksCacheProvider.Client
{
    public class VolumePrice
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }

        public TradeType TradeType => Volume > 0 ? TradeType.Buy : TradeType.Sell;
    }
}