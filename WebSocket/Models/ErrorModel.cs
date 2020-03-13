using System;
using System.Collections.Generic;
using System.Text;
using WebSocket.Utils;

namespace WebSocket.Models
{
    class ErrorModel : IBaseGameModel
    {
        public sbyte ErrorCode { get; set; } 
        public string Description { get; set; }

        public ErrorModel(BufferStream stream)
        {
            FromStream(stream);
        } 

        public void FromStream(BufferStream stream)
        {
            stream.Read(out sbyte errorCode);
            stream.Read(out string description);

            ErrorCode = errorCode;
            Description = description;
        }

        public new string ToString()
        {
            return $"GlobalContext Error: {ErrorCode}, {Description}";
        }
    }
}
