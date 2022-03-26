using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;


public class Client : MonoBehaviour
{
    public GameObject myCube;

    private static byte[] outBuffer = new byte[512];
    private static IPEndPoint remoteEP;
    private static Socket client_socket;

    private float[] pos;
    private byte[] bytepos;

    private Vector3 PreviousPosition;

    public static void GetIP(UnityEngine.UI.InputField IPinput)
    {
        IPAddress ip = IPAddress.Parse(IPinput.text);//127.0.0.1//192.168.2.144");
        remoteEP = new IPEndPoint(ip, 11111);

        client_socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    void Start()
    {
        myCube = GameObject.Find("Cube");

        pos = new float[] { myCube.transform.position.x, myCube.transform.position.y, myCube.transform.position.z };
        bytepos = new byte[pos.Length * 4];

        PreviousPosition = myCube.transform.position;

    }
    void Update()
    {
        if (myCube.transform.position != PreviousPosition)
        {
            pos = new float[] { myCube.transform.position.x, myCube.transform.position.y, myCube.transform.position.z };

            Buffer.BlockCopy(pos, 0, bytepos, 0, bytepos.Length);

            client_socket.SendTo(bytepos, remoteEP);
        }
        PreviousPosition = myCube.transform.position;

    }
}