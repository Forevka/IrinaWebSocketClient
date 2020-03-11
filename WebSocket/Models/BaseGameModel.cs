using System;
using System.Collections.Generic;
using System.Text;
using WebSocket.Utils;

namespace WebSocket.Models
{
    interface IBaseGameModel
    {
        void FromStream(BufferStream stream);
    }
}
