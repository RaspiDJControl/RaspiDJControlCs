using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RaspiDJControl
{
    class MidiSocket
    {
        private Socket socket;
        private readonly IPEndPoint ipEndPoint;
        
        public event EventHandler<MidiMessageReceivedEventArgs> MidiMessageReceived;

        public MidiSocket(string IP, int port)
        {
            socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
                );
            ipEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        }

        public void Connect(bool retry = false)
        {
            try
            {
                if(retry) Console.WriteLine("Retrying connection...");
                else Console.WriteLine("Connecting to {0}... ", ipEndPoint);

                socket.Connect(ipEndPoint);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("→ Successfully connected!\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Message Log:");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("→ ERROR: {0}\n", ex.Message);
                Console.ForegroundColor = ConsoleColor.White;

                Thread.Sleep(3000);
                Connect(true);
            }
        }

        public void BeginReceive()
        {
            new Thread(ReceiveThread).Start();
        }

        public void SendMessage(byte[] message)
        {
            if (socket.Connected)
            {
                socket.Send(message);
            }
        }

        private void ReceiveThread()
        {
            while (socket.Connected)
            {
                var buffer = new byte[3];
                try
                {
                    socket.Receive(buffer);

                    var message = (MidiMessage) buffer;
                    message.Input = true;

                    if (MidiMessageReceived != null) MidiMessageReceived(this, new MidiMessageReceivedEventArgs { Message = message });
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("→ ERROR: {0}\n", ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Connect(true);
                }
            }
        }

        public class MidiMessageReceivedEventArgs : EventArgs
        {
            public MidiMessage Message { get; set; }
        }
    }
}
