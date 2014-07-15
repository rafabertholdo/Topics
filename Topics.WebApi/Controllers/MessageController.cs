using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Topics.WebApi.Models;
using Topics.WebApi.Repositories;

namespace Topics.WebApi.Controllers
{
    public class MessageController : ApiController
    {
        public TopicRepository TopicRepository { get; set; }

        public MessageController(TopicRepository topicRepository)
        {
            TopicRepository = topicRepository;
        }

        [ResponseType(typeof(IEnumerable<Message>))]
        [Route("api/message/{topicName}/{iteration}")]
        public IHttpActionResult GetMessages(string topicName, int iteration)
        {
            var result = TopicRepository.GetMessages(topicName, iteration);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST: api/Mesage
        [ResponseType(typeof(Message))]
        public IHttpActionResult PostMessage(PostMessageDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var message = new Message
            {
                SourceId = Guid.NewGuid(),
                Content = dto.MessageContent
            };

            TopicRepository.PostMessage(dto.TopicName, message);            

            return CreatedAtRoute("DefaultApi", new { id = message.SourceId }, message);
        }

        public class PostMessageDTO
        {
            public string TopicName { get; set; }
            public string MessageContent { get; set; }
        }
    }
}
