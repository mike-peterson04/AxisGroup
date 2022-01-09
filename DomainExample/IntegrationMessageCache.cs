// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared;

namespace DomainExample
{
    /// <summary>
    /// Provides a human-readable entry for a message emitted on the in-process message bus
    /// </summary>
    public record IntegrationMessageDisplay(string MessageName, string MessageType, DateTime MessageUtc, string MessageSummary);

    /// <summary>
    /// Caches human-readable entries of notifications from the in-process message bus
    /// </summary>
    public class IntegrationMessageCache : Dictionary<Guid, IntegrationMessageDisplay> { }

    public static class MessageType
    {
        public static string Command => "Command";
        public static string Event => "Event";
    }
}
