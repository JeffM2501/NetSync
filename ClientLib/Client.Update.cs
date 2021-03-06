﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using NetworkingMessages;
using NetworkingMessages.Messages;
using NetworkingMessages.Messages.Connection;
using NetworkingMessages.Messages.Authentication;
using NetworkingMessages.Messages.ClientState;
using NetworkingMessages.Messages.Lobby;


namespace ClientLib
{
	public partial class Client
	{
		protected AuthenticationResponceMessagecs LastAuthenticationMessage = null;

		protected SetClientState.ClientStates CurrentState = SetClientState.ClientStates.Connecting;

		protected void SetClientConnecting()
		{
			ChangeClientState(SetClientState.ClientStates.Connecting);
		}
		protected void SetClientDisconnecting()
		{
			ChangeClientState(SetClientState.ClientStates.Disconnecting);
		}

		public event EventHandler StartAuthentication = null;
		public event EventHandler JoinedLobby = null;
		public event EventHandler LeftLobby = null;
		public event EventHandler JoinedGame = null;
		public event EventHandler LeftGame = null;
		public event EventHandler Disconnecting = null;


		protected void ChangeClientState(SetClientState.ClientStates state)
		{
			switch(CurrentState)
			{
				case SetClientState.ClientStates.InLobby:
					LeftLobby?.Invoke(this, EventArgs.Empty);
					CurrentLobby = null;
					break;

				case SetClientState.ClientStates.Playing:
					LeftGame?.Invoke(this, EventArgs.Empty);
					CurrentGame = null;
					break;
			}
			CurrentState = state;

			switch(CurrentState)
			{
				case SetClientState.ClientStates.Authenticating:
					StartAuthentication?.Invoke(this, EventArgs.Empty);
					break;

				case SetClientState.ClientStates.InLobby:
					JoinedLobby?.Invoke(this, EventArgs.Empty);
					CurrentLobby = new LobbySession(LastAuthenticationMessage.UserID);
					CurrentLobby.ChatSent += CurrentLobby_ChatSent;
					SendMessage(RequestRoomListUpdateMessage.Request);
					break;

				case SetClientState.ClientStates.Playing:
					CurrentGame = new ClientLib.Game.GameSession();
					JoinedGame?.Invoke(this, EventArgs.Empty);
					break;

				case SetClientState.ClientStates.Disconnecting:
					Disconnecting?.Invoke(this, EventArgs.Empty);
					CurrentGame = null;
					CurrentLobby = null;
					break;
			}

		}

		private void CurrentLobby_ChatSent(object sender, LobbySession.ChatMessageEventArgs e)
		{
			SendMessage(new NetworkingMessages.Messages.Lobby.Chat.SendChatMessage(e.From,e.ChatText));
		}

		public static int MaxMessagesPerTick = 100;

		public void Update()
		{
			if (CheckingThread == null)
			{
				int count = 0;
				while(ProcessOneMessages() && count < MaxMessagesPerTick)
					count++;
			}

			NetworkMessage msg = PopMessage();
			int i = 0;

			while(msg != null && i < MaxMessagesPerTick)
			{
				switch(CurrentState)
				{
					case SetClientState.ClientStates.Connecting:
						ProcessConnectionMessage(msg);
						break;

					case SetClientState.ClientStates.Authenticating:
						ProcessAuthenticationMessage(msg);
						break;

					case SetClientState.ClientStates.InLobby:
						ProcessLobbyMessage(msg);
						break;

					case SetClientState.ClientStates.Playing:
						ProcessGameMessage(msg);
						break;
				}
			}
		}

		protected void ProcessConnectionMessage(NetworkMessage msg)
		{
			SetClientState stateMsg = msg as SetClientState;
			if(stateMsg != null)
			{
				if(stateMsg.State == SetClientState.ClientStates.Authenticating)
				{
					CurrentState = SetClientState.ClientStates.Authenticating;
					return;
				}
			}
		}

		public class AuthenticationCredentialsEventArgs : EventArgs
		{
			public string UserID = string.Empty;
			public string Token = string.Empty;
			public string TokenSource = string.Empty;

			public bool SendNow = true;
		}

		public event EventHandler<AuthenticationCredentialsEventArgs> GetAuthenticationCredentials = null;

		public event EventHandler AuthenticationAccepted = null;
		public event EventHandler AuthenticationRejected = null;

		protected void ProcessAuthenticationMessage(NetworkMessage msg)
		{
			SetClientState stateMsg = msg as SetClientState;
			if(stateMsg != null)
			{
				if(stateMsg.State == SetClientState.ClientStates.InLobby || stateMsg.State == SetClientState.ClientStates.Playing || stateMsg.State == SetClientState.ClientStates.Disconnecting)
				{
					ChangeClientState(stateMsg.State);
					return;
				}
			}

			RequestClientVersionMessage versMsg = msg as RequestClientVersionMessage;
			if(versMsg != null)
			{
				ClientVersionMessage myVersion = new ClientVersionMessage();
				SendMessage(myVersion);
			}

			RequestAuthentication authRequestMsg = msg as RequestAuthentication;
			if(authRequestMsg != null)
			{
				SendAuthentication();
				return;
			}


			AuthenticationResponceMessagecs authResponce = msg as AuthenticationResponceMessagecs;
			if(authResponce != null)
			{
				LastAuthenticationMessage = authResponce;

				if(LastAuthenticationMessage.Responce == AuthenticationResponceMessagecs.AuthenticationResponces.Accepted)
					AuthenticationAccepted?.Invoke(this, EventArgs.Empty);
				else
				{
					AuthenticationRejected?.Invoke(this, EventArgs.Empty);
					SendAuthentication();
				}
				return;
			}
		}

		protected void SendAuthentication()
		{
			AuthenticationMessage myAuth = new AuthenticationMessage();

			AuthenticationCredentialsEventArgs args = new AuthenticationCredentialsEventArgs();
			if (GetAuthenticationCredentials != null)
			{
				GetAuthenticationCredentials.Invoke(this, args);
				if(!args.SendNow)
					return;
			}
		
			myAuth.UserID = args.UserID;
			myAuth.AuthenticationToken = args.Token;
			myAuth.AuthenticationSourceToken = args.TokenSource;

			SendMessage(myAuth);
		}

		public void JoinGameRoom(long gameID)
		{
			switch( CurrentState)
			{
				case SetClientState.ClientStates.InLobby:
					SendMessage(new JoinRoomMessage(gameID));
					break;

				case SetClientState.ClientStates.Playing:
					// TODO, kill our current game and part
					SendMessage(new JoinRoomMessage(gameID));
					break;
			}
		}

		protected void ProcessLobbyMessage(NetworkMessage msg)
		{
			SetClientState stateMsg = msg as SetClientState;
			if(stateMsg != null)
			{
				if(stateMsg.State == SetClientState.ClientStates.InLobby || stateMsg.State == SetClientState.ClientStates.Playing || stateMsg.State == SetClientState.ClientStates.Disconnecting)
				{
					ChangeClientState(CurrentState);
					return;
				}
			}

			if(CurrentLobby != null)
				CurrentLobby.ProcessMessage(msg);

		}

		protected void ProcessGameMessage(NetworkMessage msg)
		{
			SetClientState stateMsg = msg as SetClientState;
			if(stateMsg != null)
			{
				if(stateMsg.State == SetClientState.ClientStates.InLobby || stateMsg.State == SetClientState.ClientStates.Playing || stateMsg.State == SetClientState.ClientStates.Disconnecting)
				{
					ChangeClientState(stateMsg.State);
					return;
				}
			}

			if(CurrentGame != null)
				CurrentGame.ProcessMessage(msg);
		}
	}
}
