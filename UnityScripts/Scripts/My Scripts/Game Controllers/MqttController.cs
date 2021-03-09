using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // multiplayer joining by default
        isHost = false;

        changeLevelOnNextUpdate = false;
        firstChangeLevelOnNextUpdate = true;

    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("StrokeManager"))
        {
            sm = GameObject.FindGameObjectWithTag("StrokeManager").GetComponent<StrokeManager>();
        }
        if (GameObject.FindGameObjectWithTag("ChatBoxManager"))
        {
            cbm = GameObject.FindGameObjectWithTag("ChatBoxManager").GetComponent<ChatBoxManager>();
        }
        if (changeLevelOnNextUpdate)
        {
            changeLevelOnNextUpdate = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

            if (firstChangeLevelOnNextUpdate)
            {
                firstChangeLevelOnNextUpdate = false;
                //mm.PlayMultiPlayer();
            }
            else
            {
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    private StateController sc;

    private StrokeManager sm;

    private ChatBoxManager cbm;

    public bool isHost;

    public string myNickname;

    private bool changeLevelOnNextUpdate;
    private bool firstChangeLevelOnNextUpdate;

    public List<string> playerNames = new List<string>();

    public string MqttTopic;
    private MqttClient client;

    // Main menu object -- used for initially starting the game
    public MainMenu mm;

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
            // lobby multiplayer
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
                    if (parsedMessage.Length == 2 && parsedMessage[0] == "startGame")
                    {
                        HandleStartGame(parsedMessage);
                    }
                }
            }

            // in-game multiplayer
            if (sc.GetState() == GameState.DETECTING_POSE)
            {
                // Host client behavior
                if (isHost)
                {
                    if (parsedMessage.Length == 2)
                    {
                        if (parsedMessage[1] == "poseOK")
                        {
                            HandleMultiPoseOkMsg(parsedMessage);
                        }
                    }
                }

                // Guest client behavior
                else
                {
                    if (parsedMessage.Length > 0)
                    {
                        if (parsedMessage[0] == "newState")
                        {
                            HandleNewStateMsg(parsedMessage);
                        }
                    }
                }
            }
            if (sc.GetState() == GameState.AIMING_BALL)
            {
                // Host client behavior
                if (isHost)
                {
                    if (parsedMessage.Length == 4)
                    {
                        if (parsedMessage[1] == "turn")
                        {
                            HandleMultiAimDataMsg(parsedMessage);
                        }
                    }
                    if (parsedMessage.Length == 2)
                    {
                        if (parsedMessage[1] == "endButtons")
                        {
                            HandleMultiDoneAimingMsg(parsedMessage);
                        }
                    }
                }

                // Guest client behavior
                else
                {
                    if (parsedMessage.Length > 0)
                    {
                        if (parsedMessage[0] == "newState")
                        {
                            HandleNewStateMsg(parsedMessage);
                        }
                    }
                }
            }
            if (sc.GetState() == GameState.AWAITING_SWING)
            {
                // Host client behavior
                if (isHost)
                {
                    if (parsedMessage.Length == 3)
                    {
                        if (parsedMessage[1] == "classifierData")
                        {
                            HandleMultiSwingDataMsg(parsedMessage);
                        }
                    }
                }

                // Guest client behavior
                else
                {
                    if (parsedMessage.Length > 0)
                    {
                        if (parsedMessage[0] == "newState")
                        {
                            HandleNewStateMsg(parsedMessage);
                        }
                    }
                }
            }
            if (!isHost && parsedMessage.Length == 3 && parsedMessage[0] == "newState" && parsedMessage[2] == "DETECTING_POSE")
            {
                HandleNewStateMsg(parsedMessage);
            }

            // The following conditions should apply to all states
            if (parsedMessage.Length == 2 && parsedMessage[0] == "newPlayerPutting")
            {
                HandleNewPlayerPuttingMsg(parsedMessage);
            }
            if (parsedMessage.Length == 5 && parsedMessage[0] == "newPlayerPosition")
            {
                HandleNewPlayerPositionMsg(parsedMessage);
            }
            if (parsedMessage.Length == 3 && parsedMessage[0] == "newMessage")
            {
                if (parsedMessage[1] == myNickname)
                {
                    cbm.SendMessageToChat("You: " + parsedMessage[2]);
                }
                else
                {
                    cbm.SendMessageToChat(parsedMessage[1] + ": " + parsedMessage[2]);
                }
            }

            // Moving on to the next hole
            if (parsedMessage.Length == 1 && parsedMessage[0] == "holeFinished")
            {
                sc.SetState(GameState.DETECTING_POSE);
                int current_level = sc.GetLevel();
                sc.SetLevel(current_level + 1);
                Debug.Log("Setting the level to " + (current_level + 1));
                changeLevelOnNextUpdate = true;
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    public void AddPlayer(string name)
    {
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

    public void ProcessStrokeFinished()
    {
        // Mark player as 'has putted'
        sm.SetPlayerHasPuttedYet(playerNames[sc.GetPlayerPuttingIndex()], true);

        // Go through roster and search for the next player who isn't done with the hole
        bool everyoneFinishedHole = true;
        for (int i = 1; i <= playerNames.Count; i++)
        {
            if (!sm.GetPlayerFinishedHole(playerNames[(sc.GetPlayerPuttingIndex() + i) % playerNames.Count]))
            {
                everyoneFinishedHole = false;
                sc.SetPlayerPuttingIndex((sc.GetPlayerPuttingIndex() + i) % playerNames.Count);
                Publish("newPlayerPutting," + playerNames[sc.GetPlayerPuttingIndex()]);
                break;
            }
        }

        // This will trigger if we have scanned the entire roster and everyone's done with the hole
        if (everyoneFinishedHole)
        {
            Debug.Log("Moving on to the next hole");
            Publish("holeFinished");

            sm.ResetAllBallPositions();
            sm.MoveBallTo(playerNames[0]);

            //changeLevelOnNextUpdate = true;
            return;
        }

        // Revert back to aiming ball, with an updated position
        Vector3 playerPosition = sm.GetPlayerPosition(playerNames[sc.GetPlayerPuttingIndex()]);
        if (playerPosition == null)
        {
            Debug.Log("MqttController.ProcessStrokeFinished: Bad playerPosition returned by StrokeManager");
        }

        // Check the player's pose if they have not putted yet
        if (!sm.GetPlayerHasPuttedYet(playerNames[sc.GetPlayerPuttingIndex()]))
        {
            Publish(playerNames[sc.GetPlayerPuttingIndex()] + ",startPose");
            Publish("newState," + playerNames[sc.GetPlayerPuttingIndex()] + ",DETECTING_POSE");
            sc.SetState(GameState.DETECTING_POSE);
        }
        else
        {
            Publish(playerNames[sc.GetPlayerPuttingIndex()] + ",startButtons");
            Publish("newState," + playerNames[sc.GetPlayerPuttingIndex()] + ",AIMING_BALL");
        }
    }

    /*
     * Singleplayer MQTT message handlers
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

    private void HandleStartGame(string[] parsedMessage)
    {
        Debug.Log("Starting the game");
        sc.SetLevel(0);
        changeLevelOnNextUpdate = true;
    }

    /*
     * Multiplayer MQTT message handlers
     */
    private void HandleMultiPoseOkMsg(string[] parsedMessage)
    {
        // When the host receives poseOK, it will broadcast the new state to everyone
        if (parsedMessage[0] == playerNames[sc.GetPlayerPuttingIndex()])
        {
            sc.SetState(GameState.AIMING_BALL);
            Publish(parsedMessage[0] + ",startButtons");
            Publish("newState," + parsedMessage[0] + ",AIMING_BALL");
        }
    }

    private void HandleMultiAimDataMsg(string[] parsedMessage)
    {
        // When the host receives new tilt data, it will broadcast the new state to everyone
        if (parsedMessage[0] == playerNames[sc.GetPlayerPuttingIndex()])
        {
            sm.TiltBall(parsedMessage[2], parsedMessage[3]);
            Publish("newState," + parsedMessage[0] + ",CHANGING_BALL_ANGLE," + parsedMessage[2] + "," + parsedMessage[3]);
        }
    }

    private void HandleMultiDoneAimingMsg(string[] parsedMessage)
    {
        // When the host receives endButtons, broadcast the new state to everyone
        if (parsedMessage[0] == playerNames[sc.GetPlayerPuttingIndex()])
        {
            sc.SetState(GameState.AWAITING_SWING);
            Publish(parsedMessage[0] + ",startClassifier");
            Publish("newState," + parsedMessage[0] + ",AWAITING_SWING");
        }
    }

    private void HandleMultiSwingDataMsg(string[] parsedMessage)
    {
        // When the host receives swing data, it will broadcast it to everyone
        if (parsedMessage[0] == playerNames[sc.GetPlayerPuttingIndex()])
        {
            sm.WhackBall(parsedMessage[2]);
            Publish("newState," + parsedMessage[0] + ",SWINGING," + parsedMessage[2]);
        }
    }

    private void HandleNewPlayerPuttingMsg(string[] parsedMessage)
    {
        sc.SetPlayerPuttingIndex(playerNames.IndexOf(parsedMessage[1]));
        sm.MoveBallTo(parsedMessage[1]);
    }

    private void HandleNewPlayerPositionMsg(string[] parsedMessage)
    {
        Vector3 newPosition = new Vector3((float)Convert.ToDouble(parsedMessage[2]), (float)Convert.ToDouble(parsedMessage[3]), (float)Convert.ToDouble(parsedMessage[4]));
        sm.UpdatePlayerPosition(parsedMessage[1], newPosition);
    }

    private void HandleNewStateMsg(string[] parsedMessage)
    {
        // Update the most recent putting player
        int newPuttingPlayerIndex = playerNames.IndexOf(parsedMessage[1]);
        if (newPuttingPlayerIndex == -1)
        {
            Debug.Log("MqttController: Player name " + parsedMessage[1] + " not found in local roster");
            return;
        }
        sc.SetPlayerPuttingIndex(newPuttingPlayerIndex);

        if (parsedMessage.Length == 3)
        {
            if (parsedMessage[2] == "AIMING_BALL")
            {
                sm.ResetBallRotation();
                sc.SetState(GameState.AIMING_BALL);
            }
            if (parsedMessage[2] == "AWAITING_SWING")
            {
                sc.SetState(GameState.AWAITING_SWING);
            }
            if (parsedMessage[2] == "DETECTING_POSE")
            {
                sc.SetState(GameState.DETECTING_POSE);
            }
        }
        if (parsedMessage.Length == 4)
        {
            if (parsedMessage[2] == "SWINGING")
            {
                sm.WhackBall(parsedMessage[3]);
            }
        }
        if (parsedMessage.Length == 5)
        {
            if (parsedMessage[2] == "CHANGING_BALL_ANGLE")
            {
                sm.TiltBall(parsedMessage[3], parsedMessage[4]);
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
