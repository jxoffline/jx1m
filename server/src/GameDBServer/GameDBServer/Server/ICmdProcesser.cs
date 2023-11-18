namespace GameDBServer.Server
{
    public interface ICmdProcessor
    {
        void processCmd(GameServerClient client, int nID, byte[] cmdParams, int count);
    }
}