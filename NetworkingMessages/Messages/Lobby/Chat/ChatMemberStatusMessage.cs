using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.Lobby.Chat
{
    public class ChatMemberStatusMessage : NetworkMessage
    {
        public enum StatusTypes
        {
            Joined,
            Left,
            Disconnected,
        }

        public StatusTypes Status = StatusTypes.Disconnected;

        public string UserName = string.Empty;

        public ChatMemberStatusMessage() { }
        public ChatMemberStatusMessage(string name, StatusTypes st) { UserName = name; Status = st; }
    }
}
