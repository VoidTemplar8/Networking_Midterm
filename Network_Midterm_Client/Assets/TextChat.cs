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

    public void SendMessage()
    {
        string message = "MSG" + username.text + ": " + inputmsg.text;
        
    }
    
}
