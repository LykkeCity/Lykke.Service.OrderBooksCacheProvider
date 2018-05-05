using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            if (string.IsNullOrEmpty(assetPair))
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorMessage = "assetPair cannot be empty"
                });
            }

            try
            {
                return Ok(await _booksProvider.GetCurrentOrderBooksAsync(assetPair));
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