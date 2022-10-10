namespace Utils
{
    /// <summary>
    /// Enum holding the different codes used for communication between the different clients
    /// </summary>
    public enum EventCode : byte
    {
        // Room 1 - 9

        // Player 1X
        /// <summary> Code used for creating a new post-it </summary>
        SendNewPostIt = 10,
        /// <summary> Code used for updating the position of a player </summary>
        SendNewPosition,
        /// <summary> Code used for signaling a player joining the room </summary>
        SendNewPlayerIn,
        /// <summary> Code used for creating a new ping </summary>
        SendNewPing,

        // Tools 2X
        /// <summary> Code used for sending a modification made by the marker </summary>
        Marker = 20,
        /// <summary> Code used for sending a modification made by the eraser </summary>
        Eraser,
        /// <summary> Code used for sending the whole texture </summary>
        Texture = 29,

        // Objects 30 - 59
        /// <summary> Code used for creating a new object </summary>
        SendNewObject = 30,
        /// <summary> Code used for destroying an object </summary>
        SendDestroy,
        /// <summary> Code used for updating the transform of an object </summary>
        SendTransform,
        /// <summary> Code used for updating the ownership of an object </summary>
        SendOwnership,

        // Chat 6X
            
        // Error 100 - 200 (temporary)
    }
}