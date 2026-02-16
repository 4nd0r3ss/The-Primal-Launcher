using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using PrimalLauncher;

namespace PrimalLauncher
{
    public static class Launcher
    {
        [DllImport("kernel32.dll")]
        static extern ulong GetTickCount64();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation
         );

        [DllImport("kernel32.dll")]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// This function is a C++-to-C# translated version of the original Seventh Umbral.
        /// I launches the game directly from command line skippping the updater and web login windows.
        /// </summary>
        /// <param name="sessionId"></param>
        public static void Launch(string sessionId)
        {
            ulong tick = GetTickCount64();
            string installPath = Preferences.GetGameInstallPath();

            string commandLine =
                $" T ={tick} /LANG =en-us /REGION =2 /SERVER_UTC =1356916742 /SESSION_ID ={sessionId}";

            // encryption key (8 hex chars)
            uint keyVal = (uint)(tick & ~0xFFFFUL);
            string encryptionKey = keyVal.ToString("x8");

            // create a Blowfish instance using the encryption key bytes
            var blowfish = new Blowfish(Encoding.ASCII.GetBytes(encryptionKey));

            byte[] commandBytes = Encoding.ASCII.GetBytes(commandLine + "\0");

            int commandLineSize = commandBytes.Length;
            int newSize = commandLineSize & ~0x7;

            for (int i = 0; i < newSize; i += 8)
            {
                uint a = BitConverter.ToUInt32(commandBytes, i);
                uint b = BitConverter.ToUInt32(commandBytes, i + 4);

                blowfish.BlowfishEncipher(ref a, ref b);

                Buffer.BlockCopy(BitConverter.GetBytes(a), 0, commandBytes, i, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(b), 0, commandBytes, i + 4, 4);
            }

            string encoded = Convert.ToBase64String(commandBytes, 0, commandLineSize)
                .Replace('+', '-')
                .Replace('/', '_');

            string fullCommandLine =
                $"{installPath}\\ffxivgame.exe sqex0002{encoded}!////";

            var si = new STARTUPINFO();
            si.cb = Marshal.SizeOf<STARTUPINFO>();

            if (!CreateProcess(
                    null,
                    fullCommandLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    ProcessCreationFlags.CREATE_SUSPENDED,
                    IntPtr.Zero,
                    installPath,
                    ref si,
                    out var pi))
            {
                throw new InvalidOperationException("Failed to launch game executable.");
            }

            ResumeThread(pi.hThread);

            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);
        }

        enum ProcessCreationFlags : uint
        {
            CREATE_SUSPENDED = 0x00000004
        }

        [StructLayout(LayoutKind.Sequential)]
        struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }
    }
}
