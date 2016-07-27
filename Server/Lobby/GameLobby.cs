using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using Server.Room;
using Server.Host;

using NetworkingMessages.Messages;
using NetworkingMessages.Messages.ClientState;
using NetworkingMessages.Messages.Lobby;
using NetworkingMessages.Messages.Lobby.Chat;

namespace Server.Lobby
{
    public class GameLobby : LobbyInstance
    {
        public GameLobby() : base()
        {
            RoomManager.RoomAdded += RoomManager_RoomChanged;
            RoomManager.RoomUpdated += RoomManager_RoomChanged;
            RoomManager.RoomRemoved += RoomManager_RoomRemoved;
        }

        private void RoomManager_RoomRemoved(object sender, RoomManager.RoomEventArgs e)
        {
            RoomListUpdateMessage msg = new RoomListUpdateMessage();
            msg.RoomID = e.AffectedRoom.ID;
            msg.RoomName = string.Empty;
            SendToAll(msg);
        }

        private void RoomManager_RoomChanged(object sender, RoomManager.RoomEventArgs e)
        {
            RoomListUpdateMessage msg = new RoomListUpdateMessage();
            msg.RoomID = e.AffectedRoom.ID;
            msg.RoomName = e.AffectedRoom.Name;
            SendToAll(msg);
        }

        public override void AddPeer(Peer peer)
        {
            base.AddPeer(peer);

            peer.SendMessage(SetClientState.LobbyState);

            SendToAll(new ChatMemberStatusMessage(peer.DisplayName, ChatMemberStatusMessage.StatusTypes.Joined));
            SendRoomList(peer);
        }

        public override void PeerDisconnected(string reason, Peer peer)
        {
            base.PeerDisconnected(reason, peer);
            SendToAll(new ChatMemberStatusMessage(peer.DisplayName, ChatMemberStatusMessage.StatusTypes.Disconnected));
        }

        public override void PeerReceiveMessage(NetworkMessage msg, Peer peer)
        {
            base.PeerReceiveMessage(msg, peer);

            RequestRoomListUpdateMessage requestList = msg as RequestRoomListUpdateMessage;
            if (requestList != null)
            {
                SendRoomList(peer);
                return;
            }

            SendChatMessage chat = msg as SendChatMessage;
            if (chat != null && chat.Message != string.Empty)
            {
                chat.From = peer.DisplayName;
                SendToAll(chat);
                return;
            }

            JoinRoomMessage join = msg as JoinRoomMessage;
            if (join != null)
            {
                RoomInstance room = RoomManager.GetRoom(join.RoomID);
                if (room == null)
                {
                    peer.SendMessage(SetClientState.LobbyState);
                    SendRoomList(peer);
                }
                else
                {
                    SendToAll(new ChatMemberStatusMessage(peer.DisplayName, ChatMemberStatusMessage.StatusTypes.Left));
                    RemovePeer(peer);
                    room.AddPlayer(peer);
                }

                return;
            }

            CreateRoomMessage create = msg as CreateRoomMessage;
            if(create != null && create.RoomName != string.Empty)
            {
                RoomManager.CreateRoom(peer, create);
                return;
            }
        }

        protected virtual void SendRoomList(Peer peer)
        {
            foreach(var r in RoomManager.GetRoomList())
                peer.SendMessage(new RoomListUpdateMessage(r));
        }
    }
}
