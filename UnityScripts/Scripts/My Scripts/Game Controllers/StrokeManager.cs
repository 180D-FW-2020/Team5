using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
//using uPLibrary.Networking.M2Mqtt;
//using uPLibrary.Networking.M2Mqtt.Messages;
//using uPLibrary.Networking.M2Mqtt.Utility;
//using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;

using TMPro;

public class StrokeManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        // Get player ball Rigidbody object
        playerBallGO = GameObject.FindGameObjectWithTag("PlayerBall");
        playerBallRB = playerBallGO.GetComponent<Rigidbody>();

        // Get state controller object
        sc = GameObject.FindGameObjectWithTag("StateController").GetComponent<StateController>();

        // Find mqtt controller object
        mqtt = GameObject.FindGameObjectWithTag("MqttController").GetComponent<MqttController>();

        // Initialize name buffer for ball movement
        moveBallToNameBuffer = "";

        // Error checking
        if (!playerBallGO)
        {
            Debug.Log("StrokeManager: Player ball game object not found");
        }
        if (!playerBallRB)
        {
            Debug.Log("StrokeManager: Player ball rigid body not found");
        }
        if (!sc)
        {
            Debug.Log("StrokeManager: State controller not found");
        }
        if (!mqtt)
        {
            Debug.Log("StrokeManager: Mqtt controller not found");
        }    

        // Set initial game state
        sc.SetState(GameState.DETECTING_POSE);

        resetBallRotationOnNextTick = false;

        // Initialize the ball positions for each player
        ResetAllBallPositions();
    }

    private Quaternion INERTIA_QUATERNION = new Quaternion(0.01f, 0.01f, 0.01f, 1.0f);

    private StateController sc;

    private MqttController mqtt;

    GameObject playerBallGO;
    Rigidbody playerBallRB;
    private double whackVelocity = 0.0;

    private int buttonPressMs = 0;

    public Text strokeReadyStatus;
    public TextMeshProUGUI puttingPlayer;

    // Can only move ball during FixedUpdate
    private string moveBallToNameBuffer;

    private bool resetBallRotationOnNextTick;

    private Dictionary<string, Vector3> ballPositions = new Dictionary<string, Vector3>();
    private Dictionary<string, bool> playerFinishedHole = new Dictionary<string, bool>();
    private Dictionary<string, bool> playerHasPuttedYet = new Dictionary<string, bool>();

    public ScoreboardManager sbm;

    public void WhackBall(string whackVelocityString)
    {
        whackVelocity = Convert.ToDouble(whackVelocityString);
        sc.SetState(GameState.SWINGING);
        mqtt.Publish("startButtons");
    }

    public void TiltBall(string direction, string buttonPressMsString)
    {
        int ms_held = -1;
        bool result = int.TryParse(buttonPressMsString, out ms_held);
        if (result)
        {
            buttonPressMs = ms_held;
            if (direction == "left")
            {
                buttonPressMs = (-1) * buttonPressMs;
            }
            sc.SetState(GameState.CHANGING_BALL_ANGLE);

        }
        else Debug.Log("bad tilt number received");
    }

    private float convertVelocity(float velocity)
    {
        Debug.Log("velocity:");
        Debug.Log(velocity);
        float foo = velocity;
        foo = Mathf.Max(foo, 7000.0f);
        foo = Mathf.Min(foo, 32000.0f);
        return ((foo - 7000.0f) * 15.0f / (30000.0f - 7000.0f));
    }

    public void UpdatePlayerPosition(string name, Vector3 position)
    {
        Debug.Log("Recording new position of player " + name + " as " + position.ToString());

        ballPositions[name] = position;
    }

    public Vector3 GetPlayerPosition(string name)
    {
        if (name == "")
        {
            Debug.Log("StrokeManager.GetPlayerPosition: Empty name parameter");
        }
        if (ballPositions[name] == null)
        {
            Debug.Log("StrokeManager.GetPlayerPosition: Ball position does not exist for name " + name);
        }
        return ballPositions[name]; 
    }

    public void MoveBallTo(string name)
    {
        if (name == "")
        {
            Debug.Log("StrokeManager.MoveBallTo: Empty name parameter");
        }
        if (ballPositions[name] == null)
        {
            Debug.Log("StrokeManager.MoveBallTo: Ball position does not exist for name " + name);
        }
        moveBallToNameBuffer = name;

        //Debug.Log("Moving ball to " + name + " at position " + ballPositions[name].ToString());
        //playerBallRB.MovePosition(ballPositions[name]);
    }
    
    public void ResetBallRotation()
    {
        playerBallRB.MoveRotation(Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f)));
    }

    public void SetPlayerFinishedHole(string name, bool hasFinished)
    {
        if (name == "")
        {
            Debug.Log("StrokeManager.SetPlayerFinishedHole: Empty name parameter");
        }
        if (ballPositions[name] == null)
        {
            Debug.Log("StrokeManager.SetPlayerFinishedHole: HasFinishedHole value does not exist for name " + name);
        }
        playerFinishedHole[name] = hasFinished;
    }

    public bool GetPlayerFinishedHole(string name)
    {
        if (name == "")
        {
            Debug.Log("StrokeManager.GetPlayerFinishedHole: Empty name parameter");
        }
        return playerFinishedHole[name];
    }

    public void SetPlayerHasPuttedYet(string name, bool hasPutted)
    {
        if (name == "")
        {
            Debug.Log("StrokeManager.SetPlayerHasPuttedYet: Empty name parameter");
        }
        playerHasPuttedYet[name] = hasPutted;
    }

    public bool GetPlayerHasPuttedYet(string name)
    {
        if (name == "")
        {
            Debug.Log("StrokeManager.GetPlayerHasPuttedYet: Empty name parameter");
        }
        return playerHasPuttedYet[name];
    }

    public void ProcessBallInHole()
    {
        // If the ball stops, reset its rotation angle
        ResetBallRotation();

        sc.SetState(GameState.AIMING_BALL);

        // If you are hosting a multiplayer game, 
        // record the ball position and switch to the next player
        if (mqtt.isHost && !sc.isSinglePlayer)
        {

            SetPlayerFinishedHole(mqtt.playerNames[sc.GetPlayerPuttingIndex()], true);


            string positionString = playerBallGO.transform.position.x.ToString() + ","
                                  + playerBallGO.transform.position.y.ToString() + ","
                                  + playerBallGO.transform.position.z.ToString();

            // Update scoreboard
            sbm.scores[sc.GetPlayerPuttingIndex()][sc.GetLevel()]++;

            // Broadcast player position to lobby
            mqtt.Publish("newPlayerPosition," + mqtt.playerNames[sc.GetPlayerPuttingIndex()] + "," + positionString);
            mqtt.ProcessStrokeFinished();
        }
    }

    public void ResetAllBallPositions()
    {
        // Initialize the ball positions for each player
        if (!sc.isSinglePlayer)
        {
            if (mqtt.playerNames.Count > 0)
            {
                for (int i = 0; i < mqtt.playerNames.Count; i++)
                {
                    ballPositions[mqtt.playerNames[i]] = new Vector3(0.0f, 0.0f, 0.0f);
                    playerFinishedHole[mqtt.playerNames[i]] = false;
                    playerHasPuttedYet[mqtt.playerNames[i]] = false;
                }
            }
            else
            {
                Debug.Log("StrokeManager: Cannot initialize multiplayer ball positions. Bad playerNames");
            }
        }
    }

    // Update is called once per frame -- use this for inputs
    private void Update()
    {
        // Display current game state
        strokeReadyStatus.text = sc.GetState().ToString();

        // Display current putter
        puttingPlayer.text = (sc.isSinglePlayer) ? "single player" : mqtt.playerNames[sc.GetPlayerPuttingIndex()];
    }

    // FixedUpdate runs on every tick of the physics engine -- use this for manipulation
    void FixedUpdate()
    {
        if (playerBallRB == null)
        {
            return;
        }

        if (moveBallToNameBuffer != "")
        {
            Debug.Log("Moving ball to " + moveBallToNameBuffer + " at position " + ballPositions[moveBallToNameBuffer].ToString());
            playerBallRB.MovePosition(ballPositions[moveBallToNameBuffer]);
            moveBallToNameBuffer = "";
        }

        if (resetBallRotationOnNextTick)
        {
            playerBallRB.MoveRotation(Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f)));
            resetBallRotationOnNextTick = false;
        }

        // we must do this, otherwise the ball will not stop very fast
        playerBallRB.inertiaTensorRotation = INERTIA_QUATERNION;
        playerBallRB.AddTorque(-playerBallRB.angularVelocity * 0.01f);

        if (sc.GetState() == GameState.SWINGING)
        {
            sc.SetState(GameState.BALL_IN_MOTION);

            Debug.Log("Whacking the ball");

            Vector3 forwardWithoutY = playerBallGO.transform.forward;
            forwardWithoutY[1] = 0.0f;
            forwardWithoutY = Vector3.Normalize(forwardWithoutY);

            Debug.Log(convertVelocity((float)whackVelocity));

            playerBallRB.AddForce(forwardWithoutY * convertVelocity((float)whackVelocity), ForceMode.Impulse);
        }
        else if (sc.GetState() == GameState.BALL_IN_MOTION)
        {
            if (playerBallRB.velocity.magnitude < 0.01)
            {
                // If the ball stops, reset its rotation angle
                ResetBallRotation();
                
                sc.SetState(GameState.AIMING_BALL);

                // If you are hosting a multiplayer game, 
                // record the ball position and switch to the next player
                if (mqtt.isHost && !sc.isSinglePlayer)
                {
                    // Determine whether the player is finished with the hole
                    if (playerBallGO.transform.position.y < GameConstants.BALL_IN_HOLE_Y_THRESH)
                    {
                        //SetPlayerFinishedHole(mqtt.playerNames[sc.GetPlayerPuttingIndex()], true);
                    }
                         

                    string positionString = playerBallGO.transform.position.x.ToString() + ","
                                          + playerBallGO.transform.position.y.ToString() + ","
                                          + playerBallGO.transform.position.z.ToString();

                    // Update scoreboard
                    sbm.scores[sc.GetPlayerPuttingIndex()][sc.GetLevel()]++;

                    // Broadcast player position to lobby
                    mqtt.Publish("newPlayerPosition," + mqtt.playerNames[sc.GetPlayerPuttingIndex()] + "," + positionString);
                    mqtt.ProcessStrokeFinished();
                }
            }
        }
        if (sc.GetState() == GameState.CHANGING_BALL_ANGLE)
        {
            sc.SetState(GameState.AIMING_BALL);

            double tiltAmount =(int)((double)GameConstants.MAX_ANGLE * buttonPressMs / GameConstants.MAX_BUTTON_MS);

            if (playerBallGO.transform.up.y < 0)
            {
                tiltAmount = (-1) * tiltAmount;
            }

            Debug.Log("tilting the ball by " + tiltAmount);

            playerBallRB.rotation = playerBallRB.rotation * Quaternion.AngleAxis((float)tiltAmount, Vector3.up);
        }
    }
}