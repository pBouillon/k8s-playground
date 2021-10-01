using Api.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public record CreateMessageRequest(string content);

    [ApiController]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
            => _messageService = messageService;

        [HttpGet]
        public ActionResult<IEnumerable<Message>> GetMessages()
            => Ok(_messageService.GetMessages());

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
            => Ok(await _messageService.GetMessage(id));

        [HttpPost]
        public async Task<IActionResult> CreateMessages(CreateMessageRequest payload)
        {
            var message = await _messageService.CreateMessage(payload.content);
            return CreatedAtAction(nameof(GetMessage), new { Id = message.Id }, message);
        }
    }
}
