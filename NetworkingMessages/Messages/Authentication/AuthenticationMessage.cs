using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.Authentication
{
	public class AuthenticationMessage : NetworkMessage
	{
		public string UserID = string.Empty;
		public string AuthenticationToken = string.Empty;
		public string AuthenticationSourceToken = string.Empty;
	}
}
