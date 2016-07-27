using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

namespace Server.Host
{
    public interface PeerHandler
    {
        void PeerReceiveData(NetIncomingMessage msg, Peer peer);
        void PeerDisconnected(string reason, Peer peer);

		void DisconnectPeer(string reason, Peer peer);
	}

    public class Peer
    {
        public PeerHandler Handler = null;
        public NetConnection SocketConnection = null;

        private Dictionary<string, object> Attributes = new Dictionary<string, object>();
        private Dictionary<string, double> AttributeNumbers = new Dictionary<string, double>();

        public void SetAttribute(string name, object data)
        {
            if (Attributes.ContainsKey(name))
                Attributes.Remove(name);

            Attributes.Add(name, data);
        }

        public object GetAttribute(string name)
        {
            if (Attributes.ContainsKey(name))
                return null;

            return Attributes[name];
        }

        public void SetAttributeS(string name, string data)
        {
            if (Attributes.ContainsKey(name))
                Attributes.Remove(name);

            Attributes.Add(name, data);
        }

        public void SetAttribute(string name, string data)
        {
            SetAttributeS(name, data);
        }

        public string GetAttributeS(string name)
        {
            if (Attributes.ContainsKey(name))
                return string.Empty;

            return (Attributes[name] as String) ?? string.Empty;
        }

        public void SetAttribute(string name, double data)
        {
            SetAttributeD(name, data);
        }

        public void SetAttributeD(string name, double data)
        {
            if (AttributeNumbers.ContainsKey(name))
                AttributeNumbers.Remove(name);

            AttributeNumbers.Add(name, data);
        }

        public double GetAttributeD(string name)
        {
            if (AttributeNumbers.ContainsKey(name))
                return 0.0;

            return AttributeNumbers[name];
        }

        public void SetAttribute(string name, bool data)
        {
            SetAttributeB(name, data);
        }

        public void SetAttributeB(string name, bool data)
        {
            SetAttributeD(name, data ? 1 : 0);
        }

        public bool GetAttributeB(string name)
        {
            return GetAttributeD(name) != 0;
        }

        public long ID
        {
            get { return (SocketConnection?.RemoteUniqueIdentifier ?? long.MinValue); }
        }
    }
}
