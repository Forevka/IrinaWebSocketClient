using System;

namespace WebSocket.Abstractions
{
    internal interface IIrinaBotService : IDisposable
    {
        void Start();
        void Dispose();
    }
}
