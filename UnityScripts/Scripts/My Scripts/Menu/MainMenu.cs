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

        // Set the initial player putting index to 0 (host putts first)
        sc.SetPlayerPuttingIndex(0);

        // Beginning at level 0
        sc.SetLevel(0);

        // If host, send out first startPose message
        if (mqtt.isHost)
        {
            // Start game at level 0 (notify all guests that game has started)
            mqtt.Publish("startGame," + sc.GetLevel().ToString());

            // Ask host pose detector to reply with poseOK
            mqtt.Publish(mqtt.playerNames[sc.GetPlayerPuttingIndex()] + ",startPose");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
