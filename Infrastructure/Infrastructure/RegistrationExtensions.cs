// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Internal.Infrastructure;
using Shared;
using System.Reflection;
using System.Linq;
using System.IO;

namespace Infrastructure.Registration
{
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Register our example solution's required messaging infrastructure
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static void RegisterInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<InMemoryContext>();
            services.AddHostedService<MediatorQueueService>();
            services.AddSingleton<MediatorQueue>();
            services.AddTransient<IQueuedMediator, ParallelQueuedMediator>();
            services.AddMediatR(cfg => { cfg.Using<ParallelQueuedMediator>(); }, GetSolutionAssemblies());
        }

        /// <summary>
        /// Scan all assemblies in solution. This will let us pick up any "microservices" with handlers where there was not an assembly reference during startup
        /// </summary>
        /// <returns></returns>
        private static Assembly[] GetSolutionAssemblies() 
            => Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x))).ToArray();
    }
}
