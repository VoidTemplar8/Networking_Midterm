using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

class Server
{
	static IPAddress ip = null;

	class Player
	{
		public Socket tcpSock;
		public IPEndPoint tcpRemoteEP;
		public EndPoint udpRemoteEP;
		public int id;

		public Player(Socket handler, int id) {
			this.id = id;
			tcpSock = handler;
			tcpRemoteEP = (IPEndPoint)tcpSock.RemoteEndPoint;
			udpRemoteEP = new IPEndPoint(tcpRemoteEP.Address, 11112);
			tcpSock.Blocking = false;
		}

		public int SendTCP(in byte[] message, int size) {
			return tcpSock.SendTo(message, size, SocketFlags.None, tcpRemoteEP);
		}

		public int SendUDP(in byte[] message, int size) {
			return Server.udpServer.SendTo(message, size, SocketFlags.None, udpRemoteEP);
		}
	}

	static Socket udpServer;
	static Socket tcpServer;

	static int playerCount = 0;

	static List<Player> players = new List<Player>();

	static byte[] buffer = new byte[512];
	static int recv = 0;

	static void StartServer(string ipInput, int maxPlayers) {
		ip = IPAddress.Parse(ipInput);
		tcpServer = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		IPEndPoint tcpRemote = new IPEndPoint(ip, 11111);
		tcpServer.Bind(tcpRemote);
		tcpServer.Listen(maxPlayers);
		tcpServer.Blocking = false;

		udpServer = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		IPEndPoint udpRemote = new IPEndPoint(ip, 11112);
		udpServer.Blocking = false;
	}

	static Socket tempHandler = null;

	static bool RunServer() {

		//listen for players
		try {
			tempHandler = tcpServer.Accept();
			//if it gets past this a player attempted to connect
			int id = ++playerCount;

			//create the join message
			byte[] message = Encoding.ASCII.GetBytes("JND" + id);


			//send a message to all players that a new player connected
			//also send a message to the player with each current player
			string otherPlayers = id.ToString() + "\t";
			foreach (Player other in players) {
				other.SendTCP(message, message.Length);
				otherPlayers += other.id.ToString() + "\t";
			}
			tempHandler.SendTo(Encoding.ASCII.GetBytes(otherPlayers),
					(IPEndPoint)tempHandler.RemoteEndPoint);

			players.Add(new Player(tempHandler, id));

			tempHandler = null;
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Console.WriteLine(sock.ToString());
				tempHandler = null;
			}
		}
		catch (Exception e) {
			Console.WriteLine(e.ToString());
			tempHandler = null;
		}

		foreach (Player player in players) {
			//tcp checks
			try {
				recv = player.tcpSock.Receive(buffer);

				if (recv >= 3) {
					string code = Encoding.ASCII.GetString(buffer, 0, 3);
					if (code == "MSG") {
						//if it recieves something, send it to everyone
						foreach (Player other in players) {
							other.SendTCP(buffer, recv);
						}
					}
					//leave server
					else if (code == "LVS") {
						//send it to everyone, id should be attached
						foreach (Player other in players) {
							other.SendTCP(buffer, recv);
						}
					}
				}
			}
			catch (SocketException sock) {
				if (sock.SocketErrorCode != SocketError.WouldBlock) {
					Console.WriteLine(sock.ToString());
				}
			}

			//udp check
			recv = udpServer.ReceiveFrom(buffer, ref player.udpRemoteEP);

			if (recv >= 0) {
				//do udp sending, id should be attached
				foreach (Player other in players) {
					if (player == other) continue;
					other.SendUDP(buffer, recv);
				}
			}
		}

		return true;
	}

	static void CloseServer() {
		tcpServer.Close();
	}

	static void Main() {
		Console.Write("Input max player count: ");
		int maxPlayers = int.Parse(Console.ReadLine());
		Console.Write("Input server IP: ");
		string ip = Console.ReadLine();

		try {
			StartServer(ip, maxPlayers);
			while (RunServer()) ;
			CloseServer();
		}
		catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}
}
