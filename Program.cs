using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace FreezeLagCMD
{
    class Program
    {
        // WinDivert DLL importları
        [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WinDivertOpen(string filter, int layer, short priority, ulong flags);

        [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool WinDivertRecv(IntPtr handle, byte[] packet, uint packetLen, ref uint readLen, ref WINDIVERT_ADDRESS addr);

        [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool WinDivertClose(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDIVERT_ADDRESS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Reserved;
        }

        // Hotkey register
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static bool freezeActive = false;
        private static IntPtr handle;
        private static Thread freezeThread;
        private static CancellationTokenSource cts;

        const int HOTKEY_ID_F5 = 1;
        const int HOTKEY_ID_ESC = 2;
         private static api KeyAuthApp = new api(
         name: "",
         ownerid: "",
         secret: "",
         version: ""
         );

        static void Main()
        {
            Console.Title = "YOUR NAME";

            KeyAuthApp.init();

            Console.WriteLine("==Login==");
            Console.Write("Username: ");
            string user = Console.ReadLine();
            Console.Write("Password: ");
            string pass = Console.ReadLine();

            KeyAuthApp.login(user, pass);
            if (!KeyAuthApp.response.success)
            {
                Console.WriteLine("Login Failed: " + KeyAuthApp.response.message);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Welcome to Magıc Cheats Fake Lag Panel.");
            Console.WriteLine("Developing by luciviq3439 ...");
            Console.WriteLine("F5 = ON/OFF | ESC = CLOSE");

            // Register hotkeys
            RegisterHotKey(IntPtr.Zero, HOTKEY_ID_F5, 0, (uint)ConsoleKey.F5);       // F tuşu
            RegisterHotKey(IntPtr.Zero, HOTKEY_ID_ESC, 0, (uint)ConsoleKey.Escape); // ESC tuşu

            // Hotkey mesajlarını dinle
            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                if (msg.message == WM_HOTKEY)
                {
                    int id = msg.wParam.ToInt32();
                    if (id == HOTKEY_ID_F5)
                    {
                        if (!freezeActive)
                        {
                            StartFreeze();
                            Console.WriteLine("FreezeLag ON.");
                            freezeActive = true;
                        }
                        else
                        {
                            StopFreeze();
                            Console.WriteLine("FreezeLag OFF.");
                            freezeActive = false;
                        }
                    }
                    else if (id == HOTKEY_ID_ESC)
                    {
                        Console.WriteLine("Closing ...");
                        if (freezeActive) StopFreeze();
                        break;
                    }
                }
            }

            // Temizlik
            UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_F5);
            UnregisterHotKey(IntPtr.Zero, HOTKEY_ID_ESC);
        }

        static void StartFreeze()
        {
            cts = new CancellationTokenSource();
            freezeThread = new Thread(() => FreezeLoop(cts.Token));
            freezeThread.IsBackground = true;
            freezeThread.Start();
        }

        static void StopFreeze()
        {
            if (cts != null)
                cts.Cancel();

            if (handle != IntPtr.Zero)
            {
                WinDivertClose(handle);
                handle = IntPtr.Zero;
            }
        }

        static void FreezeLoop(CancellationToken token)
        {
            handle = WinDivertOpen("inbound and udp.PayloadLength >= 48", 0, 0, 0);
            if (handle == IntPtr.Zero)
            {
                Console.WriteLine("WinDivert açılamadı!");
                return;
            }

            byte[] packet = new byte[65535];
            uint readLen = 0;
            WINDIVERT_ADDRESS addr = new WINDIVERT_ADDRESS { Reserved = new byte[16] };

            while (!token.IsCancellationRequested)
            {
                WinDivertRecv(handle, packet, (uint)packet.Length, ref readLen, ref addr);
                // Paketler burada tutuluyor → lag etkisi
            }
        }

        // WinAPI mesaj yapısı
        private const int WM_HOTKEY = 0x0312;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int pt_x;
            public int pt_y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
    }
}
