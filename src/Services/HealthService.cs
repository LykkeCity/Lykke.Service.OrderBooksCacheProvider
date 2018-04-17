using System.Collections.Generic;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain.Health;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class HealthService : IHealthService
    {
        public string GetHealthViolationMessage()
        {
            // TODO: Check gathered health statistics, and return appropriate health violation message, or NULL if job hasn't critical errors
            return null;
        }

        public IEnumerable<HealthIssue> GetHealthIssues()
        {
            var issues = new HealthIssuesCollection();
            
            return issues;
        }
    }
}