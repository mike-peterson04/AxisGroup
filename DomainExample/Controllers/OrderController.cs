// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Order;
using Microsoft.FSharp.Collections;
using Shared.Menu;

namespace Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IMediator _publisher;

        public OrderController(ILogger<OrderController> logger, IMediator publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger;
        }

        /// <summary>
        /// Start a new order with a provided order number
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <returns>Ok</returns>
        [HttpPost]
        [Route("Command/StartNewOrder")]
        public async Task<ActionResult> PublishStartNewOrder(int orderNumber)
        {
            _logger.LogDebug($"Received {nameof(PublishStartNewOrder)}");
            await _publisher.Publish(new StartNewOrder(DateTime.UtcNow, orderNumber));
            return Ok();
        }

        /// <summary>
        /// Add a burger, fries, and soft drink to an order
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <returns>Ok</returns>
        [HttpPost]
        [Route("Command/AddComboToOrder")]
        public async Task<ActionResult> AddComboToOrder(int orderNumber)
        {
            _logger.LogDebug($"Received {nameof(AddItemsToOrder)}");
            var items = new MenuItem[] {
                new Burger(),
                new Fries(),
                new SoftDrink()
            };
            await _publisher.Publish(new AddItemsToOrder(DateTime.UtcNow, orderNumber, ListModule.OfSeq(items)));
            return Ok();
        }

        /// <summary>
        /// Add an item from the menu to an order
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <param name="itemNumber">Item number</param>
        /// <returns>Ok</returns>
        [HttpPut]
        [Route("Command/AddItemToOrder")]
        public async Task<ActionResult> AddItemToOrder(int orderNumber, int itemNumber)
        {
            _logger.LogDebug($"Received {nameof(AddItemsToOrder)}");
            var items = Array.Empty<MenuItem>();
            switch (itemNumber)
            {
                case 1:
                    items = new MenuItem[]
                    {
                        new Burger()
                    };
                break;
                case 2:
                    items = new MenuItem[]
                    {
                        new Fries()
                    };
                    break;
                case 3:
                    items = new MenuItem[]
                    {
                        new SoftDrink()
                    };
                break;
                case 4:
                    items = new MenuItem[]
                    {
                        new Tea()
                    };
                break;
                default:
                break;

            }
            await _publisher.Publish(new AddItemsToOrder(DateTime.UtcNow, orderNumber, ListModule.OfSeq(items)));
            return Ok();
        }

        /// <summary>
        /// Serve an item prepared for an order
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <param name="orderItemNumber">Order item number</param>
        /// <returns>Ok</returns>
        [HttpPost]
        [Route("Command/ServeItemForOrder")]
        public async Task<ActionResult> ServeItemForOrder(int orderNumber, int orderItemNumber)
        {
            _logger.LogDebug($"Received {nameof(ServeItemForOrder)}");
            await _publisher.Publish(new ServeItemForOrder(DateTime.UtcNow, orderNumber, orderItemNumber));
            return Ok();
        }

        /// <summary>
        /// Close an order with a provided payment
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <param name="paymentAmount">Payment amount</param>
        /// <returns>Ok</returns>
        [HttpPost]
        [Route("Command/CloseOrderWithPayment")]
        public async Task<ActionResult> CloseOrderWithPayment(int orderNumber, double paymentAmount)
        {
            _logger.LogDebug($"Received {nameof(CloseOrderWithPayment)}");
            await _publisher.Publish(new CloseOrderWithPayment(DateTime.UtcNow, orderNumber, paymentAmount));
            return Ok();
        }

        /// <summary>
        /// Get an order with a provided order number
        /// </summary>
        /// <param name="orderNumber">Order number</param>
        /// <returns>Order DTO</returns>
        [HttpGet]
        [Route("Query/GetOrder")]
        public async Task<ActionResult> GetOrder(int orderNumber)
        {
            _logger.LogDebug($"Received {nameof(GetOrder)}");
            string error = string.Empty;
            try
            {
                var result = await _publisher.Send(new GetOrder(orderNumber));
                if (result != null) return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error on {nameof(GetOrder)} for {orderNumber}");
            }
            return NotFound($"Order {orderNumber} not found");
        }
    }
}
