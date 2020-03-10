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
    class IrinaBotService : IIrinaBotService
    {
        private readonly WebsocketClient _client;
        private readonly ManualResetEvent _resetEvent;

        public IrinaBotService()
        {
            _client = new WebsocketClient(new Uri("wss://irinabot.ru/ghost/"));
            _resetEvent = new ManualResetEvent(false);

            _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _client.ReconnectionHappened.Subscribe(ReconnectHandler);
            _client.MessageReceived.Subscribe(Dispatch);
        }

        public void Start()
        {
            _client.Send(new[] { (byte)ContextType.DefaultContext, (byte)DefaultContext.GetGameList });

            _client.Start();

            _resetEvent.WaitOne();
        }

        private void Dispatch(ResponseMessage msg)
        {
            Console.WriteLine(msg.ToString());
            BufferStream stream = new BufferStream(msg.Binary.Length, 1);
            stream.ReassignMemory(msg.Binary);

            if (msg.Binary.Length <= 2) return;

            stream.Read(out byte header);
            stream.Read(out byte context);

            if (header == (byte)ContextType.DefaultContext)
            {
                if (context == (byte)DefaultContext.NewMessage)
                {
                    HandleMessage(stream);
                }
                else if (context == (byte)DefaultContext.GameList)
                {
                    HandleGameList(stream);
                }
            }
        }


        private void HandleMessage(BufferStream stream)
        {
            Console.WriteLine("new chat message");
            string chatMsg = "";
            try
            {
                stream.Read(out string msgStr);
                chatMsg += " " + msgStr;
                stream.Read(out msgStr);
                chatMsg += " " + msgStr;
                stream.Read(out msgStr);
                chatMsg += " " + msgStr;
                stream.Read(out msgStr);
                chatMsg += " " + msgStr;

                Console.WriteLine(chatMsg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void HandleGameList(BufferStream stream)
        {
            Console.WriteLine("game list");
            stream.Read(out ushort gameCount);
            Console.WriteLine(gameCount);

            var gameList = new List<GameModel>();

            for (int i = 0; i < gameCount; i++)
            {
                stream.Read(out sbyte started);
                stream.Read(out string name);
                stream.Read(out sbyte hasAdmin);
                stream.Read(out sbyte hasPassword);
                stream.Read(out sbyte hasGamePowerUp);
                stream.Read(out int gameCounter);
                stream.Read(out int gameTicks);
                stream.Read(out string iccupHost);
                stream.Read(out sbyte slotflsg);
                stream.Read(out sbyte maxPlayers);
                stream.Read(out sbyte playersCount);
                var game = new GameModel
                {
                    Started = started,
                    Name = name,
                    GameCounter = gameCounter,
                    GameTicks = gameTicks,
                    HasAdmin = hasAdmin,
                    HasGamePowerUp = hasGamePowerUp,
                    HasPassword = hasPassword,
                    IccupHost = iccupHost,
                    MaxPlayers = maxPlayers,
                    PlayersCount = playersCount,
                    Slotflsg = slotflsg,
                    Players = new List<GamePlayerModel>()
                };


                for (int j = 0; j < playersCount; j++)
                {
                    stream.Read(out sbyte color);
                    stream.Read(out string pName);
                    stream.Read(out string relam);
                    stream.Read(out string comment);
                    var player = new GamePlayerModel
                    {
                        Color = color, 
                        Comment = comment,
                        Name = pName, 
                        Relam = relam
                    };

                    game.Players.Add(player);
                }
                gameList.Add(game);
            }

            foreach (var game in gameList.Where(x => x.Started == 0))
            {
                Console.WriteLine("Game:" + game.Name);
                Console.WriteLine("Started:" + game.Started);
                Console.WriteLine("HasPassword:" + game.HasPassword);
                Console.WriteLine("Tick:" + game.GameTicks);
                Console.WriteLine("Counter:" + game.GameCounter);
                foreach (var player in game.Players)
                {
                    Console.WriteLine("\tPlayer: " + player.Name);
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
