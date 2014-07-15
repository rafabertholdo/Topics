using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Topics.Framework.Messaging;

namespace Topics.WebApi.Models
{
    public class Message : IEvent
    {
        public Guid SourceId { get; set; }
        public string Content { get; set; }
    }
}