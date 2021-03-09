using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{

    // Enumerated type currently defined in StrokeManager.cs
    private GameState state;
    public bool isSinglePlayer;
    private int playerPuttingIndex;
    private int level;

    public void SetState(GameState gs)
    {
        state = gs;
    }

    public GameState GetState()
    {
        return state;
    }

    public void SetPlayerPuttingIndex(int index)
    {
        playerPuttingIndex = index;
    }

    public int GetPlayerPuttingIndex()
    {
        return playerPuttingIndex;
    }

    public void SetLevel(int newLevel)
    {
        level = newLevel;
    }

    public int GetLevel()
    {
        return level;
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
