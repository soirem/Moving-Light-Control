using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DMXControll;
using System.Threading;

namespace MovingLightControl
{
    class Program
    {
        static void Main(string[] args)
        {
            OpenDMX.initOpenDMX();
            OpenDMX.start();

            UDPListener listener = new UDPListener(1234);
            Thread udpListenerThread = new Thread(new ThreadStart(listener.startListening));
            udpListenerThread.Start();

            while (!Console.KeyAvailable)
            { 
                
            }
        }
    }
}
