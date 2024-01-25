using System;
using TexasHoldemServer.Servers;

namespace TexasHoldemServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(5555);
            Console.WriteLine("服務端啟動..");
            Console.Read();
        }
    }
}
