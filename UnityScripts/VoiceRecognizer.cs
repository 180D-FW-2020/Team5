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

    public Text voicedText = null;

    void Start()
    {

        actions.Add("good job", GoodJob);
        actions.Add("good game", GoodGame);
        actions.Add("let's go", LetsGo);
        actions.Add("yes", Yes);
        actions.Add("no", No);
        actions.Add("nice putt", NicePutt);

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
        Debug.Log("listening!");
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    private void GoodJob()
    {
        voicedText.text = "you said: Good Job!";
    }

    private void GoodGame()
    {
        voicedText.text = "you said: Good Game!";
    }

    private void LetsGo()
    {
        voicedText.text = "you said: Let's go!";
    }

    private void Yes()
    {
        voicedText.text = "you said: Yes!";
    }

    private void No()
    {
        voicedText.text = "you said: No!";
    }

    private void NicePutt()
    {
        voicedText.text = "you said: Nice Putt!";
    }

}
