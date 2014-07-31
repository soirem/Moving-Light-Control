using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace DMXControll
{   
    class UDPListener
    {

        private int port = 1234;
        private UdpClient listener;

        private int pan_channel = 0;
        private int tilt_channel = 1;
        private int strobe_channel = 2;
        private int red_channel = 3;
        private int green_channel = 4;
        private int blue_channel = 5;
        private int white_channel = 6;
        private int dimmer_channel = 7;

        public UDPListener(int portNumber) {
            this.port = portNumber;
        }

        public void startListening() 
        {
            listener = new UdpClient(port);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

            string received_data;
            byte[] receive_byte_array;

            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for data...");

                    receive_byte_array = listener.Receive(ref groupEP);
                    Console.WriteLine("Received data from {0}", groupEP.ToString());
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    Console.WriteLine("Message: {0}", received_data);
                    parseMessage(received_data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            
        }

        public void parseMessage(String data)
        {
            if (data.Equals("open",StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(white_channel, (byte)255);
                OpenDMX.setDmxValue(red_channel, (byte)0);
                OpenDMX.setDmxValue(green_channel, (byte)0);
                OpenDMX.setDmxValue(blue_channel, (byte)0);
                OpenDMX.setDmxValue(dimmer_channel, (byte)255);

            }
            else if (data.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(dimmer_channel, (byte)0);
            }
            else if (data.Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(pan_channel, (byte)0);
                OpenDMX.setDmxValue(tilt_channel, (byte)0);
                OpenDMX.setDmxValue(blue_channel, (byte)0);
                OpenDMX.setDmxValue(green_channel, (byte)0);
                OpenDMX.setDmxValue(red_channel, (byte)0);
                OpenDMX.setDmxValue(white_channel, (byte)0);
                OpenDMX.setDmxValue(strobe_channel, (byte)0);
                OpenDMX.setDmxValue(dimmer_channel, (byte)0);
            }
            else if (data.Equals("strobe off", StringComparison.OrdinalIgnoreCase))
            {
                //Wert anpassen 255 oder 0
                OpenDMX.setDmxValue(strobe_channel, (byte)255);
            }
            else if (data.Equals("strobe slow", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(strobe_channel, (byte)50);
            }
            else if (data.Equals("strobe medium", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(strobe_channel, (byte)120);
            }
            else if (data.Equals("strobe fast", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(strobe_channel, (byte)200);
            }
            else if (data.Equals("color white", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(white_channel, (byte)255);
                OpenDMX.setDmxValue(red_channel, (byte)0);
                OpenDMX.setDmxValue(green_channel, (byte)0);
                OpenDMX.setDmxValue(blue_channel, (byte)0);

            }
            else if (data.Equals("color green", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(green_channel, (byte)255);
                OpenDMX.setDmxValue(white_channel, (byte)0);
                OpenDMX.setDmxValue(red_channel, (byte)0);
                OpenDMX.setDmxValue(blue_channel, (byte)0);
            }
            else if (data.Equals("color blue", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(blue_channel, (byte)255);
                OpenDMX.setDmxValue(white_channel, (byte)0);
                OpenDMX.setDmxValue(red_channel, (byte)0);
                OpenDMX.setDmxValue(green_channel, (byte)0);
            }
            else if (data.Equals("color red", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(red_channel, (byte)255);
                OpenDMX.setDmxValue(white_channel, (byte)0);
                OpenDMX.setDmxValue(green_channel, (byte)0);
                OpenDMX.setDmxValue(blue_channel, (byte)0);
            }
            else if (data.Equals("color whitegreen", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(white_channel, (byte)255);
                OpenDMX.setDmxValue(green_channel, (byte)255);
                OpenDMX.setDmxValue(blue_channel, (byte)0);
                OpenDMX.setDmxValue(red_channel, (byte)0);
            }
            else if (data.Equals("color greenblue", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(green_channel, (byte)255);
                OpenDMX.setDmxValue(blue_channel, (byte)255);
                OpenDMX.setDmxValue(white_channel, (byte)0);
                OpenDMX.setDmxValue(red_channel, (byte)0);
            }
            else if (data.Equals("color bluered", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(blue_channel, (byte)255);
                OpenDMX.setDmxValue(red_channel, (byte)255);
                OpenDMX.setDmxValue(white_channel, (byte)0);
                OpenDMX.setDmxValue(green_channel, (byte)0);
            }
            else if (data.Equals("color all", StringComparison.OrdinalIgnoreCase))
            {
                OpenDMX.setDmxValue(white_channel, (byte)255);
                OpenDMX.setDmxValue(green_channel, (byte)255);
                OpenDMX.setDmxValue(blue_channel, (byte)255);
                OpenDMX.setDmxValue(red_channel, (byte)255);
            }
            else if (data.StartsWith("moveh"))
            {
                int val = -1;
                string number = data.Substring(6);
                bool isInt = int.TryParse(number, out val);
                if (val >= 0 && val <= 255)
                {
                    OpenDMX.setDmxValue(pan_channel, (byte)val);
                }
            }
            else if (data.StartsWith("movev"))
            {
                int val = -1;
                string number = data.Substring(6);
                bool isInt = int.TryParse(number, out val);
                if (val >= 0 && val <= 255)
                {
                    OpenDMX.setDmxValue(tilt_channel, (byte)val);
                }
            }
        }

        public void closeListener() 
        {
            listener.Close();
            Console.WriteLine("Listener closed...");
        }
    
    }
}
