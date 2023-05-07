#region Namespaces
using NDI.SLIKDA.Interop;
using System;

using System.Windows;
#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    #region PropertyInfoViewModel Class
    public static class SLIKDAHelper
    {
        private static SLIKServer slikdaObject;
        public static SLIKServer SlikdaObject
        {
            get
            {
                if (slikdaObject == null)
                {                  
                    try
                    {
                        slikdaObject = new SLIKServer();                       
                        //slikdaObject.CreateControl();

                        slikdaObject.AppID = "{95C7EE0A-59BF-4498-B327-D42D871FEF3B}";
                        slikdaObject.AppName = "C# OPC Server";
                        slikdaObject.CLSID = "{E0B81EE8-11B6-40EC-9274-3B986CEF9951}";
                        slikdaObject.COMCallTracingEnabled =false;
                        slikdaObject.ProgID = "Elpis.OPCServer.1";
                        slikdaObject.MaxUpdateRate = SLIKDAUACONFIG.GetMaxUpdateRate();  //100;  //TODO:Done Need to fetch the value from config file
                        int maxUpdateRate= SLIKDAUACONFIG.GetMaxUpdateRate();                    

                        //slikdaObject.COMCallTracingEnabled = true;

                    }
                    catch (Exception e)
                    {

                        MessageBox.Show(e.Message);
                    }
                }
                return slikdaObject;
            }
        }
    } 
    #endregion PropertyInfoViewModel Class
}
#endregion OPCEngine namespace