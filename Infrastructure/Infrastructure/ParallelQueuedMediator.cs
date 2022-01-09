// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Internal.Infrastructure
{
    public interface IQueuedMediator : IMediator
    {
        bool RedirectToQueue(bool setting);
        bool Dequeue(out INotification notification);
    }

    public class ParallelQueuedMediator : Mediator, IQueuedMediator
    {
        private readonly MediatorQueue _queue;
        private bool _redirectToQueue = true;
        public ParallelQueuedMediator(ServiceFactory serviceFactory, MediatorQueue queue) : base(serviceFactory) => _queue = queue;

        public bool Dequeue(out INotification notification) => _queue.TryDequeue(out notification);

        public bool RedirectToQueue(bool setting) => _redirectToQueue = setting;

        protected override Task PublishCore(IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers, INotification notification, CancellationToken cancellationToken)
        {
            if (_redirectToQueue) _queue.Enqueue(notification);
            else
            {
                foreach (var handler in allHandlers)
                {
                    Task.Run(() => handler(notification, cancellationToken));
                }
            }
            
            return Task.CompletedTask;
        }
    }  

}
