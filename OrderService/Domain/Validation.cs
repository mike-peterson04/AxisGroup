// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Linq;
using FluentValidation;
using Microsoft.FSharp.Collections;

namespace OrderService.Domain
{
    /// <summary>
    /// Validation rules for an order aggregate projection
    /// </summary>
    public class OrderAggregateValidator : AbstractValidator<OrderAggregate>
    {
        public OrderAggregateValidator()
        {
            RuleFor(x => x.StreamId).GreaterThan(0).WithMessage("An order must have a valid order number associated with it.");
            RuleFor(x => x.StartedUtc).NotEmpty().WithMessage("An order must have a valid, non-default start time.");
            RuleFor(x => x.OrderPrice).GreaterThanOrEqualTo(0).WithMessage("An order must have a total price greater than or equal to zero.");
            RuleFor(x => x.LastUpdatedUtc).NotEqual(x => x.StartedUtc).When(x => x.AggregateVersion > 1).WithMessage("An order can only be opened once, and this must be its first action.");
            RuleFor(x => x.ServedItems).Must((x, served) => served.IsSubsetOf(SetModule.OfSeq(x.OrderedItems.Select(x => x.Key)))).WithMessage("All served items must have been previously ordered.");
            RuleFor(x => x.ServedCount).Equal(x => x.ServedItems.Count).WithMessage("The expected count of served items must match the actual served items. This may have been a duplicate operation for a provided item.");
            RuleFor(x => x.ItemCount).Equal(x => x.OrderedItems.Count).WithMessage("The expected count of ordered items must match the actual ordered items. This may have been a duplicate operation for a provided item.");
            RuleFor(x => x.ClosedUtc).Null().When(x => x.ServedCount < x.ItemCount).WithMessage("An order cannot be closed and have unserved items. Please wait for remaining items to be served or add items to a new order.");
            RuleFor(x => x.ClosedUtc).NotEmpty().When(x => x.PaidAmount > 0).WithMessage("A payment can only be applied at order closing.");
            RuleFor(x => x.PaidAmount).LessThanOrEqualTo(x => x.OrderPrice).WithMessage("Payment cannot exceed the order price.");
        }
    }
}
