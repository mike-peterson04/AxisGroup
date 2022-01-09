// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.FSharp.Collections;
using Shared;
using Shared.Order;

namespace OrderService.Domain
{
    public static class OrderDomainExtensions
    {
        #region Apply

        /// <summary>
        /// Apply/fold an async enumerable of domain events
        /// </summary>
        /// <param name="events">Async enumerable of domain events</param>
        /// <returns>Order aggregate projection</returns>
        public static async Task<OrderAggregate> ApplyDomainEventsAsync(this IAsyncEnumerable<OrderDomainEvent> events)
            => await events.AggregateAsync(Zero(), (oldState, ev)
                => oldState.ApplyDomainEvent(ev));

                    /// <summary>
                    /// Initialize a new zero-state order aggregate
                    /// </summary>
                    /// <returns>Zero-state order aggregate</returns>
                    private static OrderAggregate Zero() =>
                        new OrderAggregate(0, 0, 0, 0, DateTime.MinValue, DateTime.MinValue, null, MapModule.Empty<int, MenuItem>(), SetModule.Empty<int>(), 0, 0);

                    /// <summary>
                    /// Apply new domain events syncronously to a generated projection
                    /// </summary>
                    /// <param name="aggregate">Projected aggregate</param>
                    /// <param name="events">Enumerable of domain events</param>
                    /// <returns></returns>
                    private static OrderAggregate ApplyNewDomainEvents(this OrderAggregate aggregate, IEnumerable<OrderDomainEvent> events)
                        => events.Aggregate(aggregate, (oldState, ev)
                            => oldState.ApplyDomainEvent(ev));

                    /// <summary>
                    /// Match and apply a known domain event against a projected aggregate
                    /// </summary>
                    /// <param name="aggregate">Projected aggregate</param>
                    /// <param name="ev">Domain event</param>
                    /// <returns>New aggregate projection</returns>
                    private static OrderAggregate ApplyDomainEvent(this OrderAggregate aggregate, OrderDomainEvent ev)
                        => ev switch
                        {
                            Domain.OrderStarted e1 => aggregate with { StartedUtc = e1.UtcTime, StreamId = e1.OrderNumber, LastUpdatedUtc = e1.UtcTime, AggregateVersion = aggregate.AggregateVersion + 1 },
                            Domain.ItemsAddedToOrder e2 => aggregate with { LastUpdatedUtc = e2.UtcTime, AggregateVersion = aggregate.AggregateVersion + 1, OrderedItems = aggregate.OrderedItems.FoldMap(e2.Items), ItemCount = aggregate.ItemCount + e2.Items.Count, OrderPrice = aggregate.OrderPrice + e2.Items.Select(x => x.Value.Price).Sum() },
                            Domain.ItemServedForOrder e3 => aggregate with { LastUpdatedUtc = e3.UtcTime, AggregateVersion = aggregate.AggregateVersion + 1, ServedItems = aggregate.ServedItems.UnionOne(e3.ItemNumber), ServedCount = aggregate.ServedCount + 1 },
                            Domain.OrderClosedByPayment e4 => aggregate with { LastUpdatedUtc = e4.UtcTime, AggregateVersion = aggregate.AggregateVersion + 1, ClosedUtc = e4.UtcTime, PaidAmount = aggregate.PaidAmount + e4.PaymentAmount },
                            _ => aggregate
                        };

        #endregion

        #region Validate Command

        /// <summary>
        /// Validate the content of a command
        /// </summary>
        /// <typeparam name="TCommand">Command type</typeparam>
        /// <param name="command">Command</param>
        /// <returns>Result with command or errors</returns>
        public static Result<TCommand, string> ValidateCommand<TCommand>(this TCommand command)
            where TCommand : Command => command switch
            {
                StartNewOrder c1 => c1.OrderNumber > 0
                    ? Result.Success<TCommand, string>(command)
                    : Result.Failure<TCommand, string>("Order number is invalid"),

                AddItemsToOrder c2 => c2.OrderNumber > 0
                    ? Result.Success<TCommand, string>(command)
                    : Result.Failure<TCommand, string>("Order number is invalid"),

                ServeItemForOrder c3 => c3.OrderNumber > 0 && c3.OrderItemNumber > 0
                    ? Result.Success<TCommand, string>(command)
                    : Result.Failure<TCommand, string>("Order or order item number is invalid"),

                CloseOrderWithPayment c4 => c4.OrderNumber > 0 && c4.PaymentAmount >= 0
                    ? Result.Success<TCommand, string>(command)
                    : Result.Failure<TCommand, string>("Order number or payment amount is invalid"),

                _ => Result.Failure<TCommand, string>("Unknown command type")
            };

        #endregion

        #region Execute

        /// <summary>
        /// Execute a validated command against a projected aggregate and resolve new projection and events
        /// </summary>
        /// <typeparam name="TCommand">Command type</typeparam>
        /// <param name="projection">Existing projection</param>
        /// <param name="command">Validated command</param>
        /// <returns>Result with new aggregate, domain events, and integration events, or an error</returns>
        public static Result<(OrderAggregate, IEnumerable<OrderDomainEvent>, IEnumerable<Shared.IEvent>), string> Execute<TCommand>(this OrderAggregate projection, TCommand command)
            where TCommand : Command
        {
            var newEvents = projection.Convert(command);
            var maybeNewProjection = projection
                .ApplyNewDomainEvents(newEvents);

            var validator = new OrderAggregateValidator().Validate(maybeNewProjection);

            if (validator.IsValid)
            {
                var integrationEvents = ToIntegrationEvents(maybeNewProjection, command, newEvents);
                return Result.Success<(OrderAggregate, IEnumerable<OrderDomainEvent>, IEnumerable<Shared.IEvent>), string>((maybeNewProjection, newEvents, integrationEvents));
            }  
            else return Result.Failure<(OrderAggregate, IEnumerable<OrderDomainEvent>, IEnumerable<Shared.IEvent>), string>(validator.ToString(";"));
        }

                    /// <summary>
                    /// Convert an integration command into domain events
                    /// </summary>
                    /// <typeparam name="TCommand">Command type</typeparam>
                    /// <param name="projection">Projection</param>
                    /// <param name="command">Command</param>
                    /// <returns>Enumerable of domain events</returns>
                    private static IEnumerable<OrderDomainEvent> Convert<TCommand>(this OrderAggregate projection, TCommand command)
                        where TCommand : Command => command switch
                        {  
                            StartNewOrder c1 => new[] { new OrderStarted(c1.OrderNumber, projection.AggregateVersion + 1, DateTime.UtcNow) },
                            AddItemsToOrder c2 => new[] { new ItemsAddedToOrder(c2.OrderNumber, projection.AggregateVersion + 1, DateTime.UtcNow, MapModule.OfSeq(c2.Items.Select((x, i) => new Tuple<int, MenuItem>(projection.ItemCount + i + 1, x)))) },
                            ServeItemForOrder c3 => new[] { new ItemServedForOrder(c3.OrderNumber, projection.AggregateVersion + 1, DateTime.UtcNow, c3.OrderItemNumber, projection.OrderedItems.TryGetValue(c3.OrderItemNumber, out var item) ? item : new BadItem() ) },
                            CloseOrderWithPayment c4 => new[] { new OrderClosedByPayment(c4.OrderNumber, projection.AggregateVersion + 1, DateTime.UtcNow, c4.PaymentAmount) },
                            _ => Array.Empty<OrderDomainEvent>()
                        };

                    private record BadItem() : MenuItem(-1, "Failed", -1);  // This item type indicates a failure

                    /// <summary>
                    /// Convert a projection, command, and new domain events into corresponding integration events
                    /// </summary>
                    /// <param name="projection">New projection</param>
                    /// <param name="command">Originating command</param>
                    /// <param name="domainEvents">New domain events</param>
                    /// <returns>Enumerable of integration events</returns>
                    private static IEnumerable<Shared.IEvent> ToIntegrationEvents(OrderAggregate projection, Command command, IEnumerable<OrderDomainEvent> domainEvents)
                    {
                        foreach (var de in domainEvents)
                        {
                            yield return de switch
                            {
                                OrderStarted d1 when command is StartNewOrder c1 => new Shared.Order.OrderStarted(d1.UtcTime, c1, projection.ToDto()),
                                ItemsAddedToOrder d2 when command is AddItemsToOrder c2 => new Shared.Order.ItemsAddedToOrder(d2.UtcTime, c2, projection.ToDto(), d2.Items),
                                ItemServedForOrder d3 when command is ServeItemForOrder c3 => new Shared.Order.ItemServedForOrder(d3.UtcTime, c3, projection.ToDto(), d3.Item),
                                OrderClosedByPayment d4 when command is CloseOrderWithPayment c4 => new Shared.Order.OrderClosedByPayment(d4.UtcTime, c4, projection.ToDto()),
                                _ => throw new NotImplementedException()
                            };
                        }
                    }

        #endregion

        #region Convert

        /// <summary>
        /// Convert domain object to DTO
        /// </summary>
        /// <param name="p">Domain aggregate projection</param>
        /// <returns>DTO</returns>
        public static Order ToDto(this OrderAggregate p) =>
            new Order(p.StreamId, p.StartedUtc, !p.ClosedUtc.HasValue, p.ClosedUtc, ListModule.OfSeq(p.OrderedItems.Where(x => !p.ServedItems.Contains(x.Key)).Select(x => x.Value)), ListModule.OfSeq(p.OrderedItems.Where(x => p.ServedItems.Contains(x.Key)).Select(x => x.Value)), p.OrderPrice);

        #endregion
    }
}
