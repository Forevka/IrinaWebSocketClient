using System;
using System.Collections.Generic;
using Websocket.Client;
using WebSocket.Enums;
using WebSocket.Utils;

namespace WebSocket.Abstractions
{
    public interface IIrinaBotService : IDisposable
    {
        void AddTask(Func<WebsocketClient, bool> task, int sleepTime);
        void AddHandler(DefaultContext contextHeader, Func<BufferStream, WebsocketClient, Dictionary<string, object>> handler);
        void AddHandler(GlobalContext contextHeader, Func<BufferStream, WebsocketClient, Dictionary<string, object>> handler);
        void Start();
        void Dispose();
        void Send(BufferStream buffer);
    }
}
