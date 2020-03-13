using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Websocket.Client;
using Websocket.Client.Models;
using WebSocket.Abstractions;
using WebSocket.Enums;
using WebSocket.Models;
using WebSocket.Utils;

namespace WebSocket.Services
{
    public class IrinaBotService: IIrinaBotService
    {
        private static Random _random = new Random();

        private readonly WebsocketClient _client;
        private readonly ManualResetEvent _resetEvent;

        private readonly Dictionary<DefaultContext, List<Func<BufferStream, WebsocketClient, HandlerWorkResult>>> _handlersDefaultContext;
        private readonly Dictionary<GlobalContext, List<Func<BufferStream, WebsocketClient, HandlerWorkResult>>> _handlersGlobalContext;

        private readonly Dictionary<int, Tuple<CancellationTokenSource, Thread>> _tasks;

        public IrinaBotService(string webSocketUrl)
        {
            _client = new WebsocketClient(new Uri(webSocketUrl));
            _resetEvent = new ManualResetEvent(false);

            _tasks = new Dictionary<int, Tuple<CancellationTokenSource, Thread>>();

            _handlersDefaultContext = new Dictionary<DefaultContext, List<Func<BufferStream, WebsocketClient, HandlerWorkResult>>>();
            _handlersGlobalContext = new Dictionary<GlobalContext, List<Func<BufferStream, WebsocketClient, HandlerWorkResult>>>();

            _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _client.ReconnectionHappened.Subscribe(ReconnectHandler);
            _client.MessageReceived.Subscribe(Dispatch);
        }

        public void Send(BufferStream buffer)
        {
            _client.Send(buffer.Memory);
        }

        public int AddTask(Action<WebsocketClient> task, int sleepTime)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;
            var taskId = _tasks.Count;

            var thread = new Thread(() => Task(task, sleepTime, taskId, cancelToken));
            thread.Start();

            _tasks.Add(taskId, new Tuple<CancellationTokenSource, Thread>(cancelTokenSource, thread));
            return taskId;
        }

        public void StopTask(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var threadTuple))
                if (!threadTuple.Item1.IsCancellationRequested)
                    threadTuple.Item1.Cancel();
        }

        public void Task(Action<WebsocketClient> task, int sleepTime, int taskId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(sleepTime);
                task.Invoke(_client);
            }
        }

        public void AddHandler(DefaultContext contextHeader, Func<BufferStream, WebsocketClient, HandlerWorkResult> handler)
        {
            if (!_handlersDefaultContext.TryGetValue(contextHeader, out var hList))
            {
                hList = new List<Func<BufferStream, WebsocketClient, HandlerWorkResult>>();
                _handlersDefaultContext.Add(contextHeader, hList);
            }

            hList.Add(handler);
        }

        public void AddHandler(GlobalContext contextHeader, Func<BufferStream, WebsocketClient, HandlerWorkResult> handler)
        {
            if (!_handlersGlobalContext.TryGetValue(contextHeader, out var hList))
            {
                hList = new List<Func<BufferStream, WebsocketClient, HandlerWorkResult>>();
                _handlersGlobalContext.Add(contextHeader, hList);
            }

            hList.Add(handler);
        }

        public void Start()
        {
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

                    Console.WriteLine($"Error! cant find handlers for: {header} context {context}");

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

                        return;
                    }

                    Console.WriteLine($"Error! cant find handlers for: {header} context {context}");

                    return;
                }
                default:
                {
                    Console.WriteLine($"Error! Unknown context {context}!");

                    return;
                }
            }
        }

        public void SetConnectorName(string name)
        {
            var buffer = new BufferStream(2 + name.ToCharArray().Length * 2, 1);
            buffer.Write((byte)ContextType.GlobalContext);
            buffer.Write((byte)GlobalContext.SetConnectorName);
            buffer.Write(name);

            Send(buffer);
        }

        public void AnonimAuth()
        {
            var token = RandomString("Awmb2H3jYSo5u4XKR2meMbl30kU6ae".Length);
            Auth(token);
            SetConnectorName("qwerqwerqwer");
        }

        public void Auth(string token)
        {
            var buffer = new BufferStream(1024, 1);
            buffer.Write((byte)ContextType.GlobalContext);
            buffer.Write((byte)GlobalContext.UserAuth);
            buffer.Write((byte)1); //discord
            buffer.Write((byte)0);
            buffer.Write(token);

            Send(buffer);
        }

        private void ReconnectHandler(ReconnectionInfo info)
        {
            Console.WriteLine($"Reconnection happened, type: {info.Type}");
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public void Dispose()
        {
            _client.Dispose();
            _resetEvent.Dispose();
        }
    }
}
