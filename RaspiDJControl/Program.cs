using System;
using System.Threading;
using System.Net;

namespace RaspiDJControl
{
    class Program
    {
        private static readonly VirtualMIDI midi = new VirtualMIDI("Virtual DJ Controller");
        private static readonly MidiLogger logger = new MidiLogger();
        private static MidiSocket socket;
        
        static void Main()
        {
            socket = new MidiSocket(Dns.GetHostAddresses("RASPBERRYPI")[0].ToString(), 5008);
            socket.MidiMessageReceived += socket_MidiMessageReceived;
            socket.Connect();
            socket.BeginReceive();

            new Thread(ReadMidiInputThread).Start();

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static void socket_MidiMessageReceived(object sender, MidiSocket.MidiMessageReceivedEventArgs e)
        {
            midi.sendCommand(e.Message);
            logger.LogMidiMessage(e.Message);
        }

        private static void ReadMidiInputThread()
        {
            while (true)
            {
                MidiMessage message = (MidiMessage)midi.getCommand();
                socket.SendMessage(message);
                logger.LogMidiMessage(message);
            }
        }
    }
}