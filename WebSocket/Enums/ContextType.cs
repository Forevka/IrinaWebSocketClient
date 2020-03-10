using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket.Enums
{
    enum ContextType : byte
    {
        DefaultContext = 0x01,
        GlobalContext = 0x00,
    }
}
