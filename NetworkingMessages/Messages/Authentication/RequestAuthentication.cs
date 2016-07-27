using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;


namespace NetworkingMessages.Messages.Authentication
{
	public class RequestAuthentication : NetworkMessage
	{
		public static readonly RequestAuthentication Request = new RequestAuthentication();
	}
}