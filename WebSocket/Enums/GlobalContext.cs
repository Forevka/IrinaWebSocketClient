namespace WebSocket.Enums
{
    public enum GlobalContext : byte
    {
        // Server answer constant
        GetError = 0x00,
        Pong = 0x02,
        UserAuthResponse = 0x03,
        BnetKey = 0x04,
        IntegrationByToken = 0x05,
        SetConnectorName = 0x06,
        DeleteIntegration = 0x07,

        // Client request constant
        SendError = 0x00,
        Ping = 0x02,
        UserAuth = 0x03,
        GetBnetKey = 0x04,
        AddIntegrationByToken = 0x05,
    }
}
