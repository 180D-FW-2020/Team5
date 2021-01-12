using UnityEngine;
using System.Collections;

public class ChangeBallLayer : MonoBehaviour {

    public int LayerOnEnter; // BallInHole
    public int LayerOnExit;  // BallOnTable
	
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PlayerBall")
        {
            Debug.Log("Ball in hole!");
            other.gameObject.layer = LayerOnEnter;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PlayerBall")
        {
            other.gameObject.layer = LayerOnExit;
        }
    }
}