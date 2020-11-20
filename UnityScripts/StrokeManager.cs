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

public class StrokeManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindPlayerBall();
        MqttSubscriberInit();
    }
    
    Rigidbody playerBallRB;
    private bool doWhack = false;
    private double whackVelocity = 0.0;
     
    private MqttClient client;

    private void MqttSubscriberInit()
    {
        // create client instance 
        client = new MqttClient("mqtt.eclipse.org");

        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);

        // subscribe to the topic "ece180da_team5" with QoS 2
        client.Subscribe(new string[] { "ece180da_team5" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
        whackVelocity = Convert.ToDouble(System.Text.Encoding.UTF8.GetString(e.Message));
        doWhack = true;
    }

    private void FindPlayerBall()
    {
        GameObject go = GameObject.FindGameObjectWithTag("PlayerBall");

        if (go == null)
        {
            Debug.LogError("Couldn't find the ball.");
        }

        playerBallRB = go.GetComponent<Rigidbody>();
        if (playerBallRB == null)
        {
            Debug.LogError("Player ball has no rigid body.");
        }
    }

    // Update is called once per frame -- use this for inputs
    private void Update()
    {

    }

    // FixedUpdate runs on every tick of the physics engine -- use this for manipulation
    void FixedUpdate()
    {
        if (playerBallRB == null)
        {
            return;
        }
        if (doWhack)
        {
            doWhack = false;

            Debug.Log("Whacking the ball");

            Vector3 forceVec = new Vector3(0, 0, (float)whackVelocity);

            playerBallRB.AddForce(forceVec, ForceMode.Impulse);
            //whackVelocity = 0.0;
        }
    }
}