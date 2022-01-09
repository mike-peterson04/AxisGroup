// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain;
using Shared;
using Shared.Order;

namespace OrderService
{
    /// <summary>
    /// Handle commands related to the order service
    /// </summary>
    public class OrderCommandHandler :
        INotificationHandler<StartNewOrder>,
        INotificationHandler<AddItemsToOrder>,
        INotificationHandler<ServeItemForOrder>,
        INotificationHandler<CloseOrderWithPayment>
    {
        private readonly IEventStore<OrderDomainEvent> _eventStore;
        private readonly IDocumentStore<OrderAggregate> _documentStore;
        private readonly IMediator _publisher;
        private readonly ILogger _logger;

        public OrderCommandHandler(ILogger<OrderCommandHandler> logger, IDocumentStore<OrderAggregate> documentStore, IEventStore<OrderDomainEvent> eventStore, IMediator publisher)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger;
        }

        // StartNewOrder handler
        Task INotificationHandler<StartNewOrder>.Handle(StartNewOrder notification, CancellationToken cancellationToken) 
            => HandleBase(notification,
                failureMessage => { return new StartNewOrderFailed(DateTime.UtcNow, notification, failureMessage); }
                    ,cancellationToken);

        // AddItemsToOrder handler
        Task INotificationHandler<AddItemsToOrder>.Handle(AddItemsToOrder notification, CancellationToken cancellationToken)
            => HandleBase(notification,
                failureMessage => { return new AddItemsToOrderFailed(DateTime.UtcNow, notification, failureMessage); }
                    ,cancellationToken);

        // CloseOrderWithPayment handler
        Task INotificationHandler<CloseOrderWithPayment>.Handle(CloseOrderWithPayment notification, CancellationToken cancellationToken)
            => HandleBase(notification, 
                failureMessage => { return new CloseOrderWithPaymentFailed(DateTime.UtcNow, notification, failureMessage); }
                    ,cancellationToken);

        // ServeItemForOrder handler
        Task INotificationHandler<ServeItemForOrder>.Handle(ServeItemForOrder notification, CancellationToken cancellationToken) 
            => HandleBase(notification,
                failureMessage => { return new ServeItemForOrderFailed(DateTime.UtcNow, notification, failureMessage); }
                    , cancellationToken);

        /// <summary>
        /// Base handler for processing order commands
        /// </summary>
        /// <typeparam name="TCommand">OrderCommand type</typeparam>
        /// <param name="incomingCommand">Originating command</param>
        /// <param name="failureEvent">Function for accepting an error and producing an integration failure event</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task HandleBase<TCommand>(TCommand incomingCommand, Func<string, IEvent> failureEvent, CancellationToken cancellationToken)
            where TCommand : OrderCommand
        {
            _logger.LogInformation($"Received {typeof(TCommand)} {incomingCommand.MessageId}");
            try
            {
                var maybeCommand = incomingCommand.ValidateCommand();
                if (maybeCommand.IsSuccess)
                {
                    var command = maybeCommand.Value;
                    var existingEvents = _eventStore.GetDomainEvents(command.OrderNumber);
                    var existingProjection = await existingEvents.ApplyDomainEventsAsync();

                    var maybeExecutionResult = existingProjection.Execute(command);
                    if (maybeExecutionResult.IsSuccess)
                    {
                        (OrderAggregate newProjection, IEnumerable<OrderDomainEvent> newEvents, IEnumerable<IEvent> integrationEvents) = maybeExecutionResult.Value;
                        var maybeCommit = await _eventStore.Commit(newEvents);
                        if (maybeCommit.IsSuccess)
                        {
                            _logger.LogInformation($"{typeof(TCommand)} {incomingCommand.MessageId} processed with {newEvents.Count()} new events, storing updated projection for Order # {newProjection.StreamId} with version {newProjection.AggregateVersion}");
                            await _documentStore.Store(newProjection);
                            foreach(var ie in integrationEvents) await _publisher.Publish(ie, cancellationToken);
                        }
                        else
                        {
                            var error = $"Attempt to process {typeof(TCommand)} {incomingCommand.MessageId} failed (Retries {incomingCommand.Retries} left) : Could not store events, {maybeCommit.Error}";
                            _logger.LogError(error);
                            if(incomingCommand.Retries > 0)
                            {
                                await RetryCommand(incomingCommand, cancellationToken);
                                return;
                            }
                            await _publisher.Publish(failureEvent.Invoke(error), cancellationToken);
                            return;
                        }
                    }
                    else
                    {
                        var error = $"Attempt to process {typeof(TCommand)} {incomingCommand.MessageId} failed (Retries {incomingCommand.Retries} left) : Projection state was invalid, {maybeExecutionResult.Error}";
                        _logger.LogError(error);
                        if (incomingCommand.Retries > 0)
                        {
                            await RetryCommand(incomingCommand, cancellationToken);
                            return;
                        }
                        await _publisher.Publish(failureEvent.Invoke(error), cancellationToken);
                        return;
                    }
                }
                else
                {
                    var error = $"Attempt to process {typeof(TCommand)} {incomingCommand.MessageId} failed: Command was invalid, {maybeCommand.Error}";
                    _logger.LogError(error);
                    await _publisher.Publish(failureEvent.Invoke(error), cancellationToken);
                    return;
                }
            }
            catch(Exception ex)
            {
                var error = $"Attempt to process {typeof(TCommand)} {incomingCommand.MessageId} failed: Exception thrown, {ex.Message}";
                _logger.LogError(error);
                await _publisher.Publish(failureEvent.Invoke(error), cancellationToken);
            }
        }

        /// <summary>
        /// Re-issue a command to retry after a delay. This may be needed when concurrency conflicts affect event storage or projection state.
        /// </summary>
        /// <typeparam name="TCommand">OrderCommand type</typeparam>
        /// <param name="retryCommand">Command to retry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        async Task RetryCommand<TCommand>(TCommand retryCommand, CancellationToken cancellationToken)
            where TCommand : OrderCommand
        {
            var newCommand = retryCommand with { MessageId = Guid.NewGuid(), Retries = retryCommand.Retries - 1 };
            Random random = new Random();
            int wait = random.Next(300, 2000);
            await Task.Delay(wait, cancellationToken);
            await _publisher.Publish(newCommand, cancellationToken);
        }
    }
}
