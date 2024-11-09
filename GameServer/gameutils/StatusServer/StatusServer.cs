using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DOL.GS
{
    public class StatusServer
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        //static string ip = "127.0.0.1"; //localhost as default ip to prevent errors

        /*
        
        public event CancelEventHandler closeEvent = delegate { };

        public void MakeSomethingThatRaisesEvent()
        {
            CancelEventArgs cea = new CancelEventArgs();
            closeEvent(this, cea);


            if (cea.Cancel)
            {
                if (_clientSocket != null && _clientSocket.Connected)
                {
                    //_clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Close();
                }
            }
            else
            {
                // Do something else
            }
        }
     
        public static string DoGetHostEntry(string hostname)
        {
            IPHostEntry host = Dns.GetHostEntry(hostname);

            Console.WriteLine($"GetHostEntry({hostname}) returns:");

            foreach (IPAddress address in host.AddressList)
            {
                if (address != null)
                    return address.ToString();
            }
            return null;
        }

        public static void Send(string msg)
        {
            // ip = DoGetHostEntry("brotherlandii.ddns.net");
            log.Error("sende nachricht");

            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(ip), 100);//DNS IP und Port
            //IPEndPoint(IPAddress.Parse(ip)
            NetworkStream nwStream = client.GetStream();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(msg);

            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            nwStream.Flush();
            client.Close();
            log.ErrorFormat("message gesendet an ip: {0}", ip);
        }

        /*
        public void Send(string message)
        {
            try
            {


                //data send on udp port 1366 server
                UdpClient client = new UdpClient();
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 1366);
                byte[] bytes = Encoding.ASCII.GetBytes(message);
                client.Send(bytes, bytes.Length, ip);
                client.Close();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UDP send Error {0}", ex.Message.ToString());
                
            }
        }
       */
        public static void GetServerInfo(string info)
        {
           
            try
            {
               // StatusServer udp = new StatusServer();

               
                  
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UDP Get info Error {0}", ex.Message.ToString());
            }
        }
        public static void StartServer()
        {
           /// StatusServer udp = new StatusServer();

            try
            {
                StatusServer.GetServerInfo("The server is back online. Waiting for infos.. Receiving status infos..");
            }
            catch (Exception ex)
            {
               log.ErrorFormat("UDP Error {0}", ex.Message.ToString());
            }
        }

       



    }
}
