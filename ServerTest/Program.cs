using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new Random(); 
            var listener = new TcpListener(IPAddress.Any, 9001);
            listener.Start();
            var client = listener.AcceptTcpClient();
            using(var stream = client.GetStream())
            {
                while(client.Connected)
                {
                    byte b = 0;
                    while(stream.DataAvailable)
                    {
                        b = (byte)stream.ReadByte();
                        Console.WriteLine("<- {0}", b);
                    }

                    b = (byte)r.Next(256);
                    Console.WriteLine("-> {0}", b);
                    stream.WriteByte(b);
                }
            }
        }
    }
}
