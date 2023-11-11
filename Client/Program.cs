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
        static List<Frame> framesToSend = new List<Frame>();

        static void Main(string[] args)
        {
            prepareFrames();
            initStuff();
            for (int i = 0; i < framesToSend.Count; i = i + WINDOW_SIZE)
            {
                Thread.Sleep(new TimeSpan(0, 0, 1));
                sendClient.Send(framesToSend[i].GetAsBytes(), framesToSend[i].GetAsBytes().Length, remoteEP);
            }
            var b = false;
        }

        private static void DataReceived(IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var frameRecieved = c.EndReceive(ar, ref receivedIpEndPoint).GetFrame();
            Console.WriteLine(frameRecieved.Body);
            c.BeginReceive(DataReceived, ar.AsyncState);
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
            string target = $@"{dir}\COSC635_P2_DataSent.txt";

            using (var _streamReader = new StreamReader(target))
            {
                var rawData = null as string;
                var i = 1;
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

                    i++;
                }

                Console.WriteLine("finished reading file");
            }
        }
    }
}
