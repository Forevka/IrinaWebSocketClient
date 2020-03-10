namespace WebSocket.Enums
{
    public enum DefaultContext : byte
    {
        // Server answer constant
        Welcome = 0x00,
        GameList = 0x01,
        UdpAnswer = 0x03,
        CreateGameResponse = 0x04,
        WebSocketConnect = 0x05,
        NewMessage = 0x0C,
        MapInfo = 0x0D,

        // Client request constant
        ContextRequest = 0x00,
        GetGameList = 0x01,
        SendGameExternalSignal = 0x02,
        GetUdpGame = 0x03,
        CreateGame = 0x04,
        GetWebsocketConnect = 0x05,
        SendMessage = 0x0C,
        GetMapInfo = 0x0D,
    }
}
