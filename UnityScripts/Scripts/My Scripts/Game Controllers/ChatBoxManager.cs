using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatBoxManager : MonoBehaviour
{

    public int maxMessages = 25;

    public GameObject chatPanel, textObject;

    [SerializeField]
    List<Message> messageList = new List<Message>();

    // Can only call SendMessageToChat during an Update() call
    string messageBuffer;

    // Start is called before the first frame update
    void Start()
    {
        messageBuffer = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (messageBuffer != "")
        {
            if (messageList.Count >= maxMessages)
            {
                Destroy(messageList[0].textObject.gameObject);
                messageList.Remove(messageList[0]);
            }
            Message newMessage = new Message();
            newMessage.text = messageBuffer;

            GameObject newText = Instantiate(textObject, chatPanel.transform);

            newMessage.textObject = newText.GetComponent<Text>();

            newMessage.textObject.text = newMessage.text;

            messageList.Add(newMessage);

            messageBuffer = "";
        }
    }

    public void SendMessageToChat(string text)
    {
        messageBuffer = text;
        /*if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
        }
        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);

        newMessage.textObject = newText.GetComponent<Text>();

        newMessage.textObject.text = newMessage.text;

        messageList.Add(newMessage);*/
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public Text textObject;
}