using System.Threading.Tasks;

namespace Shared
{
    public interface IOutboxRepository
    {
        Task<OutboxMessageModel> PopOutboxMessageAsync();
        Task ReturnOutboxMessageToPendingAsync(int messageId);
        Task DeleteOutboxMessageAsync(int messageId);
    }
}