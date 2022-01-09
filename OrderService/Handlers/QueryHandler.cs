// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain;
using Shared;
using Shared.Order;

namespace OrderService
{
    /// <summary>
    /// Handle query requests for orders
    /// </summary>
    public class OrderQueryHandler :
        IRequestHandler<GetOrder, Order>,
        IRequestHandler<GetOrders, IEnumerable<Order>>
    {
        private readonly IDocumentStore<OrderAggregate> _documentStore;
        private readonly ILogger _logger;

        public OrderQueryHandler(ILogger<OrderQueryHandler> logger, IDocumentStore<OrderAggregate> documentStore)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _logger = logger;
        }

        /// <summary>
        /// Return a single order by order number
        /// </summary>
        /// <param name="request">Order query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A result with an order or error</returns>
        async Task<Order> IRequestHandler<GetOrder, Order>.Handle(GetOrder request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Received {nameof(GetOrder)} for order {request.OrderNumber}");
            var maybeResult = await _documentStore.Get(request.OrderNumber);
            if (maybeResult.IsSuccess) return maybeResult.Value.ToDto();
            else throw new IndexOutOfRangeException($"Order {request.OrderNumber} does not exist in store");
        }

        /// <summary>
        /// Return all orders
        /// </summary>
        /// <param name="request">Orders query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable of orders</returns>
        Task<IEnumerable<Order>> IRequestHandler<GetOrders, IEnumerable<Order>>.Handle(GetOrders request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Received {nameof(GetOrders)}");
            return Task.FromResult(
                _documentStore.Get()
                    .ToEnumerable()
                    .Select(x => x.ToDto()));
        }
    }
}
