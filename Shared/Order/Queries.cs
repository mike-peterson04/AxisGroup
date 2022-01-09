// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using MediatR;

namespace Shared.Order
{
    public record GetOrder(int OrderNumber) 
        : IRequest<Order>;

    public record GetOrders()
        : IRequest<IEnumerable<Order>>;
}
