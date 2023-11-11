using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lib;
using static System.Net.Mime.MediaTypeNames;
using static Lib.Lib;

namespace Server
{
    internal class Program
    {
        static UdpClient serverUDP = new UdpClient(PORT_SERVER);
        static IPEndPoint SERVER_IP_ENDPOINT = new IPEndPoint(IPAddress.Any, PORT_CLIENT);
        static int i = 0;

        static List<Frame> framesRecieved = new List<Frame>();

        static void Main(string[] args)
        {
            while (true)
            {
                startAndContinueListening();

   
            }
        }

        static void startAndContinueListening()
        {
            var data = serverUDP.Receive(ref SERVER_IP_ENDPOINT);
            var dataText = data.GetFrame().ToString();
            Console.WriteLine($"INN {dataText}");
            var dataSend = new Frame()
            {
                Type = Lib.Type.receive,
                Body = "Server response" + i,
            }.GetAsBytes();
            serverUDP.Send(dataSend, dataSend.Count(), SERVER_IP_ENDPOINT);
            i++;
        }
    }
}
