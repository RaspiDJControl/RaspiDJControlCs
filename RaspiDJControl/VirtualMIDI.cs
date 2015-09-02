/* teVirtualMIDI C# interface
 *
 * Copyright 2009-2012, Tobias Erichsen
 * All rights reserved, unauthorized usage & distribution is prohibited.
 *
 *
 * File: VirtualMIDI.cs
 *
 * This file implements the C#-class-wrapper for the teVirtualMIDI-driver.
 * This class encapsualtes the native C-type interface which is integrated
 * in the teVirtualMIDI32.dll and the teVirtualMIDI64.dll.
 */

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace RaspiDJControl
{
    [Serializable]
    public class TeVirtualMIDIException : Exception
    {
        /* defines of specific WIN32-error-codes that the native teVirtualMIDI-driver
         * is using to communicate specific problems to the application */
        private const int ERROR_PATH_NOT_FOUND = 3;
        private const int ERROR_INVALID_HANDLE = 6;
        private const int ERROR_TOO_MANY_CMDS = 56;
        private const int ERROR_TOO_MANY_SESS = 69;
        private const int ERROR_INVALID_NAME = 123;
        private const int ERROR_MOD_NOT_FOUND = 126;
        private const int ERROR_ALREADY_EXISTS = 183;
        private const int ERROR_OLD_WIN_VERSION = 1150;
        private const int ERROR_REVISION_MISMATCH = 1306;
        private const int ERROR_ALIAS_EXISTS = 1379;

        public TeVirtualMIDIException()
        {
        }

        public TeVirtualMIDIException(string message) : base(message)
        {
        }

        public TeVirtualMIDIException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TeVirtualMIDIException(SerializationInfo info, StreamingContext context)
        {
        }

        public int reasonCode
        {
            get { return fReasonCode; }

            set { fReasonCode = value; }
        }

        private int fReasonCode;

        private static string reasonCodeToString(int reasonCode)
        {
            switch (reasonCode)
            {
                case ERROR_OLD_WIN_VERSION:
                    return "Your Windows-version is too old for dynamic MIDI-port creation.";

                case ERROR_INVALID_NAME:
                    return "You need to specify at least 1 character as MIDI-portname!";

                case ERROR_ALREADY_EXISTS:
                    return "The name for the MIDI-port you specified is already in use!";

                case ERROR_ALIAS_EXISTS:
                    return "The name for the MIDI-port you specified is already in use!";

                case ERROR_PATH_NOT_FOUND:
                    return "Possibly the teVirtualMIDI-driver has not been installed!";

                case ERROR_MOD_NOT_FOUND:
                    return "The teVirtualMIDIxx.dll could not be loaded!";

                case ERROR_REVISION_MISMATCH:
                    return "The teVirtualMIDIxx.dll and teVirtualMIDI.sys driver differ in version!";

                case ERROR_TOO_MANY_SESS:
                    return "Maximum number of ports reached";

                case ERROR_INVALID_HANDLE:
                    return "Port not enabled";

                case ERROR_TOO_MANY_CMDS:
                    return "MIDI-command too large";

                default:
                    return "Unspecified virtualMIDI-error: " + reasonCode;
            }
        }

        public static void ThrowExceptionForReasonCode(int reasonCode)
        {
            var exception = new TeVirtualMIDIException(reasonCodeToString(reasonCode))
            {
                reasonCode = reasonCode
            };

            throw exception;
        }
    }

    public class VirtualMIDI
    {
        /* default size of sysex-buffer */
        private const UInt32 TE_VM_DEFAULT_SYSEX_SIZE = 65535;

        /* constant for loading of teVirtualMIDI-interface-DLL, either 32 or 64 bit */
        private const string DllName = "teVirtualMIDI32.dll";
        /* private const string DllName = "teVirtualMIDI64.dll"; */

        /* TE_VM_LOGGING_MISC - log internal stuff (port enable, disable...) */
        public const UInt32 TE_VM_LOGGING_MISC = 1;
        /* TE_VM_LOGGING_RX - log data received from the driver */
        public const UInt32 TE_VM_LOGGING_RX = 2;
        /* TE_VM_LOGGING_TX - log data sent to the driver */
        public const UInt32 TE_VM_LOGGING_TX = 4;

        /* TE_VM_FLAGS_PARSE_RX - parse incoming data into single, valid MIDI-commands */
        public const UInt32 TE_VM_FLAGS_PARSE_RX = 1;


        /* static initializer to retrieve version-info from DLL... */

        static VirtualMIDI()
        {
            //fVersionString = virtualMIDIGetVersion(ref fVersionMajor, ref fVersionMinor, ref fVersionRelease, ref fVersionBuild );
            fVersionString = "versie 1337 enzu";
        }


        public VirtualMIDI(string portName, UInt32 maxSysexLength = TE_VM_DEFAULT_SYSEX_SIZE,
            UInt32 flags = TE_VM_FLAGS_PARSE_RX)
        {
            fInstance = virtualMIDICreatePortEx2(portName, IntPtr.Zero, IntPtr.Zero, maxSysexLength, flags);

            if (fInstance == IntPtr.Zero)
            {
                var lastError = Marshal.GetLastWin32Error();

                TeVirtualMIDIException.ThrowExceptionForReasonCode(lastError);
            }

            fReadBuffer = new byte[maxSysexLength];
            fMaxSysexLength = maxSysexLength;
        }


        ~VirtualMIDI()
        {
            if (fInstance != IntPtr.Zero)
            {
                virtualMIDIClosePort(fInstance);

                fInstance = IntPtr.Zero;
            }
        }


        public static int versionMajor
        {
            get { return fVersionMajor; }
        }


        public static int versionMinor
        {
            get { return fVersionMinor; }
        }


        public static int versionRelease
        {
            get { return fVersionRelease; }
        }


        public static int versionBuild
        {
            get { return fVersionBuild; }
        }


        public static String versionString
        {
            get { return fVersionString; }
        }


        public static UInt32 logging(UInt32 loggingMask)
        {
            return virtualMIDILogging(loggingMask);
        }


        public void shutdown()
        {
            if (!virtualMIDIShutdown(fInstance))
            {
                var lastError = Marshal.GetLastWin32Error();

                TeVirtualMIDIException.ThrowExceptionForReasonCode(lastError);
            }
        }


        public void sendCommand(byte[] command)
        {
            if ((command == null) || (command.Length == 0))
            {
                return;
            }

            if (!virtualMIDISendData(fInstance, command, (UInt32) command.Length))
            {
                var lastError = Marshal.GetLastWin32Error();
                TeVirtualMIDIException.ThrowExceptionForReasonCode(lastError);
            }
        }


        public byte[] getCommand()
        {
            var length = fMaxSysexLength;

            if (!virtualMIDIGetData(fInstance, fReadBuffer, ref length))
            {
                var lastError = Marshal.GetLastWin32Error();
                TeVirtualMIDIException.ThrowExceptionForReasonCode(lastError);
            }

            var outBytes = new byte[length];

            Array.Copy(fReadBuffer, outBytes, length);

            return outBytes;
        }


        private readonly byte[] fReadBuffer;
        private IntPtr fInstance;
        private readonly UInt32 fMaxSysexLength;
        public static ushort fVersionMajor;
        public static ushort fVersionMinor;
        public static ushort fVersionRelease;
        public static ushort fVersionBuild;
        private static readonly String fVersionString;


        [DllImport(DllName, EntryPoint = "virtualMIDICreatePortEx2", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr virtualMIDICreatePortEx2(string portName, IntPtr callback,
            IntPtr dwCallbackInstance, UInt32 maxSysexLength, UInt32 flags);

        [DllImport(DllName, EntryPoint = "virtualMIDIClosePort", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern void virtualMIDIClosePort(IntPtr instance);

        [DllImport(DllName, EntryPoint = "virtualMIDIShutdown", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern Boolean virtualMIDIShutdown(IntPtr instance);

        [DllImport(DllName, EntryPoint = "virtualMIDISendData", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern Boolean virtualMIDISendData(IntPtr midiPort, byte[] midiDataBytes, UInt32 length);

        [DllImport(DllName, EntryPoint = "virtualMIDIGetData", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern Boolean virtualMIDIGetData(IntPtr midiPort, [Out] byte[] midiDataBytes, ref UInt32 length);

        [DllImport(DllName, EntryPoint = "virtualMIDILogging", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern UInt32 virtualMIDILogging(UInt32 loggingMask);
    }
}
