using System.Collections.Generic;

namespace WebSocket.Models
{
    public class GameModel
    {
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
    }
}
