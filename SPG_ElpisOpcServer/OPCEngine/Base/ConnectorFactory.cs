#region Namespaces
using OPCEngine;
using OPCEngine.Connectors.Allen_Bradley;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
#endregion Namespaces

#region OPCEngine namespace

/// <summary>
/// The purpose of this class is to create a Connector Object based on the connector type. 
/// Get the connector object based on the connector name.
/// </summary>
namespace Elpis.Windows.OPC.Server
{
    public static class ConnectorFactory
    {
        public static IConnector GetConnector(string connectorName, ObservableCollection<IConnector> ConnectorCollection) //TODO: --Done Change name to GetConnector(--) with overloading, Remove duplicate code --Done
        {
            if (!string.IsNullOrEmpty(connectorName) && (ConnectorCollection != null))
            {
                string[] elements = connectorName.Split('.');
                IConnector connector = ConnectorCollection.FirstOrDefault(c => c.Name == elements[1]);
                ConnectorBase connectorObject = ConnectorFactory.GetConnector(connector) as ConnectorBase;
                return connectorObject as IConnector;


                #region Using LINQ
                //string[] elements = connectorName.Split('.');
                //IConnector connector = ConnectorCollection.FirstOrDefault(c => c.Name == elements[1]);
                //ConnectorBase connectorObject = ConnectorFactory.GetConnectorObj(connector) as ConnectorBase;
                //if (connectorObject != null)
                //{
                //    IDevice device = (connectorObject.DeviceCollection).FirstOrDefault(d => d.Name == elements[2]);
                //    DeviceBase deviceObj = DeviceFactory.GetDeviceObj(device) as DeviceBase;
                //    if (elements.Count() == 4)
                //    {
                //        Tag tag = deviceObj.TagsCollection.FirstOrDefault(t => t.TagName == elements[3]);
                //        if (tag != null)
                //        {
                //            return connectorObject as IConnector;
                //        }
                //        return null;
                //    }
                //    else
                //    {
                //        Tag tag = deviceObj.TagsCollection.FirstOrDefault(t => t.TagName == elements[4]);
                //        if (tag != null)
                //        {
                //            return connectorObject as IConnector;
                //        }
                //        return null;
                //    }
                //}
                #endregion Using LINQ

                //Comment on 12-Feb-2018 by Hari
                #region comment
                //for (int i = 0; i < ConnectorCollection.Count; i++) 
                //{
                //    IConnector protocol = ConnectorCollection[i] ;
                //    dynamic protocolObj = ConnectorFactory.GetConnectorObj(protocol);
                //    if (protocolObj == null)
                //    {
                //        Debug.Assert(false); 
                //        return null;
                //    }

                //    if (protocolObj.DeviceCollection == null)
                //    {
                //        protocolObj.DeviceCollection = new ObservableCollection<IDevice>();
                //    }
                //    else
                //    {
                //        //TODO: --Done  Use a LINQ Technique                      

                //        for (int j = 0; j < protocolObj.DeviceCollection.Count; j++)
                //        {
                //            //TODO: --Done Remove Device Factory Call use devicebase. 
                //            IDevice device = protocolObj.DeviceCollection[j] as IDevice;
                //            dynamic deviceObj = DeviceFactory.GetDeviceObj(device);

                //            ObservableCollection<Tag> tags = deviceObj.TagsCollection;
                //            if (tags == null)
                //            {
                //                tags = new ObservableCollection<Tag>();
                //            }
                //            //TODO: --Done Use LINQ 
                //            foreach (Tag tag in tags)
                //            {
                //                //string name = "User." +deviceObj.ProtocolAssignment+"."+deviceObj.DeviceName+"."+tag.TagName;
                //                //string[] listNames = name.Split('.');
                //                if (connectorName.Contains(deviceObj.ConnectorAssignment) && connectorName.Contains(deviceObj.DeviceName) && connectorName.Contains(tag.TagName))//(string.Compare(path, name) == 0)//TODO what is path here 
                //                {
                //                    return protocolObj;
                //                }
                //            }
                //        } //End of For loop 
                //    }
                //} //End of For loop 
                #endregion
            } //End of If

            return null;
        }

        public static IConnector GetConnector(ConnectorType connectorType)
        {
            switch (connectorType)
            {
                case ConnectorType.ModbusEthernet:
                    return new ModbusEthernetConnector();
                case ConnectorType.ABControlLogix:
                    return new ABControlLogicConnector();
                case ConnectorType.ModbusSerial:
                    return new ModbusSerialConnector();
                case ConnectorType.ABMicroLogixEthernet:
                    return new ABMicrologixEthernetConnector();
                case ConnectorType.TcpSocket:
                    return new TcpSocketConnector();
            }

            return null;
        }

        /// <summary>
        /// Based on the type of connector it creates connector object and return it.
        /// </summary>
        /// <param name="ProtocolType"></param>
        /// <returns></returns>
        public static IConnector GetConnector(IConnector connectorType)
        {
            if (connectorType != null)
            {
                dynamic ProtocolObject = connectorType as ConnectorBase;
                ConnectorType name = ProtocolObject.TypeofConnector;
                switch (name)
                {
                    // Connector type as Modbus Ethernet
                    case ConnectorType.ModbusEthernet:
                        return connectorType as ModbusEthernetConnector;

                    // Connector type as AB-Control Logicx
                    case ConnectorType.ABControlLogix:
                        return connectorType as ABControlLogicConnector;

                    // Connector type as ModbusSerial
                    case ConnectorType.ModbusSerial:
                        return connectorType as ModbusSerialConnector;
                    case ConnectorType.ABMicroLogixEthernet:
                        return connectorType as ABMicrologixEthernetConnector;
                    case ConnectorType.TcpSocket:
                        return connectorType as TcpSocketConnector;

                    default:
                        return null;
                } //End of Switch
            }

            return null;
        }

        /// <summary>
        /// This method return connector object based on the name of the connector passed by user.
        /// </summary>
        /// <param name="connectorName"></param>
        /// <param name="connectorCollection"></param>
        /// <returns></returns>
        public static IConnector GetConnectorByName(string connectorName, ObservableCollection<IConnector> connectorCollection)
        {
            if (string.IsNullOrEmpty(connectorName))
                return null;

            IConnector protocolConn = connectorCollection.FirstOrDefault(c => c.Name == connectorName);
            return protocolConn;
        }
    }
} //End of Namespace Elpis.Windows.OPC.Server

#endregion OPCEngine namespace