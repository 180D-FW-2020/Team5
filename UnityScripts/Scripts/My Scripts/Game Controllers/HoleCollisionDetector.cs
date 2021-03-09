using UnityEngine;
using System.Collections;

public class HoleCollisionDetector : MonoBehaviour
{

    public StrokeManager sm;


    void Update()
    {
        if (GameObject.FindGameObjectWithTag("StrokeManager"))
        {
            sm = GameObject.FindGameObjectWithTag("StrokeManager").GetComponent<StrokeManager>();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "PlayerBall")
        {
            Debug.Log("Ball in hole!");
            sm.ProcessBallInHole();
        }
    }
}