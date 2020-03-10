using System;
using System.Text;
using WebSocket.Abstractions;
using WebSocket.Services;

namespace WebSocket
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (IIrinaBotService irka = new IrinaBotService())
            {
                irka.Start();
            }
        }
    }
}
