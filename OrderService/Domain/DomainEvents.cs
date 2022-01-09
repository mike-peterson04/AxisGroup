// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using Microsoft.FSharp.Collections;
using Shared;

namespace OrderService.Domain
{
    /// <summary>
    /// Abstrac order domain event including an order number
    /// </summary>
    public abstract record OrderDomainEvent(int OrderNumber, int EventId, DateTime UtcTime)
        : DomainEvent(OrderNumber, EventId);

    /// <summary>
    /// Domain event indicating an order has started
    /// </summary>
    public record OrderStarted(int OrderNumber, int EventId, DateTime UtcTime)
        : OrderDomainEvent(OrderNumber, EventId, UtcTime);

    /// <summary>
    /// Domain event indicating items have been added to an order
    /// </summary>
    public record ItemsAddedToOrder(int OrderNumber, int EventId, DateTime UtcTime, FSharpMap<int, MenuItem> Items)
        : OrderDomainEvent(OrderNumber, EventId, UtcTime);

    /// <summary>
    /// Domain event indicating an order was closed due to a valid payment
    /// </summary>
    public record OrderClosedByPayment(int OrderNumber, int EventId, DateTime UtcTime, double PaymentAmount)
        : OrderDomainEvent(OrderNumber, EventId, UtcTime);

    /// <summary>
    /// Domain event indicating item ordered has been served
    /// </summary>
    public record ItemServedForOrder(int OrderNumber, int EventId, DateTime UtcTime, int ItemNumber, MenuItem Item)
        : OrderDomainEvent(OrderNumber, EventId, UtcTime);
}
