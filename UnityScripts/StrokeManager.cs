using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;

public enum GameState
{
    DETECTING_POSE,
    AIMING_BALL,
    AWAITING_SWING,
    BALL_IN_MOTION
}

public class StrokeManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindPlayerBall();
        MqttSubscriberInit();
        gs = GameState.DETECTING_POSE;
    }

    private Quaternion INERTIA_QUATERNION = new Quaternion(0.01f, 0.01f, 0.01f, 1.0f);

    private GameState gs;
    private string MQTT_topic = "ece180da_team5";
    private string MQTT_broker = "broker.hivemq.com";

    // 1000 ms button push = 45 degree tilt
    private double MAX_ANGLE = 45;
    private double MAX_BUTTON_MS = 1000;

    GameObject playerBallGO;
    Rigidbody playerBallRB;
    private bool doWhack = false;
    private double whackVelocity = 0.0;
    private double MAX_WHACK_VELOCITY_SCALAR = 1700;

    private bool doTilt = false;
    private int buttonPressMs = 0;

    public Text strokeReadyStatus;
     
    private MqttClient client;

    private void MqttSubscriberInit()
    {
        // create client instance 
        client = new MqttClient(MQTT_broker);

        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);

        // subscribe to the topic "ece180da_team5" with QoS 2
        client.Subscribe(new string[] { MQTT_topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string receivedMessage = System.Text.Encoding.UTF8.GetString(e.Message);
        string[] parsedMessage = receivedMessage.Split(',');
        Debug.Log("Received: " + receivedMessage);
        Debug.Log(parsedMessage);
        if (gs == GameState.AWAITING_SWING)
        {
            whackVelocity = Convert.ToDouble(receivedMessage);
            doWhack = true;
            gs = GameState.BALL_IN_MOTION;
            return;
        }
        if (receivedMessage == "poseOK" && gs == GameState.DETECTING_POSE)
        {
            gs = GameState.AIMING_BALL;
            return;
        }
        if (gs == GameState.AIMING_BALL)
        {
            if (receivedMessage == "endButtons")
            {
                gs = GameState.AWAITING_SWING;
                return;
            }
            int ms_held = -1;
            if (parsedMessage.Length < 3)
            {
                Debug.Log("bad button message");
                return;
            }
            bool result = int.TryParse(parsedMessage[2], out ms_held);
            if (result)
            {
                Debug.Log("tilt received: " + receivedMessage);
                buttonPressMs = ms_held;
                if (parsedMessage[1] == "left")
                {
                    buttonPressMs = (-1) * buttonPressMs;
                }
                doTilt = true;

            }
            else Debug.Log("bad tilt number received");

            return;
        }
    }

    private void FindPlayerBall()
    {
        playerBallGO = GameObject.FindGameObjectWithTag("PlayerBall");

        if (playerBallGO == null)
        {
            Debug.LogError("Couldn't find the ball.");
        }

        playerBallRB = playerBallGO.GetComponent<Rigidbody>();
        if (playerBallRB == null)
        {
            Debug.LogError("Player ball has no rigid body.");
        }
    }

    private void setDisplayString(GameState currentGameState)
    {
        switch(currentGameState)
        {
            case GameState.AIMING_BALL:
                strokeReadyStatus.text = "Aim with buttons";
                break;
            case GameState.AWAITING_SWING:
                strokeReadyStatus.text = "Ready for swing";
                break;
            case GameState.BALL_IN_MOTION:
                strokeReadyStatus.text = "Wait for ball to stop";
                break;
            case GameState.DETECTING_POSE:
                strokeReadyStatus.text = "Awaiting pose detection";
                break;
            default:
                strokeReadyStatus.text = "Error: unknown game state";
                break;
        }

    }

    // Update is called once per frame -- use this for inputs
    private void Update()
    {
        setDisplayString(gs);
    }

    // FixedUpdate runs on every tick of the physics engine -- use this for manipulation
    void FixedUpdate()
    {
        if (playerBallRB == null)
        {
            return;
        }

        // we must do this, otherwise the ball will not stop very fast
        playerBallRB.inertiaTensorRotation = INERTIA_QUATERNION;
        playerBallRB.AddTorque(-playerBallRB.angularVelocity * 0.01f);

        if (doWhack)
        {
            doWhack = false;

            Debug.Log("Whacking the ball");

            Vector3 forwardWithoutY = playerBallGO.transform.forward;
            forwardWithoutY[1] = 0.0f;
            forwardWithoutY = Vector3.Normalize(forwardWithoutY);

            playerBallRB.AddForce(forwardWithoutY * (float)whackVelocity, ForceMode.Impulse);
        }
        else if (gs == GameState.BALL_IN_MOTION)
        {
            if (playerBallRB.velocity.magnitude < 0.01)
            {
                gs = GameState.AIMING_BALL;
            }
        }
        if (doTilt)
        {
            doTilt = false;

            double tiltAmount =(int)((double)MAX_ANGLE * buttonPressMs / MAX_BUTTON_MS);

            if (playerBallGO.transform.up.y < 0)
            {
                tiltAmount = (-1) * tiltAmount;
            }

            Debug.Log("tilting the ball by " + tiltAmount);

            playerBallRB.rotation = playerBallRB.rotation * Quaternion.AngleAxis((float)tiltAmount, Vector3.up);
        }
    }
}