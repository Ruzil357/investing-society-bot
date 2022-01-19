using Discord;
using Discord.WebSocket;

namespace PsnHackathonBot.Services
{
    public interface IMessageLogger
    {
        void OnMessageCreated(SocketUserMessage message);
        
        void OnMessageEdited(SocketUserMessage newMessage, IMessage oldMessage = null);
        
        void OnMessageDeleted(SocketUserMessage message);
    }
}