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

    public ChatBoxManager chatBoxManager;

    void Start()
    {

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
        chatBoxManager.SendMessageToChat("You: Good Job!");
    }

    private void LetsGo()
    {
        chatBoxManager.SendMessageToChat("You: Let's go!");
    }

    private void Yes()
    {
        Debug.Log("Sending \"Yes\" to chat");
        chatBoxManager.SendMessageToChat("You: Yes!");
    }

    private void No()
    {
        chatBoxManager.SendMessageToChat("You: No!");
    }

    private void NicePutt()
    {
        chatBoxManager.SendMessageToChat("You: Nice Putt!");
    }

    private void GG()
    {
        chatBoxManager.SendMessageToChat("You: GG");
    }

}
