using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{

    // Enumerated type currently defined in StrokeManager.cs
    private GameState state;
    public bool isSinglePlayer;

    public void SetState(GameState gs)
    {
        state = gs;
    }

    public GameState GetState()
    {
        return state;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Preserve this object throughout every scene
        DontDestroyOnLoad(transform.gameObject);

        // Game is multiplayer by default
        isSinglePlayer = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
