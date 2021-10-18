namespace MainGame
{
    namespace Message
    {
        public enum MessageType
        {
            DAMAGED,
            DEAD,
            RESPAWN,
            //Add your user defined message type after
        }

        public interface IMessageReceiver
        {
            void OnReceiveMessage(MessageType type, object sender, object msg);
        }
    }
}

