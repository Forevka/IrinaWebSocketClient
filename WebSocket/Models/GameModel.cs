using System.Collections.Generic;
using WebSocket.Utils;

namespace WebSocket.Models 
{
    public class GameModel : IBaseGameModel
    {
        public GameModel(BufferStream stream)
        {
            FromStream(stream);
        }

        public sbyte Started { get; set; }
        public string Name { get; set; }
        public sbyte HasAdmin { get; set; }
        public sbyte HasPassword { get; set; }
        public sbyte HasGamePowerUp { get; set; }
        public int GameCounter { get; set; }
        public int GameTicks { get; set; }
        public string IccupHost { get; set; }
        public sbyte Slotflsg { get; set; }
        public sbyte MaxPlayers { get; set; }
        public sbyte PlayersCount { get; set; }
        public List<GamePlayerModel> Players { get; set; }
        public void FromStream(BufferStream stream)
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

            Started = started;
            Name = name;
            GameCounter = gameCounter;
            GameTicks = gameTicks;
            HasAdmin = hasAdmin;
            HasGamePowerUp = hasGamePowerUp;
            HasPassword = hasPassword;
            IccupHost = iccupHost;
            MaxPlayers = maxPlayers;
            PlayersCount = playersCount;
            Slotflsg = slotflsg;
            Players = new List<GamePlayerModel>();

            for (int j = 0; j < playersCount; j++)
            {
                var player = new GamePlayerModel(stream);

                Players.Add(player);
            }
        }
    }
}
