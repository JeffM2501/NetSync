using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using Server.Lobby;

namespace Server.Host
{
    internal class ServerHost
    {
        public static string PeerConfigName = "NetSync.0.0.1";

        NetServer SocketServer = null;
        int ActualPort = -1;

        public void Listen(int port)
        {
            if (port == 0)
                port = new Random().Next(1024, Int16.MaxValue - 10);
            NetPeerConfiguration config = new NetPeerConfiguration(PeerConfigName);

            config.AutoFlushSendQueue = true;
            config.MaximumConnections = 200;
            config.ConnectionTimeout = 10;
#if (DEBUG)
            config.ConnectionTimeout = 100;
#endif 
            config.Port = port;
            SocketServer = new NetServer(config);
            SocketServer.Start();

            ActualPort = port;
        }

        public void Shutdown()
        {
            SocketServer.Shutdown("Closing");
            SocketServer = null;
        }

        public event EventHandler LogLineAdded = null;

        protected List<string> LogLines = new List<string>();
        public string GetLogLine()
        {
            lock (LogLines)
            {
                if (LogLines.Count == 0)
                    return string.Empty;

                string l = LogLines[0];
                LogLines.RemoveAt(0);
                return l;
            }
        }

        protected void AddLogLine(string text)
        {
            lock (LogLines)
                LogLines.Add(text);

            LogLineAdded?.Invoke(this, EventArgs.Empty);
        }

        protected object Locker = new object();
        protected bool Connected = false;

        public bool IsConnected
        {
            get { lock (Locker) return Connected; }
        }

        protected Dictionary<long, Peer> ConnectedPeers = new Dictionary<long, Peer>();

        public void ProcessSockets()
        {
            NetIncomingMessage im;
            while ((im = SocketServer.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        AddLogLine(im.ReadString());
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        string reason = im.ReadString();
                        AddLogLine(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);

                        if (status == NetConnectionStatus.Connected)
                        {
                            lock (Locker)
                                Connected = true;

                            AddLogLine("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());
                            var peer = Server.Lobby.Manager.AcceptPeer(im.SenderConnection);
                            if (peer == null)
                                im.SenderConnection.Disconnect("Denied");

                        }
                        else if (status == NetConnectionStatus.Disconnected)
                        {
                            lock (Locker)
                                Connected = false;
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        //    AddInboundMessage(NetworkMessageManager.Deserialize(im));
                        break;
                    default:
                        AddLogLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                        break;
                }
                SocketServer.Recycle(im);
            }
        }
    }
}
