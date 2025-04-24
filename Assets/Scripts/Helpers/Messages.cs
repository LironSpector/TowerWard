//------- Messages (different message types) used in the client side in some other files as a way to transfer some information -------
public class Message
{
    public string Type { get; set; }
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
