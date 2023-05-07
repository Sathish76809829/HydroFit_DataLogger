using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElpisOpcServer.SunPowerGen
{
    public class Helper
    {
        public static TcpClient CreateTcpClient(string deviceIP = "127.0.0.1", ushort port = 550)
        {
            try
            {
                string fileLocation = string.Format("{0}//DeviceInfo.txt", Directory.GetCurrentDirectory());
                string[] fileContent= File.ReadAllLines(fileLocation);
                if(fileContent.Count()==2)
                {
                    deviceIP = fileContent[0].Split(':')[1];
                    port = ushort.Parse(fileContent[1].Split(':')[1].ToString());
                }
                TcpClient client = new TcpClient();
                client.Connect(deviceIP, port);
                return client;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }
    }
}
