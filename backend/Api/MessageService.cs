using Api.Entities;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api
{
    public class MessageService
    {
        private readonly ApplicationContext _context;

        private readonly ILogger<MessageService> _logger;

        public MessageService(ApplicationContext context, ILogger<MessageService> logger)
            => (_context, _logger) = (context, logger);

        public Task<Message> GetMessage(int id, CancellationToken cancellationToken = default)
            => _context.Messages.SingleAsync(message => message.Id == id, cancellationToken);

        public IEnumerable<Message> GetMessages()
            => _context.Messages.ToList();

        public async Task<Message> CreateMessage(string content, CancellationToken cancellationToken = default)
        {
            var entity = new Message(0, content);

            await _context.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created: {}", entity);

            await PublishToRabbit(entity, cancellationToken);
            _logger.LogInformation("Sent to RabbitMQ: {}", entity);

            return entity;
        }
    
        private async Task PublishToRabbit(Message message, CancellationToken cancellationToken = default)
        {
            var routingKey = nameof(Message);
            var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

            try
            {
                using (var advancedBus = RabbitHutch.CreateBus($"host={rabbitMqHost}").Advanced)
                {
                    var queue = advancedBus.QueueDeclare("Api.Message");
                    var exchange = advancedBus.ExchangeDeclare("Api.Exchange.Message", ExchangeType.Topic);

                    var binding = advancedBus.Bind(exchange, queue, routingKey);

                    await advancedBus
                        .PublishAsync(exchange, routingKey, false, new Message<Message>(message), cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "Unable to reach broker on host '{}', the following message has not been sent: {}", rabbitMqHost, message);
                throw;
            }

            
        }
    }
}
