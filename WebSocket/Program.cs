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
        static void Main()
        {
            var p = new Program();
            p.Niam();
        }

        private void Niam()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (IrinaBotService irka = new IrinaBotService("wss://irinabot.ru/ghost/"))
            {
                irka.AddHandler(DefaultContext.NewMessage, HandleMessage);
                irka.AddHandler(DefaultContext.GameList, HandleGameList);
                irka.AddHandler(DefaultContext.WebSocketConnect, HandlePing);
                irka.AddHandler(DefaultContext.MapInfo, HandleMapInfo);

                irka.GetMapInfo(13682);

                irka.AddTask(GetGameList, 2000);
                irka.AddTask(Ping, 2000);

                irka.Start();
            }
        }

        private Dictionary<string, object> HandleMapInfo(BufferStream stream, WebsocketClient client)
        {
            var map = new GameMapModel(stream);

            map.SavePicture(map.Author + "___" + map.Name + ".tga");

            Console.WriteLine("Map: " + map.Name + "Author: " + map.Author);
            
            return new Dictionary<string, object>();
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
                var game = new GameModel(stream);

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
