using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class VoiceRecognizer : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    private MqttController mqtt;

    public ChatBoxManager chatBoxManager;

    void Start()
    {
        // Find mqtt controller object
        mqtt = GameObject.FindGameObjectWithTag("MqttController").GetComponent<MqttController>();

        actions.Add("good job", GoodJob);
        actions.Add("let's go", LetsGo);
        actions.Add("yes", Yes);
        actions.Add("no", No);
        actions.Add("nice putt", NicePutt);
        actions.Add("gee gee", GG);

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
        Debug.Log("Listening for speech...");
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    private void GoodJob()
    {
        //chatBoxManager.SendMessageToChat("You: Good Job!");
        mqtt.Publish("newMessage," + mqtt.myNickname + ",Good job");
    }

    private void LetsGo()
    {
        //chatBoxManager.SendMessageToChat("You: Let's go!");
        mqtt.Publish("newMessage," + mqtt.myNickname + ",Let's go");
    }

    private void Yes()
    {
        //chatBoxManager.SendMessageToChat("You: Yes!");
        mqtt.Publish("newMessage," + mqtt.myNickname + ",Yes");
    }

    private void No()
    {
        //chatBoxManager.SendMessageToChat("You: No!");
        mqtt.Publish("newMessage," + mqtt.myNickname + ",No");
    }

    private void NicePutt()
    {
        //chatBoxManager.SendMessageToChat("You: Nice Putt!");
        mqtt.Publish("newMessage," + mqtt.myNickname + ",Nice putt");
    }

    private void GG()
    {
        //chatBoxManager.SendMessageToChat("You: GG");
        mqtt.Publish("newMessage," + mqtt.myNickname + ",GG");
    }

}
