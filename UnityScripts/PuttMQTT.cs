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

public class PuttMQTT : MonoBehaviour
{
	private MqttClient client;
	private String mqttString = "Waiting for an MQTT string...";
	public Text mqttText = null;

	// Use this for initialization
	void Start()
	{
		// create client instance 
		client = new MqttClient("mqtt.eclipse.org");

		// register to message received 
		client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

		string clientId = Guid.NewGuid().ToString();
		client.Connect(clientId);

		// subscribe to the topic "/home/temperature" with QoS 2 
		client.Subscribe(new string[] { "wordoftheday" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
	}

	void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
	{
		Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
		mqttString = "Latest MQTT string: " + System.Text.Encoding.UTF8.GetString(e.Message);
	}

	// Update is called once per frame
	void Update()
	{

		mqttText.text = mqttString;

	}
}