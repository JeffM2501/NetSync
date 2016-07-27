using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.Lobby.Chat
{
    public class SendChatMessage: NetworkMessage
    {
        public string From = string.Empty;
        public string Message = string.Empty;
    }
}
