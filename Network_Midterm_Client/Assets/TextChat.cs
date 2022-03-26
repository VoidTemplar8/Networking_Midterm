using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChat : MonoBehaviour
{
    public Text textBox;
    public InputField inputmsg;
    public InputField username;
    

    public void DisplayMessage (string message)
    {
        textBox.text += "\n" + message;
    }

    public void SendTextChat()
    {
		if (inputmsg.text == "")	return;
        Client.SendTextChat(username.text + ": " + inputmsg.text);
		inputmsg.text = "";
    }
    
}
