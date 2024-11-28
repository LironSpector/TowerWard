public class Message
{
    public string Type { get; set; }
}

// MatchFoundMessage
public class MatchFoundMessage : Message
{
    // No additional data needed
}

// SendBalloonMessage
public class SendBalloonMessage : Message
{
    public SendBalloonData Data { get; set; }
}

public class SendBalloonData
{
    public string BalloonType { get; set; }
    public int Quantity { get; set; }
}

// GameSnapshotMessage
public class GameSnapshotMessage : Message
{
    public GameSnapshotData Data { get; set; }
}

public class GameSnapshotData
{
    public string ImageData { get; set; }
}

// GameOverMessage
public class GameOverMessage : Message
{
    public GameOverData Data { get; set; }
}

public class GameOverData
{
    public bool Won { get; set; }
}

// MatchmakingRequestMessage
public class MatchmakingRequestMessage : Message
{
    // No additional data needed
}
