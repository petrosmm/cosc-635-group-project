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

namespace Client
{
    internal class Program
    {
        static readonly UdpClient sendClient = new UdpClient();
        static int localPort = 49999;
        static int remotePort = 3000;
        static IPEndPoint localEP = new IPEndPoint(IPAddress.Any, localPort);
        static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), remotePort);

        

        private static void DataReceived(IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);
            Console.WriteLine(receivedBytes.GetFrame());

            c.BeginReceive(DataReceived, ar.AsyncState);
        }

        private static void initStuff()
        {

            sendClient.ExclusiveAddressUse = false;
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sendClient.Client.Bind(localEP);
            sendClient.BeginReceive(DataReceived, sendClient);
        }

        static void Main(string[] args)
        {
            initStuff();
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(new TimeSpan(0, 0, 1));
                var frameBody = new Frame()
                {
                    Type = Lib.Type.send,
                    Body = "client request" + i,
                };
                var data = frameBody.GetAsBytes();
                sendClient.Send(data, data.Length, remoteEP);
            }
            var b = false;
        }
    }
}
