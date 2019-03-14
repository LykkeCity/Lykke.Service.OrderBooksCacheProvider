using Lykke.Common.Api.Contract.Responses;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.Controllers
{
    [Route("api/[controller]")]
    public class OrderBookProviderController : Controller
    {
        private readonly IOrderBooksProvider _booksProvider;

        public OrderBookProviderController(IOrderBooksProvider booksProvider)
        {
            _booksProvider = booksProvider;
        }

        [HttpGet]
        [SwaggerOperation("GetOrderBook")]
        [ProducesResponseType(typeof(List<OrderBook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOrderBook(string assetPair)
        {
            try
            {
                var orderBook = await _booksProvider.GetCurrentOrderBooksAsync(assetPair);
                if (orderBook == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        ErrorMessage = "order book for assetPair not found"
                    });
                }
                return Ok(orderBook);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    ErrorMessage = ex.ToString()
                });
            }
        }
    }
}