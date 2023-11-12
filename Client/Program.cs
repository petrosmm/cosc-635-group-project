using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Lib;
using System.Threading;
using System.Text.Json;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using static Lib.Lib;

namespace Client
{
    internal class Program
    {
        static readonly UdpClient sendClient = new UdpClient();
        static int localPort = 49999;
        static int remotePort = 3000;
        static IPEndPoint localEP = new IPEndPoint(IPAddress.Any, localPort);
        static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), remotePort);
        static List<Frame> framesTotal = new List<Frame>();
        static List<Frame> framesToSend = new List<Frame>();
        static int i = 0;
        static int number_user = new Random().Next(99);
        static bool losePackets = true;
        static TimeSpan timeSpanSecond = new TimeSpan(0, 0, 1);
        static TimeSpan timeSpanSeconds = new TimeSpan(0, 0, 2);
        static TimeSpan timespanMs = new TimeSpan(0, 0, 0, 0, 1);

        static void Main(string[] args)
        {
            
            prepareFrames();
            initStuff();

            // send first only
            if (true)
            {
                var data = framesTotal[i].GetAsBytes();
                sendClient.Send(data, data.Length, remoteEP);
                Console.WriteLine($"Sent packet: {framesTotal[i].ToString()}");
                i++;
                Thread.Sleep(timeSpanSeconds);
            }
            else
            {
                SendOne(0);
                if (false)
                {
                    while (i < framesTotal.Count)
                    {
                        if (true)
                            Thread.Sleep(timespanMs);
                        var random = new Random().Next(99);

                        if (i == 0 || number_user >= random)
                        {
                            var data = framesTotal[i].GetAsBytes();
                            sendClient.Send(data, data.Length, remoteEP);
                            Console.WriteLine($"Sent packet: {framesTotal[i].ToString()}");
                        }
                        else
                        {
                            var x = $"Lost packet: {framesTotal[i].ToString()}";
                            Console.WriteLine(x);
                        }

                        i++;
                    }
                }
            }

            if (i <= framesTotal.Count)
            {
                Console.ReadLine();
            }
        }

        private static void DataReceived(IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var frameRecieved = c.EndReceive(ar, ref receivedIpEndPoint).GetFrame();

            // correction
            if (frameRecieved.Type == Lib.Type.request)
            {
                i = frameRecieved.Sequence;
                Console.WriteLine($"{frameRecieved.ToString()}|***");
            }
            else if (frameRecieved.Type == Lib.Type.receive)
            {
                Console.WriteLine(frameRecieved.ToString());
                var sequenceNumber = frameRecieved.Sequence;
                i = sequenceNumber + 1;
            }

            var sendResult = SendOne(i);
            while (sendResult == false)
            {
                sendResult = SendOne(i);
            }

            c.BeginReceive(DataReceived, ar.AsyncState);
        }

        public static bool SendOne(int v)
        {
            var result = false;
            if (i < framesTotal.Count)
            {
                if (true)
                    Thread.Sleep(timeSpanSeconds);
                var random = new Random().Next(99);

                if (number_user < random)
                {
                    var x = $"Lost packet: {framesTotal[i].ToString()}";
                    Console.WriteLine(x);
                    result = false;
                }
                else
                {
                    var data = framesTotal[i].GetAsBytes();
                    sendClient.Send(data, data.Length, remoteEP);
                    Console.WriteLine($"Sent packet: {data.GetFrame().ToString()}");
                    result = true;
                    i++;
                }

               // i++;
                return result;
            }

            return true;
        }

        private static void initStuff()
        {

            sendClient.ExclusiveAddressUse = false;
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sendClient.Client.Bind(localEP);
            sendClient.BeginReceive(DataReceived, sendClient);
        }


        public static void prepareFrames()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            while (dir.Name != appName)
            {
                dir = Directory.GetParent(dir.FullName);
            }
            string target = $@"{dir}\COSC635_P2_short.txt";

            using (var _streamReader = new StreamReader(target))
            {
                var rawData = null as string;
                var i = 0;
                while ((rawData = _streamReader.ReadLine()) != null)
                {
                    if (i != 0)
                    {

                    }
                    // header logic
                    else
                    {

                    }

                    var frame = new Frame()
                    {
                        Type = Lib.Type.send,
                        Sequence = i,
                        Body = rawData,
                        IsLast = _streamReader.Peek() == -1
                    };
                    framesToSend.Add(frame);
                    framesTotal.Add(frame);

                    i++;
                }

                Console.WriteLine("finished reading file");
            }
        }
    }
}
