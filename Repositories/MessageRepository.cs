using ChatApplication.Models;
using ChatApplication.Repositories.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ChatApplication.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _username, _password, _hostName, _virtualHost;
        private readonly int _port;
        private readonly AppDbContext _context;
        public MessageRepository(IConfiguration configuration, AppDbContext context)
        {
            _username = configuration["RabbitMQ:Username"];
            _password = configuration["RabbitMQ:Password"];
            _hostName = configuration["RabbitMQ:HostName"];
            _virtualHost = configuration["RabbitMQ:VirtualHost"];
            _context = context;

            string portValue = configuration["RabbitMQ:Port"];
            if (!int.TryParse(portValue, out _port))
                throw new ArgumentException("RabbitMQ:Port is not a valid integer.");


        }
        #region Receive Message     
        public string ReceiveMessage(IConnection con, string queueName)
        {
            try
            {
                using var channel = con.CreateModel();
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var result = channel.BasicGet(queue: queueName, autoAck: true);
                if (result != null)
                {
                    byte[] bodyBytes = result.Body.ToArray();
                    var message = Encoding.UTF8.GetString(bodyBytes);
                    // Console.WriteLine(message);
                    return message;
                }
                Console.WriteLine("Null message");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Receive Error: " + ex.Message);
                return null;
            }
        }

        #endregion

        #region RabbitMQ Connection
        public bool IsRabbitMQConnected()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    UserName = _username,
                    Password = _password,
                    VirtualHost = _virtualHost,
                    Port = _port
                };

                using var connection = factory.CreateConnection();
                return connection.IsOpen; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("RabbitMQ Connection Error: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Send Message

        public async Task<bool> SendMessageAsync(IConnection connection, string message, int senderId, int receiverId, string receiverQueue)
        {
            try
            {
                // Save message in database
                var msg = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    MessageText = message,
                    CreatedAt = DateTime.UtcNow
                };

                // _context.Messages.Add(msg);
                // await _context.SaveChangesAsync();

                // Prepare and publish to RabbitMQ
                var emailMessage = new EmailMessage
                {
                    SenderEmail = senderId.ToString(), 
                    MessageContent = message
                };
                var serializedMessage = JsonSerializer.Serialize(emailMessage);

                using var channel = connection.CreateModel();
                channel.QueueDeclare(receiverQueue, true, false, false, null);
                channel.ExchangeDeclare("messageexchange", ExchangeType.Direct);
                channel.QueueBind(receiverQueue, "messageexchange", receiverQueue, null);

                var body = Encoding.UTF8.GetBytes(serializedMessage);
                channel.BasicPublish("messageexchange", receiverQueue, null, body);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendMessage Error: " + ex.Message);
                return false;
            }
        }

        public IConnection GetConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost", 
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/" 
            };

            try
            {
                return factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine("RabbitMQ connection failed: " + ex.Message);
                return null;
            }
        }
        #endregion
    }
}
