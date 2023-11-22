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
using System.Diagnostics;

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
                StartAndContinueListening();
            }

            Debugger.Break();
        }

        static void StartAndContinueListening()
        {
            var dataRecievedRaw = serverUDP.Receive(ref SERVER_IP_ENDPOINT);
            var frameRecieved = dataRecievedRaw.GetFrame();
            var sequence = frameRecieved.Sequence;
            var frameMissing = framesRecieved.HasIssueWithPriors(sequence, new[] { frameRecieved }.ToList());
            var dataSend = new Frame();

            if (frameMissing != -1)
            {
                if (false)
                    Debugger.Break();
                Console.WriteLine($"REQUEST**** {frameRecieved.ToString()}");
                dataSend = new Frame()
                {
                    Type = Lib.Type.request,
                    Sequence = frameMissing
                };

                serverUDP.Send(dataSend.GetAsBytes(), dataSend.GetAsBytes().Length, SERVER_IP_ENDPOINT);
            }
            else
            {

                if (framesRecieved.Any(p => p.Sequence == frameRecieved.Sequence) == false)
                {
                    framesRecieved.Add(frameRecieved);

                    Console.WriteLine($"INCOMING {frameRecieved.ToString()}");
                    dataSend = new Frame()
                    {
                        Type = Lib.Type.receive,
                        Body = "Server response ->" + i,
                        Sequence = frameRecieved.Sequence
                    };

                    serverUDP.Send(dataSend.GetAsBytes(), dataSend.GetAsBytes().Length, SERVER_IP_ENDPOINT);
                }
            }

            if (frameRecieved.IsLast)
            {
                if (i == framesRecieved.Count)
                {
                    for (int i = 0; i < frameRecieved.Sequence; i++)
                    {

                        var item = framesRecieved.FirstOrDefault(p => p.Sequence == i);
                        if (item == null)
                        {
                            Debugger.Break();
                        }
                    }
                }                
            }
        }
    }
}
