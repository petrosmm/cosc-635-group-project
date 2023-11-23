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
using System.Text.RegularExpressions;

namespace Client
{
    internal class Program
    {
        static readonly UdpClient sendClient = new UdpClient();
        static IPEndPoint localEP = new IPEndPoint(IPAddress.Any, PORT_CLIENT);
        static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_SERVER);
        static List<Frame> framesTotal = new List<Frame>();
        static int i = 0;
        static int numberUser = (int?)20 ?? Lib.Lib.GenerateRandom(Lib.Lib.GetRandom());

        static void Main(string[] args)
        {
            PrepareFrames();
            initStuff();

            // send first only
            if (false)
            {
                var data = framesTotal[i].GetAsBytes();
                sendClient.Send(data, data.Length, remoteEP);
                Console.WriteLine($"Sent packet: {framesTotal[i].ToStringAlt()}");
            }
            else
            {
                SendOne(i);
            }

            // newer old logic
            if (false)
                i++;

            Thread.Sleep(Lib.Lib.GetTimeSpanSeconds(1));

            while (true)
            {
                try
                {                 
                    if (sendClient.Available > 0) // Only read if we have some data 
                    {                           // queued in the network buffer. 
                        var frameReceivedAmbiguous = sendClient.Receive(ref localEP).GetFrame();

                        // sequence correction
                        if (frameReceivedAmbiguous.Type == Lib.Type.request)
                        {
                            i = frameReceivedAmbiguous.Sequence;
                            Console.WriteLine($"Resending {frameReceivedAmbiguous.ToStringAlt()}|***");
                        }
                        else if (frameReceivedAmbiguous.Type == Lib.Type.receive)
                        {
                            Console.WriteLine(frameReceivedAmbiguous.ToStringAlt());
                            var sequenceNumber = frameReceivedAmbiguous.Sequence;
                            i = sequenceNumber + 1;
                        }

                        SendOne(i);
                    }
                    else
                    {
                        SendOne(i);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                }

                // old logic
                if (false)
                    Thread.Sleep(Lib.Lib.GetTimeSpanSeconds(1));

                if (i == framesTotal.Count)
                {
                    Debugger.Break();
                }
            }
        }

        public static bool SendOne(int v)
        {
            var result = false;
            if (i < framesTotal.Count)
            {
                // old logic
                if (false)
                    Thread.Sleep(Lib.Lib.GetTimeSpanSeconds(1));
                var random = Lib.Lib.GenerateRandom(GetRandom());
                var iEnd = WINDOW_SIZE;
                var remainder = framesTotal.Count() % WINDOW_SIZE;
                if (i == framesTotal.Count() - remainder)
                {
                    iEnd = remainder;
                }

                var frameReference = framesTotal.GetRange(i, iEnd);
                var data = frameReference.GetAsBytes();

                // if we fail random
                if (random < numberUser)
                {
                    var message = $"\r\nLost packet: {frameReference.ToStringAlt()}\r\n";
                    Console.WriteLine(message);
                    result = false;
                    Thread.Sleep(Lib.Lib.GetTimeSpanSeconds(1));
                }
                else
                {
                    sendClient.Send(data, data.Length, remoteEP);
                    Console.WriteLine($"Sent packet: {data.GetFrames().ToStringAlt()}");
                    result = true;
                    Thread.Sleep(Lib.Lib.GetTimeSpanMs(25 * WINDOW_SIZE));
                }

                // newer old logic
                if (false)
                    i++;

                return result;
            }

            return true;
        }

        private static void initStuff()
        {

            sendClient.ExclusiveAddressUse = false;
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sendClient.Client.Bind(localEP);

            // old logic
            if (false)
                sendClient.BeginReceive(DataReceived, sendClient);
        }

        /// <summary>
        /// old logic
        /// </summary>
        /// <param name="ar"></param>
        private static void DataReceived(IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var frameRecieved = c.EndReceive(ar, ref receivedIpEndPoint).GetFrame();

            // sequence correction
            if (frameRecieved.Type == Lib.Type.request)
            {
                i = frameRecieved.Sequence;
                Console.WriteLine($"{frameRecieved.ToStringAlt()}|***");
            }
            else if (frameRecieved.Type == Lib.Type.receive)
            {
                Console.WriteLine(frameRecieved.ToStringAlt());
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


        public static void PrepareFrames()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            while (dir.Name != appName)
            {
                dir = Directory.GetParent(dir.FullName);
            }
            var @short = false ? "_short" : "_DataSent";
            var paths = new string[] { dir.ToString(), $"COSC635_P2{@short}.txt" };
            var target = Path.Combine(paths);
            Console.WriteLine(target);
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

                    framesTotal.Add(frame);

                    i++;
                }

                Console.WriteLine("finished reading file");
            }
        }
    }
}
