using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;


public class Client : MonoBehaviour
{
    public Transform myCube;

    private static byte[] outBuffer = new byte[512];
    private static EndPoint remoteEP;
    private static Socket tcpSocket;
    private static Socket udpSocket;

    private float[] pos =  new float[3];
    private byte[] bytepos = new byte[sizeof(int) + sizeof(float) * 3];
	private int[] intArr = new int[1];
	public static int id = -1;

    private Vector3 previousPosition;

	int recv = 0;
	static bool connected = false;
    public void GetIP(UnityEngine.UI.InputField IPinput)
    {
        IPAddress ip = IPAddress.Parse(IPinput.text);
        remoteEP = new IPEndPoint(ip, 11111);
        tcpSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		tcpSocket.Connect(remoteEP);

		recv = tcpSocket.Receive(outBuffer);

		//if recv is not valid, then we're in trouble
		string code = Encoding.ASCII.GetString(outBuffer, 0, recv);
		int index = code.IndexOf('\t');
		id = int.Parse(code.Substring(0, index));

		//store the id inside the bytepos array, shouldnt get overwritten
		intArr[0] = id;
		Buffer.BlockCopy(intArr, 0, bytepos, 0, sizeof(int));

		code = code.Substring(index + 1);
		index = code.IndexOf('\t');
		//if there's more we need to create more players
		while (index > 0) {
			CreateNewPlayer(int.Parse(code.Substring(0, index)));
			code = code.Substring(index + 1);
			index = code.IndexOf('\t');
		}
		tcpSocket.Blocking = false;

        udpSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		connected = true;
    }

    void Start()
    {
        previousPosition = myCube.position;
    }

    void Update()
    {
		if (!connected)	return;

        if (myCube.position != previousPosition)
        {
            pos[0] = myCube.position.x;
            pos[1] = myCube.position.y;
            pos[2] = myCube.position.z;

            Buffer.BlockCopy(pos, 0, bytepos, sizeof(int), sizeof(float) * 3);

            udpSocket.SendTo(bytepos, remoteEP);

        	previousPosition = myCube.transform.position;
        }

		//listen for other stuff
		//first tcp
		try {
			recv = tcpSocket.ReceiveFrom(outBuffer, ref remoteEP);

			if (recv >= 3) {
				//get the code first
				string code = Encoding.ASCII.GetString(outBuffer, 0, 3);

				//new player joined, we can deal with this later
				if (code == "JNS") {
					//get their id
					CreateNewPlayer(int.Parse(Encoding.ASCII.GetString(outBuffer, 3, recv - 3)));
				}
				//text chat
				else if (code == "MSG") {
					//jsut display it
				}
				//a player left
				else if (code == "LVS") {
					int id = int.Parse(Encoding.ASCII.GetString(outBuffer, 3, recv - 3));
				}
			}
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Debug.Log(sock.ToString());
			}
		}
		catch (Exception e) {
			Debug.Log(e.ToString());
		}

		//udp check after that
		recv = udpSocket.Receive(outBuffer);

		if (recv < 0)	return;
		
		//get the id first
		Buffer.BlockCopy(outBuffer, 0, intArr, 0, sizeof(int));
		//then the position
		Buffer.BlockCopy(outBuffer, sizeof(int), pos, 0, sizeof(float) * 3);

		Vector3 newPos = Vector3.zero;
		newPos.x = pos[0];
		newPos.y = pos[1];
		newPos.z = pos[2];

		MovePlayer(intArr[0], newPos);
    }

	void CreateNewPlayer(int playerId) {
		//does nothign for now
	}

	void MovePlayer(int playerId, Vector3 newPos) {
		//does nothing for now
	}

	void OnApplicationQuit() {
		if (!connected)	return;

		//send leave message
		tcpSocket.SendTo(Encoding.ASCII.GetBytes("LVS" + id.ToString()), remoteEP);
	}
}