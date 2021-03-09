using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeyManager : MonoBehaviour
{

    public MqttController mqtt;
    public StateController sc;

    private float leftArrowDownTime, rightArrowDownTime, spaceDownTime;

    void Start()
    {
        leftArrowDownTime = 0.0f;
        rightArrowDownTime = 0.0f;
        spaceDownTime = 0.0f;

        // Find mqtt controller object
        mqtt = GameObject.FindGameObjectWithTag("MqttController").GetComponent<MqttController>();

        // Get state controller object
        sc = GameObject.FindGameObjectWithTag("StateController").GetComponent<StateController>();

        if (!mqtt)
        {
            Debug.Log("HotkeyManager: Mqtt controller not found");
        }
        if (!sc)
        {
            Debug.Log("HotkeyManager: State controller not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Quit game
        if (Input.GetKeyDown("escape"))
        {
            Debug.Log("Quit");
            Application.Quit();
        }

        // PoseOK
        if (Input.GetKeyDown("p"))
        {
            if (sc.isSinglePlayer)
            {
                mqtt.Publish("poseOK");
            }
            else
            {
                mqtt.Publish(mqtt.myNickname + "," + "poseOK");
            }
        }

        // Turn left
        if (Input.GetKeyDown("left"))
        {
            leftArrowDownTime = Time.time;
            Debug.Log("Left arrow key down");
        }
        if (Input.GetKeyUp("left"))
        {
            // time held down in ms
            float timeHeld = (int)(1000.0f * (Time.time - leftArrowDownTime));
            Debug.Log("Left arrow key up");
            Debug.Log(timeHeld);

            if (sc.isSinglePlayer)
            {
                mqtt.Publish("turn,left," + timeHeld);
            }
            else
            {
                mqtt.Publish(mqtt.myNickname + "," + "turn,left," + timeHeld);
            }
        }

        // Turn right
        if (Input.GetKeyDown("right"))
        {
            rightArrowDownTime = Time.time;
            Debug.Log("Right arrow key down");
        }
        if (Input.GetKeyUp("right"))
        {
            // time held down in ms
            float timeHeld = (int)(1000.0f * (Time.time - rightArrowDownTime));
            Debug.Log("Right arrow key up");
            Debug.Log(timeHeld);

            if (sc.isSinglePlayer)
            {
                mqtt.Publish("turn,right," + timeHeld);
            }
            else
            {
                mqtt.Publish(mqtt.myNickname + "," + "turn,right," + timeHeld);
            }
        }

        // EndButtons
        if (Input.GetKeyDown("e"))
        {
            if (sc.isSinglePlayer)
            {
                mqtt.Publish("endButtons");
            }
            else
            {
                mqtt.Publish(mqtt.myNickname + "," + "endButtons");
            }
        }

        // Swing
        if (Input.GetKeyDown("space"))
        {
            spaceDownTime = Time.time;
            Debug.Log("Space key down");
        }
        if (Input.GetKeyUp("space"))
        {
            // time held down in ms
            float timeHeld = (int)(1000.0f*(Time.time - spaceDownTime));
            Debug.Log("Space key up");
            Debug.Log(timeHeld);

            int msToSwingMag = (int)(7000 + (32 - 7) * Mathf.Min(timeHeld, (float)1000));
            Debug.Log("Swinging with magnitude:");
            Debug.Log(msToSwingMag);
            if (sc.isSinglePlayer)
            {
                mqtt.Publish("classifierData" + "," + msToSwingMag);
            }
            else
            {
                mqtt.Publish(mqtt.myNickname + "," + "classifierData" + "," + msToSwingMag);
            }
        }
    }
}
