using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Websocket.Client;
using WebSocket.Enums;
using WebSocket.Models;
using WebSocket.Services;
using WebSocket.Utils;

namespace WebSocket
{
    public static class IrinaExtensions 
    {
        public static void GetMapInfo(this IrinaBotService client, int gameId)
        {
            var buffer = new BufferStream(7, 1);
            buffer.Write((byte)ContextType.DefaultContext);
            buffer.Write((byte)DefaultContext.GetMapInfo);
            buffer.Write(gameId);

            client.Send(buffer);
        }
    }

    class Program
    {
        private IrinaBotService _irka;
        static void Main()
        {
            var p = new Program();
            p.Niam();
        }

        private void Niam()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (_irka = new IrinaBotService("wss://irinabot.ru/ghost/"))
            {
                _irka.AddHandler(DefaultContext.NewMessage, HandleMessage);
                _irka.AddHandler(DefaultContext.GameList, HandleGameList);
                _irka.AddHandler(DefaultContext.WebSocketConnect, HandlePing);
                _irka.AddHandler(DefaultContext.MapInfo, HandleMapInfo);

                _irka.GetMapInfo(13682);

                _irka.AddTask(GetGameList, 2000);
                _irka.AddTask(GetUserCount, 2000);

                _irka.Start();
            }
        }

        private HandlerWorkResult HandleMapInfo(BufferStream stream, WebsocketClient client)
        {
            var map = new GameMapModel(stream);

            map.SavePicture(map.Author + "___" + map.Name + ".tga");

            Console.WriteLine("Map: " + map.Name + "Author: " + map.Author);
            
            return new HandlerWorkResult()
            {
                IsOk = true,
            };
        }

        private void GetGameList(WebsocketClient client)
        {
            client.Send(new[] { (byte)ContextType.DefaultContext, (byte)DefaultContext.GetGameList });
        }

        private void GetUserCount(WebsocketClient client)
        {
            client.Send(new[] { (byte)ContextType.DefaultContext, (byte)DefaultContext.GetWebsocketConnect });
        }

        private HandlerWorkResult HandlePing(BufferStream stream, WebsocketClient client)
        {
            stream.Read(out int totalUsers);
            Console.WriteLine("Total users: " + totalUsers);
            
            return new HandlerWorkResult()
            {
                IsOk = true,
            };
        }

        private HandlerWorkResult HandleMessage(BufferStream stream, WebsocketClient client)
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

            return new HandlerWorkResult()
            {
                IsOk = true,
            };
        }

        private HandlerWorkResult HandleGameList(BufferStream stream, WebsocketClient client)
        {
            stream.Read(out ushort gameCount);
            Console.WriteLine(gameCount);

            var gameList = new List<GameModel>();

            for (int i = 0; i < gameCount; i++)
            {
                var game = new GameModel(stream);

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
                    Console.WriteLine("\tPlayer: " + player.Name + " color: " + player.Color);
                }
            }

            return new HandlerWorkResult()
            {
                IsOk = true,
            };
        }
    }
}
