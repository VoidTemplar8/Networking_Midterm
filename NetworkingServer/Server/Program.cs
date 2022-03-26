using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

class Server
{
	class Player
	{
		public Socket tcpSock;
		public IPEndPoint tcpRemoteEP;
		public IPEndPoint udpRemoteEP;
		public int id;

		public Player(Socket handler, int id, int udpPort) {
			this.id = id;
			tcpSock = handler;
			tcpRemoteEP = (IPEndPoint)tcpSock.RemoteEndPoint;
			tcpSock.Blocking = false;
			
			udpRemoteEP = new IPEndPoint(tcpRemoteEP.Address, udpPort);
		}

		public int SendTCP(in byte[] message, int size) {
			return tcpSock.SendTo(message, size, SocketFlags.None, tcpRemoteEP);
		}

		public int SendUDP(in byte[] message, int size) {
			return Server.udpServer.SendTo(message, size, SocketFlags.None, udpRemoteEP);
		}
	}
	
	static Socket tcpServer;
	static Socket udpServer;
	static IPAddress ip = null;

	static int udpPort = 11112;

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
		udpServer.Bind(udpRemote);
		udpServer.Blocking = false;

		Console.WriteLine("Starting Server");
	}

	static Socket tempHandler = null;
	static EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
	static IPEndPoint remoteIP = null;

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
			string otherPlayers = ++udpPort + "\t" + id.ToString() + "\t";
			foreach (Player other in players) {
				other.SendTCP(message, message.Length);
				otherPlayers += other.id.ToString() + "\t";
			}
			tempHandler.SendTo(Encoding.ASCII.GetBytes(otherPlayers),
					(IPEndPoint)tempHandler.RemoteEndPoint);

			players.Add(new Player(tempHandler, id, udpPort));

			Console.WriteLine("User at " + ((IPEndPoint)tempHandler.RemoteEndPoint).Address
					+ " joined with id: " + id.ToString() + " and udp port " + udpPort
					+ " notifying all other users");

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

		for (int i = 0; i < players.Count;) {
			Player player = players[i];

			//tcp checks
			try {
				recv = player.tcpSock.Receive(buffer);
				if (recv >= 3) {
					string code = Encoding.ASCII.GetString(buffer, 0, 3);
					Console.WriteLine(code);

					if (code == "MSG") {
						Console.WriteLine("User at " + ((IPEndPoint)player.udpRemoteEP).Address
								+ "Sent this message: \"" + Encoding.ASCII.GetString(buffer, 3, recv - 3) + "\"");
						
						//if it recieves something, send it to everyone
						foreach (Player other in players) {
							other.SendTCP(buffer, recv);
						}
					}
					//leave server
					else if (code == "LVS") {
						Console.WriteLine("User at " + ((IPEndPoint)player.udpRemoteEP).Address
								+ " with id: " + player.id.ToString() + "left the app, notifying all other users");

						players.RemoveAt(i);
						//send it to everyone, id should be attached
						foreach (Player other in players) {
							other.SendTCP(buffer, recv);
						}

						//close the socket
						player.tcpSock.Close();
						continue;
					}
				}
			}
			catch (SocketException sock) {
				if (sock.SocketErrorCode != SocketError.WouldBlock) {
					Console.WriteLine(sock.ToString());
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
			++i;
		}

		//udp check
		try {
			recv = udpServer.ReceiveFrom(buffer, ref remote);

			if (recv >= 0) {
				//so we dont need to cast every frame
				remoteIP = (IPEndPoint)remote;
				//do udp sending, ignore the player it came from
				foreach (Player other in players) {
					if (remoteIP.Port == other.udpRemoteEP.Port &&
						remoteIP.Address.GetHashCode() == other.udpRemoteEP.Address.GetHashCode()) {
						//this player moved
						Console.WriteLine("user " + other.id + " moved, sent change to all other users");
						continue;
					}
					other.SendUDP(buffer, recv);
				}
				remoteIP = null;
			}

			remote = new IPEndPoint(IPAddress.Any, 0);
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Console.WriteLine(sock.ToString());
			}
		}
		catch (Exception e) {
			Console.WriteLine(e.ToString());
		}

		return true;
	}

	static void CloseServer() {
		tcpServer.Shutdown(SocketShutdown.Both);
		tcpServer.Close();
		udpServer.Close();
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
		Console.ReadKey();
	}
}
