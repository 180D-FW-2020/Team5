using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardManager : MonoBehaviour
{
    public TextMeshProUGUI name1;
    public TextMeshProUGUI name2;
    public TextMeshProUGUI name3;
    public TextMeshProUGUI name4;

    public TextMeshProUGUI s00;
    public TextMeshProUGUI s10;
    public TextMeshProUGUI s20;
    public TextMeshProUGUI s30;

    public TextMeshProUGUI s01;
    public TextMeshProUGUI s11;
    public TextMeshProUGUI s21;
    public TextMeshProUGUI s31;

    public TextMeshProUGUI s0T;
    public TextMeshProUGUI s1T;
    public TextMeshProUGUI s2T;
    public TextMeshProUGUI s3T;

    public MqttController mqtt;

    public List<List<int>> scores = new List<List<int>>();

    // Start is called before the first frame update
    void Start()
    {
        // Find mqtt controller object
        mqtt = GameObject.FindGameObjectWithTag("MqttController").GetComponent<MqttController>();

        if (!mqtt)
        {
            Debug.Log("ScoreboardManager: Mqtt controller not found");
        }
        else
        {
            // write names into scoreboard
            if (mqtt.playerNames.Count > 0)
            {
                name1.text = mqtt.playerNames[0];
            }
            if (mqtt.playerNames.Count > 1)
            {
                name2.text = mqtt.playerNames[1];
            }
            if (mqtt.playerNames.Count > 2)
            {
                name3.text = mqtt.playerNames[2];
            }
            if (mqtt.playerNames.Count > 3)
            {
                name4.text = mqtt.playerNames[3];
            }
        }

        // instantiate score list
        for (int i = 0; i < GameConstants.MAX_PLAYERS; i++)
        {
            List<int> sublist = new List<int>();
            for (int j = 0; j < 9; j++)
            {
                sublist.Add(0);
            }
            scores.Add(sublist);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // write values into the scoreboard

        // column 1
        if (scores[0][0] > 0)
        {
            s00.text = scores[0][0].ToString();
        }
        if (scores[1][0] > 0)
        {
            s10.text = scores[1][0].ToString();
        }
        if (scores[2][0] > 0)
        {
            s20.text = scores[2][0].ToString();
        }
        if (scores[3][0] > 0)
        {
            s30.text = scores[3][0].ToString();
        }

        // column 2
        if (scores[0][1] > 0)
        {
            s01.text = scores[0][1].ToString();
        }
        if (scores[1][1] > 0)
        {
            s11.text = scores[1][1].ToString();
        }
        if (scores[2][1] > 0)
        {
            s21.text = scores[2][1].ToString();
        }
        if (scores[3][1] > 0)
        {
            s31.text = scores[3][1].ToString();
        }

        // Total column
        int rowSum = 0;
        for (int i = 0; i < 9; i++)
        {
            rowSum += scores[0][i];
        }
        if (rowSum > 0)
        {
            s0T.text = rowSum.ToString();
        }
        rowSum = 0;
        for (int i = 0; i < 9; i++)
        {
            rowSum += scores[1][i];
        }
        if (rowSum > 0)
        {
            s1T.text = rowSum.ToString();
        }
        rowSum = 0;
        for (int i = 0; i < 9; i++)
        {
            rowSum += scores[2][i];
        }
        if (rowSum > 0)
        {
            s2T.text = rowSum.ToString();
        }
        rowSum = 0;
        for (int i = 0; i < 9; i++)
        {
            rowSum += scores[3][i];
        }
        if (rowSum > 0)
        {
            s3T.text = rowSum.ToString();
        }
    }
}
