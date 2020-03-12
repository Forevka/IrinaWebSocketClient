using System.Collections.Generic;

namespace WebSocket.Models
{
    public class HandlerWorkResult
    {
        public bool IsOk { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}
