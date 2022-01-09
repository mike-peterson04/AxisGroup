// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Internal.Infrastructure
{
    public class MediatorQueueService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IQueuedMediator _mediator;

        public MediatorQueueService(IServiceScopeFactory serviceScopeFactory, ILogger<MediatorQueueService> logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(MediatorQueueService)} is starting.");
            using var scope = _serviceScopeFactory.CreateScope();
            _mediator = scope.ServiceProvider.GetRequiredService<IQueuedMediator>();
            _mediator.RedirectToQueue(false);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_mediator.Dequeue(out INotification notification)) await _mediator.Publish(notification, stoppingToken);
                await Task.Delay(50, stoppingToken);
            }
        }
    }

}
