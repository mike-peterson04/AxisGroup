// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using Microsoft.FSharp.Collections;

namespace Shared.Order
{
    // Order DTO
    public record Order(int OrderNumber, DateTime StartedUtc, bool Open, DateTime? ClosedUtc, FSharpList<MenuItem> UnservedItems, FSharpList<MenuItem> ServedItems, double OrderPrice);
}
