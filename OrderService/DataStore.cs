// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using OrderService.Domain;
using Shared;

namespace OrderService
{
    /// <summary>
    /// Domain event store
    /// </summary>
    public class OrderDomainEventStore : EventStoreBase<OrderAggregate, OrderDomainEvent>
    {
        public OrderDomainEventStore(InMemoryContext context) : base(context, "Order") { }
    }

    /// <summary>
    /// Order Aggregate store
    /// </summary>
    public class OrderStore : DocumentStoreBase<OrderAggregate>
    {
        public OrderStore(InMemoryContext context) : base(context, "Order") { }
    }

}
