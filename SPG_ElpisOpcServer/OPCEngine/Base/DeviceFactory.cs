#region Usings
using OPCEngine.Connectors.Allen_Bradley;
using System;
using System.Collections.ObjectModel;
using System.Linq;
#endregion Usings

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    
    /// <summary>
    /// This class is factory for Device Objects.
    /// </summary>
    public class DeviceFactory
    {
        public static DeviceBase GetDeviceObj(ConnectorType ProtocolType) //TODO: --Done  Change name to GetDevice(--) with overloading, Remove duplicate code 
        {
            switch (ProtocolType)
            {
                case ConnectorType.ModbusEthernet:
                    return new  ModbusEthernetDevice();

                case ConnectorType.ModbusSerial:
                    return new ModbusSerialDevice();

                case ConnectorType.ABControlLogix:
                    return new ABControlLogicDevice();

                case ConnectorType.ABMicroLogixEthernet:
                    return new ABMicrologixEthernetDevice();
                case ConnectorType.TcpSocket:
                    return new TcpSocketDevice();
            }

            return null;
        }

        //public static IDevice GetDeviceObj(Object Device)
        //{
        //    dynamic deviceObject = Device as DeviceBase;
        //    DeviceType name = deviceObject.DeviceType;

        //    switch (name)
        //    {
        //        case DeviceType.ModbusEthernet :
        //            return Device as  ModbusEthernetDevice;
        //        case  DeviceType.ABControlLogic :
        //            return Device as ABControlLogicDevice;
        //    }

        //    return null;
        //}
        
        public static dynamic GetDevice(Object device)
        {
            if (device != null)
            {
                dynamic Object = device as DeviceBase;
                DeviceType name = Object.DeviceType;

                switch (name)
                {
                    case DeviceType.ModbusEthernet:
                        return device as ModbusEthernetDevice;
                    case DeviceType.ABControlLogix:
                        return device as ABControlLogicDevice;
                    case DeviceType.ModbusSerial:
                        return device as ModbusSerialDevice;
                    case DeviceType.ABMicroLogixEthernet:
                        return device as ABMicrologixEthernetDevice;
                    case DeviceType.TcpSocketDevice:
                        return device as TcpSocketDevice;

                }
            }
            return null;
        }

        public static DeviceBase GetDevice(string path, ObservableCollection<DeviceBase> deviceCollection)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (!string.IsNullOrEmpty(path) && deviceCollection!=null)
            {
                string[] elements = path.Split('.');
                DeviceBase device = deviceCollection.FirstOrDefault(c => c.Name == elements[2]);
                dynamic deviceObj = DeviceFactory.GetDevice(device);
                return deviceObj;

                //if (deviceObj.TagsCollection == null)
                //{
                //    deviceObj.TagsCollection = new ObservableCollection<Tag>();
                //}
                //ObservableCollection<Tag> tags = deviceObj.TagsCollection;
                //Tag tag = tags.FirstOrDefault(c => c.TagName == elements[elements.Length - 1]);
                //if (path.Contains(tag.TagName) && path.Contains(deviceObj.DeviceName) && path.Contains(deviceObj.ConnectorAssignment))
                //{
                //    return deviceObj;
                //}
                #region old code
                //    for (int i = 0; i < deviceCollection.Count; i++)
                //    {
                //        IDevice device = deviceCollection[i] as IDevice;
                //        dynamic deviceObj = DeviceFactory.GetDeviceObjbyDevice(device);

                //        if (deviceObj.TagsCollection == null)
                //        {
                //            deviceObj.TagsCollection = new ObservableCollection<Tag>();
                //        }
                //        ObservableCollection<Tag> tags = deviceObj.TagsCollection;                   
                //        foreach (Tag tag in tags)
                //        {
                //            //TODO: Why the User is hard coded
                //            //string name = "User." + tag.TagName;
                //            if(path.Contains(tag.TagName) && path.Contains(deviceObj.DeviceName) && path.Contains(deviceObj.ProtocolAssignment)) 
                //            {
                //                return deviceObj;
                //            }
                //        }
                //    }
                #endregion old code
            }

            return null;
        }

        public static DeviceBase GetDeviceByName(string deviceName, ObservableCollection<DeviceBase> deviceCollection)
        {

            if (string.IsNullOrEmpty(deviceName))
                return null;

            DeviceBase device = deviceCollection.FirstOrDefault(c => c.Name == deviceName);
            return device;           
        }
    }
}

#endregion OPCEngine namespace