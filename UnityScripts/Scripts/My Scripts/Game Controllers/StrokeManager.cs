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
    }

    private Quaternion INERTIA_QUATERNION = new Quaternion(0.01f, 0.01f, 0.01f, 1.0f);

    private StateController sc;

    private MqttController mqtt;

    GameObject playerBallGO;
    Rigidbody playerBallRB;
    private double whackVelocity = 0.0;

    private int buttonPressMs = 0;

    public Text strokeReadyStatus;
     
   

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
        return ((foo - 7000.0f) * 8.0f / (30000.0f - 7000.0f));
    }

    // Update is called once per frame -- use this for inputs
    private void Update()
    {
        // Display current game state
        strokeReadyStatus.text = sc.GetState().ToString();
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
                sc.SetState(GameState.AIMING_BALL);
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