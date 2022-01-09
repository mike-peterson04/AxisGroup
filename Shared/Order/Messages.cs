// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using Microsoft.FSharp.Collections;

namespace Shared.Order
{
    // Base order command with order number
    public abstract record OrderCommand(DateTime UtcTime, int OrderNumber)
        : Command(Guid.NewGuid(), UtcTime, 3);

    // Start Order - Command & Events
    public record StartNewOrder(DateTime UtcTime, int OrderNumber)
        : OrderCommand(UtcTime, OrderNumber);

    public record OrderStarted(DateTime UtcTime, StartNewOrder Command, Order Order)
        : Event<StartNewOrder, Order>(Guid.NewGuid(), UtcTime, Command, Order);

    public record StartNewOrderFailed(DateTime UtcTime, StartNewOrder Command, string Error)
        : FailureEvent<StartNewOrder, string>(Guid.NewGuid(), UtcTime, Command, Error);


    // Add items to order - Command & Events
    public record AddItemsToOrder(DateTime UtcTime, int OrderNumber, FSharpList<MenuItem> Items)
        : OrderCommand(UtcTime, OrderNumber);

    public record ItemsAddedToOrder(DateTime UtcTime, AddItemsToOrder Command, Order Order, FSharpMap<int, MenuItem> OrderItems)
        : Event<AddItemsToOrder, Order>(Guid.NewGuid(), UtcTime, Command, Order);

    public record AddItemsToOrderFailed(DateTime UtcTime, AddItemsToOrder Command, string Error)
        : FailureEvent<AddItemsToOrder, string>(Guid.NewGuid(), UtcTime, Command, Error);


    // Serve a prepared/ready item for order - Command & Events
    public record ServeItemForOrder(DateTime UtcTime, int OrderNumber, int OrderItemNumber)
        : OrderCommand(UtcTime, OrderNumber);

    public record ItemServedForOrder(DateTime UtcTime, ServeItemForOrder Command, Order Order, MenuItem Item)
        : Event<ServeItemForOrder, Order>(Guid.NewGuid(), UtcTime, Command, Order);

    public record ServeItemForOrderFailed(DateTime UtcTime, ServeItemForOrder Command, string Error)
        : FailureEvent<ServeItemForOrder, string>(Guid.NewGuid(), UtcTime, Command, Error);


    // Complete Order by payment - Command & Events
    public record CloseOrderWithPayment(DateTime UtcTime, int OrderNumber, double PaymentAmount)
        : OrderCommand(UtcTime, OrderNumber);

    public record OrderClosedByPayment(DateTime UtcTime, CloseOrderWithPayment Command, Order Order)
        : Event<CloseOrderWithPayment, Order>(Guid.NewGuid(), UtcTime, Command, Order);

    public record CloseOrderWithPaymentFailed(DateTime UtcTime, CloseOrderWithPayment Command, string Error)
        : FailureEvent<CloseOrderWithPayment, string>(Guid.NewGuid(), UtcTime, Command, Error);
}
