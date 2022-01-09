// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System.Collections.Concurrent;
using MediatR;

namespace Internal.Infrastructure
{
    public class MediatorQueue : ConcurrentQueue<INotification> { }

}
