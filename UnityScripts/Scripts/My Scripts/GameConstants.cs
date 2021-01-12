/*
 * GameState enum
 */
public enum GameState
{
    ON_MAIN_PAGE,
    IN_WAITING_ROOM,
    DETECTING_POSE,
    AIMING_BALL,
    CHANGING_BALL_ANGLE,
    AWAITING_SWING,
    SWINGING,
    BALL_IN_MOTION
}

public static class GameConstants
{
    /*
     * MQTT
     */
    public const string MQTT_BROKER = "broker.hivemq.com";
    public const string DEFAULT_MQTT_TOPIC = "ece180da_team5";
    public const int MAX_PLAYERS = 4;

    /*
     * Game Physics
     */
    // 1000 ms button push = 45 degree tilt
    public const double MAX_ANGLE = 30;
    public const double MAX_BUTTON_MS = 1000;
}