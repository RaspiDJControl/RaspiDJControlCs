using System;

namespace RaspiDJControl
{
    class MidiLogger
    {
        public void LogMidiMessage(MidiMessage message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.Write(" {0}: {1} ", message.Input ? "Input " : "Output", BitConverter.ToString(message));

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;

            switch (((byte[])message)[0])
            {
                case 0x80:
                    Console.Write("  NOTE OFF");
                    break;
                case 0x90:
                    Console.Write("  Note ON ");
                    break;
                case 0xB0:
                    Console.Write("  CC MSG  ");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  NOTIMPL ");
                    break;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Note/CC: 0x{0:X02} → 0x{1:X02}", ((byte[])message)[1], ((byte[])message)[2]);
        }
    }
}
