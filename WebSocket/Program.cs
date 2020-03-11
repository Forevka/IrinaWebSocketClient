using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket.Abstractions;
using Websocket.Client;
using WebSocket.Enums;
using WebSocket.Models;
using WebSocket.Services;
using WebSocket.Utils;

namespace WebSocket
{
    class Program
    {
        static void Main()
        {
            var p = new Program();
            p.Niam();
        }

        private void Niam()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (IIrinaBotService irka = new IrinaBotService("wss://irinabot.ru/ghost/"))
            {
                irka.AddHandler(DefaultContext.NewMessage, HandleMessage);
                irka.AddHandler(DefaultContext.GameList, HandleGameList);
                irka.AddHandler(DefaultContext.WebSocketConnect, HandlePing);

                irka.AddTask(GetGameList, 2000);
                irka.AddTask(Ping, 2000);

                irka.Start();
            }
        }

        private bool GetGameList(WebsocketClient client)
        {
            client.Send(new[] { (byte)ContextType.DefaultContext, (byte)DefaultContext.GetGameList });

            return true;
        }

        private bool Ping(WebsocketClient client)
        {
            client.Send(new[] { (byte)ContextType.DefaultContext, (byte)DefaultContext.GetWebsocketConnect });

            return true;
        }

        private Dictionary<string, object> HandlePing(BufferStream stream, WebsocketClient client)
        {
            stream.Read(out int totalUsers);
            Console.WriteLine("Total users: " + totalUsers);

            return new Dictionary<string, object>();
        }

        private Dictionary<string, object> HandleMessage(BufferStream stream, WebsocketClient client)
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

            return new Dictionary<string, object>();
        }

        private Dictionary<string, object> HandleGameList(BufferStream stream, WebsocketClient client)
        {
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

            foreach (var game in gameList.Where(x => x.Started == 1))
            {
                Console.WriteLine("Game:" + game.Name);
                Console.WriteLine("Started:" + game.Started);
                Console.WriteLine("HasPassword:" + game.HasPassword);
                Console.WriteLine("Tick:" + game.GameTicks);
                Console.WriteLine("Counter:" + game.GameCounter);
                foreach (var player in game.Players)
                {
                    Console.WriteLine("\tPlayer: " + player.Name + " color: " + player.Color);
                }
            }

            return new Dictionary<string, object>();
        }
    }
}
