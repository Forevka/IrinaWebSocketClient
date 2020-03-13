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

        public static void SendMessage(this IrinaBotService client, string toUser, string from, string text)
        {
            var buffer = new BufferStream(2 + toUser.ToCharArray().Length * 2 + from.ToCharArray().Length * 2 + text.ToCharArray().Length * 2, 1);
            buffer.Write((byte)ContextType.DefaultContext);
            buffer.Write((byte)DefaultContext.SendMessage);
            buffer.Write(toUser);
            buffer.Write(from);
            buffer.Write(text);

            client.Send(buffer);
        }

        public static void CreateGame(this IrinaBotService client, byte isPrivate, byte patch, string mapName, string gameName, string owner)
        {
            var buffer = new BufferStream(1024, 1);
            buffer.Write((byte)ContextType.DefaultContext);
            buffer.Write((byte)DefaultContext.CreateGame);
            buffer.Write(isPrivate); //isPrivate
            buffer.Write(patch); //default "1" but need to write as sbyte
            buffer.Write((byte)0); // ???
            buffer.Write(mapName);
            buffer.Write(gameName);
            buffer.Write(owner); //Forevka

            client.Send(buffer);
        }

        public static void SendPrivateMessage(this IrinaBotService client, string toUser, string text)
        {
            client.SendMessage(toUser, "whisper", text);
        }

        public static void SendGlobalMessage(this IrinaBotService client, string text)
        {
            client.SendMessage("", "chat", text);
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
                _irka.AddHandler(DefaultContext.CreateGameResponse, HandleCreateGame);

                _irka.AddHandler(GlobalContext.UserAuthResponse, HandleUserAuth);
                _irka.AddHandler(GlobalContext.GetError, HandleError);

                //_irka.GetMapInfo(13682);

                //_irka.AddTask(GetGameList, 2000);
                //_irka.AddTask(GetUserCount, 2000);
                _irka.Auth("Awmb2H3jYSo5u4XKR2meMbl30kU6ae");
                //_irka.SetConnectorName("test");
                //_irka.SendPrivateMessage("Forevka__", "qweqweqweqweqweqweqweqweqwe");
                _irka.CreateGame(0,1, "(4)ColdHeart.w3x", "qweqwe1", "1Forevka");
                //_irka.SendGlobalMessage("test from cli anon");

                _irka.Start();
            }
        }

        private HandlerWorkResult HandleCreateGame(BufferStream stream, WebsocketClient client)
        {
            stream.Read(out sbyte code);
            stream.Read(out string description);
            stream.Read(out string password);

            Console.WriteLine($"Created game response: {code}, {description} -- {password}");

            return new HandlerWorkResult()
            {
                IsOk = true,
            };
        }

        private HandlerWorkResult HandleError(BufferStream stream, WebsocketClient client)
        {
            var error = new ErrorModel(stream);
            Console.WriteLine(error.ToString());

            return new HandlerWorkResult()
            {
                IsOk = true,
            };
        }

        private HandlerWorkResult HandleUserAuth(BufferStream stream, WebsocketClient client)
        {
            stream.Read(out string id);
            stream.Read(out string nickname);
            stream.Read(out string avatarUrl);

            stream.Read(out int userId);

            Console.WriteLine(id + nickname + avatarUrl + userId);

            return new HandlerWorkResult()
            {
                IsOk = true,
            };
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
