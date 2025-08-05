using RabbitMQ.Client;

namespace ChatApplication.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        public IConnection GetConnection();

        public Task <bool> SendMessageAsync(IConnection con,string message, int senderId, int receiverId ,string queue);
        public string ReceiveMessage(IConnection con, string queue);
    }
}
