// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using Microsoft.FSharp.Collections;
using Shared;

namespace OrderService.Domain
{
    /// <summary>
    /// An order domain aggregate for projecting domain events
    /// </summary>
    public record OrderAggregate(
            int StreamId, 
            int AggregateVersion,
            int ItemCount,
            int ServedCount,
            DateTime StartedUtc, 
            DateTime LastUpdatedUtc, 
            DateTime? ClosedUtc, 
            FSharpMap<int, MenuItem> OrderedItems, 
            FSharpSet<int> ServedItems, 
            double OrderPrice,
            double PaidAmount)
        : DomainAggregate(StreamId, AggregateVersion);
}
