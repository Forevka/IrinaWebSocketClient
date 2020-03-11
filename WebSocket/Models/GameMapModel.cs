using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebSocket.Utils;

namespace WebSocket.Models
{
    public class GameMapModel : IBaseGameModel
    {
        public GameMapModel(BufferStream stream)
        {
            FromStream(stream);
        }

        public string Name { get; set; }
        public int TgaSize { get; set; }
        public byte[] Tga { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Players { get; set; }
        public void FromStream(BufferStream stream)
        {
            stream.Read(out string mapName);
            stream.Read(out int tgaSize);
            stream.Read(out byte[] tga, tgaSize);
            stream.Read(out string author);
            stream.Read(out string description);
            stream.Read(out string players);

            Name = mapName;
            Author = author;
            Description = description;
            Players = players;
            TgaSize = tgaSize;
            Tga = tga;
        }

        public void SavePicture(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(Tga, 0, TgaSize);
            }
        }
    }
}
