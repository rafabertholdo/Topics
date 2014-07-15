using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Topics.WebApi.Models;
using Topics.WebApi.Repositories;

namespace Topics.WebApi.Controllers
{
    public class TopicController : ApiController
    {
        public TopicRepository TopicRepository { get; set; }

        public TopicController(TopicRepository topicRepository)
        {
            TopicRepository = topicRepository;
        }

        public IEnumerable<Topic> GetTopics()
        {
            return TopicRepository.GetTrendingTopics();
        }        

        // POST: api/Topic
        [ResponseType(typeof(Topic))]
        public IHttpActionResult PostTopic(Topic topic)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }           

            try
            {
                TopicRepository.CreateTopicIfNotExits(topic.Name);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                return Conflict();                
            }

            return CreatedAtRoute("DefaultApi", new { id = topic.Name }, topic);
        }
    }
}
