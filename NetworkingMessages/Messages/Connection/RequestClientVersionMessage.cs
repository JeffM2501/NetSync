using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;


namespace NetworkingMessages.Messages.Connection
{
	public class RequestClientVersionMessage : NetworkMessage
	{
		public static readonly RequestClientVersionMessage Request = new RequestClientVersionMessage();
	}
}
