// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;

namespace Shared
{
    // A domain event with at least a stream ID and event ID
    public abstract record DomainEvent(int StreamId, int EventId);

    // A domain aggregate with at least a stream ID and aggregate version
    public abstract record DomainAggregate(int StreamId, int AggregateVersion);
}
