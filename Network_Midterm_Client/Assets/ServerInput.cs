using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerInput : MonoBehaviour
{
    public string serverIP;
    public GameObject inputText;
    public GameObject displayMsg;

    public void SetServerIP()
    {
        serverIP = inputText.GetComponent<Text>().text;
        displayMsg.GetComponent<Text>().text = "Server IP: " + serverIP;
    }    

}
