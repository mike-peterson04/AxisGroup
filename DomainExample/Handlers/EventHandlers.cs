// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared;
using Shared.Order;

namespace DomainExample.Handlers
{
    public class DomainEventHandlers : 
        INotificationHandler<StartNewOrder>,
        INotificationHandler<OrderStarted>,
        INotificationHandler<StartNewOrderFailed>,
        INotificationHandler<AddItemsToOrder>,
        INotificationHandler<ItemsAddedToOrder>,
        INotificationHandler<AddItemsToOrderFailed>,
        INotificationHandler<ServeItemForOrder>,
        INotificationHandler<ItemServedForOrder>,
        INotificationHandler<ServeItemForOrderFailed>,
        INotificationHandler<CloseOrderWithPayment>,
        INotificationHandler<OrderClosedByPayment>,
        INotificationHandler<CloseOrderWithPaymentFailed>
    {

        private readonly ILogger _logger;
        private readonly IntegrationMessageCache _eventCache;

        public DomainEventHandlers(ILogger<DomainEventHandlers> logger, IntegrationMessageCache eventCache)
        {
            _eventCache = eventCache ?? throw new ArgumentNullException(nameof(eventCache));
            _logger = logger;
        }

        #pragma warning disable IDE0060 // Remove unused parameter
        public Task Handle<T>(T notification, string messageType, string message, CancellationToken cancellationToken) where T : IMessage
        #pragma warning restore IDE0060 // Remove unused parameter
        {
            _logger.LogDebug($"Consumed {messageType} {typeof(T)} with ID {notification.MessageId}");
            _eventCache.Add(notification.MessageId, new IntegrationMessageDisplay(typeof(T).Name, messageType, notification.UtcTime, message));
            return Task.CompletedTask;
        }

        // StartNewOrder & events
        Task INotificationHandler<StartNewOrder>.Handle(StartNewOrder notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Command, $"Start new order with number {notification.OrderNumber}", cancellationToken);
        Task INotificationHandler<OrderStarted>.Handle(OrderStarted notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"New order started with number {notification.Order.OrderNumber}", cancellationToken);
        Task INotificationHandler<StartNewOrderFailed>.Handle(StartNewOrderFailed notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"Failed to start new order {notification.Command.OrderNumber} : {notification.Error}", cancellationToken);

        // AddItemsToOrder & events
        Task INotificationHandler<AddItemsToOrder>.Handle(AddItemsToOrder notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Command, $"Adding items to order {notification.OrderNumber} : {string.Join(";", notification.Items.Select(x => $"{x.GetType().Name} @ {x.Price}"))}", cancellationToken);
        Task INotificationHandler<ItemsAddedToOrder>.Handle(ItemsAddedToOrder notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"Added items to order {notification.Order.OrderNumber} : {string.Join(";", notification.OrderItems.Select(x => $" (#{x.Key}) {x.Value.GetType().Name} @ {x.Value.Price}"))}", cancellationToken);
        Task INotificationHandler<AddItemsToOrderFailed>.Handle(AddItemsToOrderFailed notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"Failed to add items to order {notification.Command.OrderNumber} : {notification.Error}", cancellationToken);

        // ServeReadyItemForOrder & events
        Task INotificationHandler<ServeItemForOrder>.Handle(ServeItemForOrder notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Command, $"Serve item for order {notification.OrderNumber}: Item {notification.OrderItemNumber}", cancellationToken);
        Task INotificationHandler<ItemServedForOrder>.Handle(ItemServedForOrder notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"Item served for order {notification.Order.OrderNumber} : {notification.Command.OrderItemNumber} - {notification.Item.GetType().Name}", cancellationToken);
        Task INotificationHandler<ServeItemForOrderFailed>.Handle(ServeItemForOrderFailed notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"Failed to serve item for order {notification.Command.OrderNumber} with order item number {notification.Command.OrderItemNumber} : {notification.Error}", cancellationToken);

        // CloseOrderWithPayment & events
        Task INotificationHandler<CloseOrderWithPayment>.Handle(CloseOrderWithPayment notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Command, $"Closing order number {notification.OrderNumber} with payment {notification.PaymentAmount}", cancellationToken);
        Task INotificationHandler<OrderClosedByPayment>.Handle(OrderClosedByPayment notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"Order number {notification.Order.OrderNumber} closed with payment {notification.Command.PaymentAmount}", cancellationToken);
        Task INotificationHandler<CloseOrderWithPaymentFailed>.Handle(CloseOrderWithPaymentFailed notification, CancellationToken cancellationToken) => Handle(notification, MessageType.Event, $"Order number {notification.Command.OrderNumber} could not be closed with payment {notification.Command.PaymentAmount} : {notification.Error}", cancellationToken);
    }
}
