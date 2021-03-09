using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

// TODO: Check if this import is necessary
using System.Text;

public class MultiPlayerLobby : MonoBehaviour
{
    public MqttController mqtt;

    // nicknames -- used to identify people over mqtt
    public TMP_InputField myNicknameHost;
    public TMP_InputField myNicknameJoin;

    // roomCode is the read-only display on the lobby screen
    // roomCodeInput is the join code typed in by the user
    public TextMeshProUGUI roomCode;
    public TMP_InputField roomCodeInput;

    // TODO: Figure out how to make this more compact (array)
    public TextMeshProUGUI playerNameSlot1;
    public TextMeshProUGUI playerNameSlot2;
    public TextMeshProUGUI playerNameSlot3;
    public TextMeshProUGUI playerNameSlot4;

    private static int roomCodeStringLength = 5;

    private static System.Random random = new System.Random();

    // Create a random capital alphanumeric string (exclude 0, O, and I for clarity)
    public static string RandomString(int length) 
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public void HostLobby()
    {
        // set this client as host
        mqtt.isHost = true;

        // set nickname
        mqtt.myNickname = myNicknameHost.text;

        // set player 1 nickname to my nickname
        if (myNicknameHost == null || myNicknameHost.text == "")
        {
            Debug.Log("Host Nickname error");
        }
        mqtt.Connect(RandomString(roomCodeStringLength));
        mqtt.AddPlayer(mqtt.myNickname);

        Debug.Log("The room code is " + mqtt.MqttTopic);

        // write the room code
        roomCode.text = mqtt.MqttTopic;
    }

    public void JoinLobby()
    {
        // set this client as guest
        mqtt.isHost = false;

        // set nickname
        mqtt.myNickname = myNicknameJoin.text;

        // attempt mqtt connection to string in text box
        if (roomCodeInput.text.Length == 0)
        {
            Debug.Log("Join Lobby Error: Null room code");
            return;
        }
        mqtt.Connect(roomCodeInput.text.ToUpper());
        mqtt.Publish("joining," + mqtt.myNickname);

        Debug.Log("The room code is " + mqtt.MqttTopic);
    }

    public void LeaveLobby()
    {
        mqtt.Publish("leaving," + mqtt.myNickname);
        mqtt.ClearMqttData();
    }

    // Start is called before the first render
    void Start()
    {
        // pass
    }

    // Update is called once per frame
    void Update()
    {
        if (mqtt.playerNames == null)
        {
            return;
        }

        // write the room code
        roomCode.text = mqtt.MqttTopic;

        // write the player names
        // TODO: fix this too, dirty :((
        for (int i = 0; i < GameConstants.MAX_PLAYERS; i++)
        {
            switch (i)
            {
                case 0:
                    playerNameSlot1.text = (i >= mqtt.playerNames.Count) ? "Slot 1 empty..." : mqtt.playerNames[0];
                    break;
                case 1:
                    playerNameSlot2.text = (i >= mqtt.playerNames.Count) ? "Slot 2 empty..." : mqtt.playerNames[1];
                    break;
                case 2:
                    playerNameSlot3.text = (i >= mqtt.playerNames.Count) ? "Slot 3 empty..." : mqtt.playerNames[2];
                    break;
                case 3:
                    playerNameSlot4.text = (i >= mqtt.playerNames.Count) ? "Slot 4 empty..." : mqtt.playerNames[3];
                    break;
            }
        }


        /*playerNameSlot1.text = mqtt.playerNames[0] == "" ? "Slot 1 empty..." : mqtt.playerNames[0];
        playerNameSlot2.text = mqtt.playerNames[1] == "" ? "Slot 2 empty..." : mqtt.playerNames[1];
        playerNameSlot3.text = mqtt.playerNames[2] == "" ? "Slot 3 empty..." : mqtt.playerNames[2];
        playerNameSlot4.text = mqtt.playerNames[3] == "" ? "Slot 4 empty..." : mqtt.playerNames[3];*/
    }
}
