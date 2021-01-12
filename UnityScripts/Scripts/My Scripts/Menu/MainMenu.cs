using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public MqttController mqtt;
    public StateController sc;

    public void PlaySinglePlayer()
    {
        sc.SetState(GameState.DETECTING_POSE);
        mqtt.Connect(GameConstants.DEFAULT_MQTT_TOPIC);
        mqtt.Publish("startPose");
        sc.isSinglePlayer = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PlayMultiPlayer()
    {
        sc.SetState(GameState.DETECTING_POSE);
        sc.isSinglePlayer = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
