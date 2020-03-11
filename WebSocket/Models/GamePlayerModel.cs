using WebSocket.Utils;

namespace WebSocket.Models
{
    public class GamePlayerModel : IBaseGameModel
    {
        public GamePlayerModel(BufferStream stream)
        {
            FromStream(stream);
        }

        public sbyte Color { get; set; }
        public string Name { get; set; }
        public string Relam { get; set; }
        public string Comment { get; set; }
        public void FromStream(BufferStream stream)
        {
            stream.Read(out sbyte color);
            stream.Read(out string pName);
            stream.Read(out string relam);
            stream.Read(out string comment);

            Color = color;
            Comment = comment;
            Name = pName;
            Relam = relam;
        }
    }
}
