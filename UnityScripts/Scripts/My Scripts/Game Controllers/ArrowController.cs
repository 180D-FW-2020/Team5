using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        playerBallRB = playerBall.GetComponent<Rigidbody>();
        arrowMeshRenderer = this.GetComponent<MeshRenderer>();
    }

    private MeshRenderer arrowMeshRenderer;

    private float OSC_PER_SEC = 1.0f;
    private float OSC_MAG = 5.0f;

    public GameObject playerBall;
    private Rigidbody playerBallRB;
    private Vector3 averageRelativeArrowPosition = new Vector3(0.0f, 0.0f, 0.125f);
    private float averageRelativePosScale = 0.5f;

    // Update is called once per frame
    void Update()
    {
        if (playerBallRB.velocity.magnitude > 0.01)
        {
            arrowMeshRenderer.enabled = false;
            return;
        }

        // calculate oscillation position
        float scaleModifier = Mathf.Sin(Time.time * OSC_PER_SEC * Mathf.PI * 2.0f)/OSC_MAG;

        arrowMeshRenderer.enabled = true;

        this.transform.rotation = Quaternion.Euler(180, -90+playerBall.transform.rotation.eulerAngles.y, -90);

        Vector3 normalizedForwardWithoutY = playerBall.transform.forward;
        normalizedForwardWithoutY.y = 0;
        normalizedForwardWithoutY = Vector3.Normalize(normalizedForwardWithoutY);

        this.transform.position = playerBall.transform.position + (averageRelativePosScale + scaleModifier) * normalizedForwardWithoutY;
    }
}
