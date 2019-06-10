
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.WeChat.Models;

namespace Itminus.InDirectLine.WeChat.Services
{
    public interface IWeixinUserConversationStore
    {
        Task<ConversationInfo> GetConversationAsync(string userId);

        Task<bool> StoreAsync(string userId, DirectLineConversation conversation , string watermark="");
    }

}