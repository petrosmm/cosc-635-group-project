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
        static List<Frame> framesProcessed = new List<Frame>();

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
            var framesRecieved = dataRecievedRaw.GetFrames();
            var frameRefStart = framesRecieved.FirstOrDefault();
            var frameRefEnd = framesRecieved.LastOrDefault();
            var sequence = frameRefStart.Sequence;
            var frameSequenceMissing = framesProcessed.HasIssueWithPriors(framesRecieved);
            var dataSend = new Frame();

            if (frameSequenceMissing != -1)
            {
                if (false)
                    Debugger.Break();
                Console.WriteLine($"REQUEST**** {frameRefStart.ToStringAlt()}");
                dataSend = new Frame()
                {
                    Type = Lib.Type.request,
                    Sequence = frameSequenceMissing
                };

                serverUDP.Send(dataSend.GetAsBytes(), dataSend.GetAsBytes().Length, SERVER_IP_ENDPOINT);
            }
            else
            {
                if (framesProcessed.Any(p => p.Sequence == frameRefStart.Sequence) == false)
                {
                    framesProcessed.AddRange(framesRecieved);

                    Console.WriteLine($"INCOMING {frameRefStart.Sequence} to {framesRecieved.GetSequenceNumberTrailer()}");
                    dataSend = new Frame()
                    {
                        Type = Lib.Type.receive,
                        Body = $"Server response -> for {frameRefStart.Sequence} to {framesRecieved.GetSequenceNumberTrailer()}",
                        Sequence = framesRecieved.GetSequenceNumberTrailer()
                    };

                    serverUDP.Send(dataSend.GetAsBytes(), dataSend.GetAsBytes().Length, SERVER_IP_ENDPOINT);
                }
            }

            if (frameRefEnd.IsLast)
            {
                if (i == framesProcessed.Count)
                {
                    for (int i = 0; i < frameRefStart.Sequence; i++)
                    {
                        var item = framesProcessed.FirstOrDefault(p => p.Sequence == i);
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
