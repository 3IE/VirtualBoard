public class Event
{
    public enum EventCode : byte
    {
        // Room 1 - 9
        CloseRoom = 1,
        RoomLaunch,

        // Player 1X
        SendNewPostIt = 10,
        SendNewPosition,
        SendNewPlayerIn,
        SendNewPing,

        // Tools 2X
        Marker = 20,
        Eraser,

        // Objects 3X

        // Chat 4X

        // Error 100 - 200 (temporaire)
    }
}