using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.ClientState
{
    public class SetClientState : NetworkMessage
    {
        public enum ClientStates
        {
            Connecting,
            Authenticating,
            InLobby,
            Playing,
            Disconnecting,
        }

        public ClientStates State = ClientStates.Connecting;

        public SetClientState() { }
        public SetClientState (ClientStates s) { State = s; }

        public static readonly SetClientState ConnectingState = new SetClientState(ClientStates.Connecting);
        public static readonly SetClientState AuthState = new SetClientState(ClientStates.Authenticating);
        public static readonly SetClientState LobbyState = new SetClientState(ClientStates.InLobby);
        public static readonly SetClientState PlayingState = new SetClientState(ClientStates.Playing);
        public static readonly SetClientState DisconnectingState = new SetClientState(ClientStates.Disconnecting);
    }
}
