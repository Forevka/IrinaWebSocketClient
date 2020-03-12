using System;
using Websocket.Client;
using WebSocket.Enums;
using WebSocket.Models;
using WebSocket.Utils;

namespace WebSocket.Abstractions
{
    public interface IIrinaBotService : IDisposable
    {
        void StopTask(int taskId);
        int AddTask(Action<WebsocketClient> task, int sleepTime);
        void AddHandler(DefaultContext contextHeader, Func<BufferStream, WebsocketClient, HandlerWorkResult> handler);
        void AddHandler(GlobalContext contextHeader, Func<BufferStream, WebsocketClient, HandlerWorkResult> handler);
        void Start();
        void Dispose();
        void Send(BufferStream buffer);
    }
}
