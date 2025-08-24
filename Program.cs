using System;
using System.Runtime.InteropServices;
using System.Threading;
using YourNamespace;

namespace FreezeLagCMD
{
    internal class Program
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
        [DllImport("user32.dll")]

        static extern short GetAsyncKeyState(int vKey);

        private static bool freezeEnabled = false;
        private static bool ghostEnabled = false;
        private static bool running = true;

        // KeyAuth bilgileri
        private static api KeyAuthApp = new api(
            name: "",
            ownerid: "",
            secret: "",
            version: ""
        );

        static void Main(string[] args)
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

            Console.WriteLine("✅ Login Successful!\n");

            // Kullanıcıdan tuş seçimi
            Console.WriteLine("Select FreezeLag Key:");
            ConsoleKey freezeKey = Console.ReadKey(true).Key;
            Console.WriteLine($"FreezeLag Key: {freezeKey}\n");

            Console.WriteLine("Select GhostHack Key:");
            ConsoleKey ghostKey = Console.ReadKey(true).Key;
            Console.WriteLine($"GhostHack Key: {ghostKey}\n");

            // Threadleri başlat
            Thread freezeThread = new Thread(() => FeatureLoop(freezeKey, ref freezeEnabled, LagManager.StartFreeze, LagManager.StopFreeze, "FreezeLag"));
            Thread ghostThread = new Thread(() => FeatureLoop(ghostKey, ref ghostEnabled, LagManager.StartGhostLag, LagManager.StopGhostLag, "GhostHack"));

            freezeThread.IsBackground = true;
            ghostThread.IsBackground = true;

            freezeThread.Start();
            ghostThread.Start();

            // Ana döngü: ESC ile çıkış
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    running = false;
                    Console.WriteLine("Exiting...");
                    Environment.Exit(0);
                }

                Thread.Sleep(50);
            }
        }

        // Genel toggle fonksiyonu
        private static void FeatureLoop(ConsoleKey key, ref bool featureEnabled, Action startAction, Action stopAction, string featureName)
        {
            Console.WriteLine($"{featureName} Ready...");

            // Thread başlarken tuşun mevcut durumunu kaydet
            bool lastKeyState = (GetAsyncKeyState((int)key) & 0x8000) != 0;

            while (running)
            {
                bool isPressed = (GetAsyncKeyState((int)key) & 0x8000) != 0;

                if (isPressed && !lastKeyState) // Tuşa yeni basıldıysa toggle
                {
                    featureEnabled = !featureEnabled;
                    if (featureEnabled)
                    {
                        startAction();
                        Console.WriteLine($"{featureName} ON.");
                    }
                    else
                    {
                        stopAction();
                        Console.WriteLine($"{featureName} OFF.");
                    }
                }

                lastKeyState = isPressed;
                Thread.Sleep(50);
            }
        }
    }
}
