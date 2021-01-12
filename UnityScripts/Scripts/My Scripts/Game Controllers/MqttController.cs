using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;

/* 
 * This class defines the MQTT subscriber/publisher instance for the Unity client
 * Methods in this class should be called by other objects which seek to use MQTT
 */
public class MqttController : MonoBehaviour
{

    void Start()
    {
        // Preserve this object throughout every scene
        DontDestroyOnLoad(transform.gameObject);

        // Find state controller object
        sc = GameObject.FindGameObjectWithTag("StateController").GetComponent<StateController>();

        // error checking
        if (!sc)
        {
            Debug.Log("MqttController: Could not find state controller object");
        }

        // initialize nickname
        myNickname = "";

        // initialize playerNames to avoid array out-of-bounds exceptions
        // TODO: Fix dirty initialization, probably a better way than this
        /*for (int i = 0; i < 4; i++)
        {
            playerNames.Add("");
        }*/

        // multiplayer joining by default
        isHost = false;

    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("StrokeManager"))
        {
            sm = GameObject.FindGameObjectWithTag("StrokeManager").GetComponent<StrokeManager>();
        }
    }

    private StateController sc;

    private StrokeManager sm;

    public bool isHost;

    public string myNickname;

    public List<string> playerNames = new List<string>();

    public string MqttTopic;
    private MqttClient client;

    // Connect to the argument topic
    public void Connect(string topic)
    {
        // keep local copy of the topic
        MqttTopic = topic;

        // create client instance 
        client = new MqttClient(GameConstants.MQTT_BROKER);

        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);

        // subscribe to the topic with QoS 2
        client.Subscribe(new string[] { MqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        Debug.Log("Subscribing to " + MqttTopic);

        // Change state to in-lobby
        sc.SetState(GameState.IN_WAITING_ROOM);
    }

    public void Publish(string message)
    {
        Debug.Log("publishing " + message);
        client.Publish(MqttTopic, System.Text.Encoding.UTF8.GetBytes(message));
    }

    public void Disconnect()
    {
        Debug.Log("Disconnecting from topic " + MqttTopic);
        client.Disconnect();
    }

    /* 
     * This method should only read from MQTT and write to the game state
     * Defines the part of the state machine influenced by MQTT
     */
    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string receivedMessage = System.Text.Encoding.UTF8.GetString(e.Message);
        string[] parsedMessage = receivedMessage.Split(',');
        Debug.Log((isHost ? "Host" : "Guest") + " client received: " + receivedMessage);

        // Single player behavior
        if (sc.isSinglePlayer)
        {
            if (parsedMessage.Length == 2 && sc.GetState() == GameState.AWAITING_SWING)
            {
                // Swing data received
                HandleSwingDataMsg(parsedMessage);
                return;
            }
            if (receivedMessage == "poseOK" && sc.GetState() == GameState.DETECTING_POSE)
            {
                // Pose status data received
                HandlePoseOkMsg(parsedMessage);
                return;
            }
            if (sc.GetState() == GameState.AIMING_BALL)
            {
                if (receivedMessage == "endButtons")
                {
                    // Done aiming
                    HandleDoneAimingMsg(parsedMessage);
                    return;
                }
                if (parsedMessage.Length < 3)
                {
                    // Error: bad button message
                    Debug.Log("Bad button message");
                    return;
                }
                HandleAimDataMsg(parsedMessage);
                return;
            }
        }

        // Multiplayer behavior
        else
        {
            if (sc.GetState() == GameState.IN_WAITING_ROOM)
            {
                // Host client behavior
                if (isHost)
                {
                    if (parsedMessage.Length == 2 && parsedMessage[0] == "joining")
                    {
                        HandlePlayerJoin(parsedMessage);
                    }
                    if (parsedMessage.Length == 2 && parsedMessage[0] == "leaving")
                    {
                        HandlePlayerLeave(parsedMessage);
                    }
                }

                // Guest client behavior
                else
                {
                    if (parsedMessage.Length > 0 && parsedMessage[0] == "roster")
                    {
                        HandleRosterUpdate(parsedMessage);
                    }
                }
            }
        }
    }

    public void AddPlayer(string name)
    {
        /*if (playerNames.Count <= index)
        {
            Debug.Log("SetPlayerName: Player index out of bounds");
            return;
        }
        playerNames[index] = name;*/
        playerNames.Add(name);
    }

    // called when exiting lobby
    public void ClearMqttData()
    {
        // clear lobby participants
        playerNames.Clear();

        // Change state to main page
        sc.SetState(GameState.ON_MAIN_PAGE);

        // disconnect from mqtt topic
        Disconnect();
    }

    /*
     * MQTT message handlers
     */
    private void HandleSwingDataMsg(string[] parsedMessage)
    {
        sm.WhackBall(parsedMessage[1]);
    }

    private void HandlePoseOkMsg(string[] parsedMessage)
    {
        sc.SetState(GameState.AIMING_BALL);
        Publish("startButtons");
    }

    private void HandleDoneAimingMsg(string[] parsedMessage)
    {
        sc.SetState(GameState.AWAITING_SWING);
        Publish("startClassifier");
    }

    private void HandleAimDataMsg(string[] parsedMessage)
    {
        sm.TiltBall(parsedMessage[1], parsedMessage[2]);
    }

    private void HandlePlayerJoin(string[] parsedMessage)
    {
        if (playerNames.Count >= GameConstants.MAX_PLAYERS)
        {
            Debug.Log("Player " + parsedMessage[1] + " cannot join: maximum players reached (" + GameConstants.MAX_PLAYERS + ")");
            return;
        }

        // add player name to roster list
        AddPlayer(parsedMessage[1]);

        PublishRoster();

        /*Debug.Log("Sending updated roster");
        string rosterMessage = "roster";
        for (int i = 0; i < playerNames.Count; i++)
        {
            rosterMessage += "," + playerNames[i];
        }
        Publish(rosterMessage);*/
        /*for (int i = 0; i < playerNames.Count; i++)
        {
            if (playerNames[i] == "" && parsedMessage[1] != "")
            {
                playerNames[i] = parsedMessage[1];
                Publish("roster," + playerNames[0] + "," + playerNames[1] + "," + playerNames[2] + "," + playerNames[3]);
                break;
            }
        }*/
    }

    private void HandlePlayerLeave(string[] parsedMessage)
    {
        if (!playerNames.Contains(parsedMessage[1]))
        {
            Debug.Log("Leaving member " + parsedMessage[1] + " not found in roster");
            return;
        }
        Debug.Log("Removing member " + parsedMessage[1] + " from the roster");
        playerNames.Remove(parsedMessage[1]);
        PublishRoster();
    }

    private void HandleRosterUpdate(string[] parsedMessage)
    {
        // strictly obey whatever the host says the roster is
        Debug.Log("Guest roster updating");
        playerNames.Clear();
        for (int i = 1; i < parsedMessage.Length; i++)
        {
            if (parsedMessage[i] != "")
            {
                playerNames.Add(parsedMessage[i]);
            }
        }
    }

    /*
     * Helper functions
     */
    private void PublishRoster()
    {
        Debug.Log("Sending updated roster");
        string rosterMessage = "roster";
        for (int i = 0; i < playerNames.Count; i++)
        {
            rosterMessage += "," + playerNames[i];
        }
        Publish(rosterMessage);
    }
}
