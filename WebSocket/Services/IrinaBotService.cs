using System;
using System.Collections.Generic;
using System.Threading;
using Websocket.Client;
using Websocket.Client.Models;
using WebSocket.Abstractions;
using WebSocket.Enums;
using WebSocket.Utils;

namespace WebSocket.Services
{
    class IrinaBotService: IIrinaBotService
    {
        private readonly WebsocketClient _client;
        private readonly ManualResetEvent _resetEvent;

        private readonly Dictionary<DefaultContext, List<Func<BufferStream, WebsocketClient, Dictionary<string, object>>>> _handlersDefaultContext;
        private readonly Dictionary<GlobalContext, List<Func<BufferStream, WebsocketClient, Dictionary<string, object>>>> _handlersGlobalContext;

        private readonly List<Thread> _tasks;

        public IrinaBotService(string webSocketUrl)
        {
            _client = new WebsocketClient(new Uri(webSocketUrl));
            _resetEvent = new ManualResetEvent(false);

            _tasks = new List<Thread>();

            _handlersDefaultContext = new Dictionary<DefaultContext, List<Func<BufferStream, WebsocketClient, Dictionary<string, object>>>>();
            _handlersGlobalContext = new Dictionary<GlobalContext, List<Func<BufferStream, WebsocketClient, Dictionary<string, object>>>>();

            _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _client.ReconnectionHappened.Subscribe(ReconnectHandler);
            _client.MessageReceived.Subscribe(Dispatch);
        }

        public void AddTask(Func<WebsocketClient, bool> task, int sleepTime)
        {
            var thread = new Thread(() => Task(task, sleepTime));
            thread.Start();
            _tasks.Add(thread);
        }

        public bool Task(Func<WebsocketClient, bool> task, int sleepTime)
        {
            while (true)
            {
                Thread.Sleep(sleepTime);
                task.Invoke(_client);
            }
        }

        public void AddHandler(DefaultContext contextHeader, Func<BufferStream, WebsocketClient, Dictionary<string, object>> handler)
        {
            if (!_handlersDefaultContext.TryGetValue(contextHeader, out var hList))
            {
                hList = new List<Func<BufferStream, WebsocketClient, Dictionary<string, object>>>();
                _handlersDefaultContext.Add(contextHeader, hList);
            }

            hList.Add(handler);
        }

        public void AddHandler(GlobalContext contextHeader, Func<BufferStream, WebsocketClient, Dictionary<string, object>> handler)
        {
            if (!_handlersGlobalContext.TryGetValue(contextHeader, out var hList))
            {
                hList = new List<Func<BufferStream, WebsocketClient, Dictionary<string, object>>>();
                _handlersGlobalContext.Add(contextHeader, hList);
            }

            hList.Add(handler);
        }

        public void Start()
        {
            //_client.Send(new[] { (byte)ContextType.DefaultContext, (byte)DefaultContext.GetGameList });

            _client.Start();

            _resetEvent.WaitOne();
        }

        private void Dispatch(ResponseMessage msg)
        {
            //Console.WriteLine(msg.ToString());
            BufferStream stream = new BufferStream(msg.Binary.Length, 1);
            stream.ReassignMemory(msg.Binary);

            if (msg.Binary.Length <= 2) return;

            stream.Read(out byte context);
            stream.Read(out byte header);

            switch (context)
            {
                case (byte)ContextType.DefaultContext:
                {
                    if (_handlersDefaultContext.TryGetValue((DefaultContext)header, out var hList))
                    {
                        foreach (var handler in hList)
                        {
                            var res = handler.Invoke(stream, _client);
                        }

                        return;
                    }

                    Console.WriteLine($"Error! cant find handlers for: {header}");

                    return;
                }
                case (byte)ContextType.GlobalContext:
                {
                    if (_handlersGlobalContext.TryGetValue((GlobalContext)header, out var hList))
                    {
                        foreach (var handler in hList)
                        {
                            var res = handler.Invoke(stream, _client);
                        }
                    }

                    Console.WriteLine($"Error! cant find handlers for: {header}");

                    return;
                }
                default:
                {
                    Console.WriteLine($"Error! Unknown context {context}!");

                    return;
                }
            }
        }

        private void ReconnectHandler(ReconnectionInfo info)
        {
            Console.WriteLine($"Reconnection happened, type: {info.Type}");
        }

        public void Dispose()
        {
            _client.Dispose();
            _resetEvent.Dispose();
        }
    }
}
