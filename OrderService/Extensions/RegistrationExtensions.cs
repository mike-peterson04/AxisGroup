// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Domain;
using Shared;

namespace OrderService.Registration
{
    /// <summary>
    /// Performs service registration required to enable the Order service
    /// </summary>
    public static class RegistrationExtensions
    {
        public static void RegisterOrderService(this IServiceCollection services)
        {
            services.AddTransient<IDocumentStore<OrderAggregate>, OrderStore>();
            services.AddTransient<IEventStore<OrderDomainEvent>, OrderDomainEventStore>();
        }
    }
}
