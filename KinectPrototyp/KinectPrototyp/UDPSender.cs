using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace KinectPrototyp
{
    class UDPSender
    {
        #region Member Variables
        private Socket _Sender;
        private IPAddress clientIp = IPAddress.Parse("127.0.0.01");
        private IPEndPoint client;
        #endregion

        #region Constructor
        public UDPSender()
        {
            //neues Socket
            _Sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //Endpunkt mit Ip und Port des Clients
            client = new IPEndPoint(clientIp, 1234);
        }
        #endregion Constructor

        #region Methods
        public void sendMessage(string message)
        {
            byte[] sendBuffer = Encoding.ASCII.GetBytes(message);

            try
            {
                //Nachricht an Client senden
                _Sender.SendTo(sendBuffer, client);
                Console.WriteLine("Sending {0} to IP:{1}, port:{2}", message, client.Address, client.Port);

            }
            catch (Exception e)
            {
                Console.WriteLine("FAIL! Error while sending Message.");
            }

        }
        #endregion Methods

    }
}

