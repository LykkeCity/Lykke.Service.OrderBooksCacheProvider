using System;
using System.Collections.Generic;

namespace Core.Domain
{
    public class OrderBook
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public DateTime Timestamp { get; set; }
        public List<VolumePrice> Prices { get; set; } = new List<VolumePrice>();
    }

    public class VolumePrice
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
    }
}
