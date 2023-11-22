﻿using System;
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
        static bool losePackets = true;
        static TimeSpan timeSpanSecond = new TimeSpan(0, 0, 1);
        static TimeSpan timeSpanSeconds = new TimeSpan(0, 0, 2);
        static TimeSpan timespanMs = new TimeSpan(0, 0, 0, 0, 1);

        static void Main(string[] args)
        {
            PrepareFrames();
            initStuff();

            // send first only
            var data = framesTotal[i].GetAsBytes();
            sendClient.Send(data, data.Length, remoteEP);
            Console.WriteLine($"Sent packet: {framesTotal[i].ToString()}");
            i++;
            Thread.Sleep(timeSpanSeconds);

            while (true)
            {
                try
                {
                    if (sendClient.Available > 0) // Only read if we have some data 
                    {                           // queued in the network buffer. 
                        var frameRecieved = sendClient.Receive(ref localEP).GetFrame();

                        // sequence correction
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

                        SendOne(i);
                    }
                    else
                    {
                        SendOne(i);
                        Thread.Sleep(Lib.Lib.GetTimeSpanMs(25 * WINDOW_SIZE));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.InnerException?.Message ?? ex.Message);
                }

                // old logic
                if (false)
                    Thread.Sleep(timeSpanSeconds);

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
                    Thread.Sleep(timeSpanSeconds);
                var random = Lib.Lib.GenerateRandom(GetRandom());

                // if we fail random
                if (random < numberUser)
                {
                    var message = $"Lost packet: {framesTotal[i].ToString()} to {framesTotal[i].ToString()}";
                    Console.WriteLine(message);
                    result = false;
                    Thread.Sleep(Lib.Lib.GetTimeSpanMs(25 * WINDOW_SIZE));
                }
                else
                {
                    var data = framesTotal[i].GetAsBytes();
                    sendClient.Send(data, data.Length, remoteEP);
                    Console.WriteLine($"Sent packet: {data.GetFrame().ToString()}");
                    result = true;                  
                    Thread.Sleep(Lib.Lib.GetTimeSpanMs(25 * WINDOW_SIZE));
                }

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


        public static void PrepareFrames()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            while (dir.Name != appName)
            {
                dir = Directory.GetParent(dir.FullName);
            }
            var paths = new string[] { dir.ToString(), "COSC635_P2_short.txt" };
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
