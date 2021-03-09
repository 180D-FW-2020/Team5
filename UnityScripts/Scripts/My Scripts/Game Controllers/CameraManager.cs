using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        playerBallRB = playerBall.GetComponent<Rigidbody>();
    }

    public GameObject cam;
    public GameObject playerBall;
    public Rigidbody playerBallRB;

    private Vector3 relativePosition = new Vector3(0.0f, 1.5f, 0.0f);
    private float relativePosScale = -2;
    private float cameraDownAngle = 40.0f;
    private Vector3 e2 = new Vector3(0.0f, 1.0f, 0.0f);

    // Update is called once per frame
    void Update()
    {

        cam.transform.rotation = Quaternion.Euler(cameraDownAngle, playerBall.transform.rotation.eulerAngles.y, 0);
        Vector3 normalizedForwardWithoutY = playerBall.transform.forward;

        // If the speed is high enough, velocity should dictate camera position
        // Otherwise, the direction of the ball should dictate camera position
        if (playerBallRB.velocity.magnitude > 0.01)
        {
            normalizedForwardWithoutY = Vector3.Normalize(playerBallRB.velocity);
        }

        normalizedForwardWithoutY.y = 0;
        normalizedForwardWithoutY = Vector3.Normalize(normalizedForwardWithoutY);
        cam.transform.position = playerBall.transform.position + relativePosScale * normalizedForwardWithoutY + relativePosition;
        cam.transform.LookAt(playerBall.transform.position);
    }
}
