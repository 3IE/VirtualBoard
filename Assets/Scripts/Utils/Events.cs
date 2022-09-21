namespace Utils
{
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

            // Objects 30 - 59
            SendNewObject = 30,
            SendDestroy,
            SendTransform,
            SendOwnership,

            // Chat 6X

            // Error 100 - 200 (temporaire)
        }
    }
}