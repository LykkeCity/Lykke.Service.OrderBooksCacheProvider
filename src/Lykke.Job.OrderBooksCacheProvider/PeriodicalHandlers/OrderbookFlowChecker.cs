using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;

namespace Lykke.Job.OrderBooksCacheProvider.PeriodicalHandlers
{
    internal class OrderbookFlowChecker : TimerPeriod
    {
        private readonly IOrderBookInitializer _orderBookInitializer;
        private readonly IOrderBookReader _orderBookReader;
        private readonly ILog _log;

        public OrderbookFlowChecker(
            [NotNull] IOrderBookInitializer orderBookInitializer,
            [NotNull] IOrderBookReader orderBookReader,
            TimeSpan checkInterval,
            ILog log = null)
            : base(nameof(OrderbookFlowChecker), (int)checkInterval.TotalMilliseconds, log)
        {
            _orderBookInitializer = orderBookInitializer ?? throw new ArgumentNullException(nameof(orderBookInitializer));
            _orderBookReader = orderBookReader ?? throw new ArgumentNullException(nameof(orderBookReader));
            _log = log.CreateComponentScope(nameof(OrderbookFlowChecker));
        }

        public override async Task Execute()
        {
            if (_orderBookReader.LastReceivedTimeStamp == default(DateTime))
                return; // it's ok: service is not initialized yet.

            if (_orderBookReader.LastReceivedTimeStamp > DateTime.UtcNow.AddMinutes(-1))
                return; // it's ok: we receive messages more than once a minute.

            _log.WriteWarning(nameof(OrderbookFlowChecker), null, "No orderbook updates received for the last minute.");
            await _orderBookInitializer.InitOrderBooks(false);
        }
    }
}
