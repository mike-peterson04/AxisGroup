// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DomainExample.Controllers
{
    public class MessageBusController : Controller
    {
        private readonly IntegrationMessageCache _eventCache;

        public MessageBusController(IntegrationMessageCache eventCache) => _eventCache = eventCache;

        /// <summary>
        /// Get all integration messages (commands and events)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMessages")]
        public ActionResult GetMessages() => Ok(_eventCache);
    }
}
