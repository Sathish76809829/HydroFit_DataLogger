#region Namespaces

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Windows;

#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{

    //TODO: --Done Comments for each member 
    //TODO: --Done Read Config File once  

    #region EndPoint Class
    /// <summary>
    /// This Class provides the Endpoint of server with Endpoint Encryption modes
    /// </summary>
    public class EndPoint
    {
        public string endPointName { get; set; }
        public bool chckNone { get; set; }
        public bool chck128Sign { get; set; }
        public bool chck128SignEncrypt { get; set; }
        public bool chck256Sign { get; set; }
        public bool chck256SignEncrypt { get; set; }

        ObservableCollection<Security> listOfSecurity { get; set; }
    }
    #endregion EndPoint Class

    #region Security Class
    /// <summary>
    /// This Class provides the security levels of Server
    /// </summary>
    public class Security
    {
        public string securityLevel { get; set; }
        public string securityType { get; set; }
    }
    #endregion EndPoint Class

    #region Authentication Class
    /// <summary>
    /// This Class provides the Authentication mode of Server 
    /// </summary>
    public class Authentication
    {
        public string anonymousUser { get; set; }
        public string authorisedUser { get; set; }
    }
    #endregion Authentication Class

    #region DiscoveryRegistration Class
    /// <summary>
    /// This Class provides Server Discovery registration parameters
    /// </summary>
    public class DiscoveryRegistration
    {
        public string discoveryInterval { get; set; }
        public string discoveryURL { get; set; }
    }
    #endregion DiscoveryRegistration Class

    #region SLIKDACINFIG Class
    /// <summary>
    /// This Class load all server elements from the Configuration file.
    /// </summary>
    public class SLIKDAUACONFIG
    {
        static IEnumerable<XElement> ConfigElements = from e in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig") select e;

        #region LoadEndPoint
        /// <summary>
        /// Load all the endpoints from  the Configuration file.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<EndPoint> LoadEndPoint()
        {
            ObservableCollection<EndPoint> listOfEndpoints = new ObservableCollection<EndPoint>();
            // 12 10 2016
            var endPoints = ConfigElements.Elements("UaEndpoint").Elements("Url"); //from e in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("UaEndpoint").Elements("Url") select e;

            //var endPoints = from e in XElement.Load(@"C:\\Program Files (x86)\\Software Toolbox\\SLIK-DA5\Redistributables\\SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("UaEndpoint").Elements("Url") select e;
            foreach (var endpoint in endPoints)
            {
                EndPoint EEndpoint = new EndPoint()
                {
                    endPointName = endpoint.Value
                };

                XmlDocument doc = new XmlDocument();
                doc.Load(@"SLIKDAUAConfig.xml");
                var el = doc.SelectSingleNode("/OpcServerConfig/UaServerConfig/UaEndpoint/Url[text()='" + endpoint.Value + "']");


                listOfEndpoints.Add(EEndpoint);
            }
            return listOfEndpoints;
        }
        #endregion LoadEndPoint

        #region LoadSecurityLevel
        /// <summary>
        /// It load all security levels from the Configuration File.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Security> LoadSecurityLevel() //TODO: Remove this Function Update with  LoadSecurityType(); 
        {
            ObservableCollection<Security> listOfSecurity = new ObservableCollection<Security>();
            var securityDetails = ConfigElements.Elements("UaEndpoint").Elements("SecuritySetting").Elements("MessageSecurityMode");  //from s in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("UaEndpoint").Elements("SecuritySetting").Elements("MessageSecurityMode") select s;
            // var securityDetails = from s in XElement.Load(@"C:\\Program Files (x86)\\Software Toolbox\\SLIK-DA5\Redistributables\\SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("UaEndpoint").Elements("SecuritySetting").Elements("MessageSecurityMode") select s;

            foreach (var securityValue in securityDetails)
            {
                Security security = new Security()
                {

                    securityLevel = securityValue.Value,
                    //security128 = security.Value,
                    //security256 = security.Value
                };

                listOfSecurity.Add(security);

            }

            return listOfSecurity;
        }
        #endregion LoadSecurityLevel

        #region LoadSecurityType
        /// <summary>
        /// It Load all security policies from the Configuration File.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Security> LoadSecurityType() //TODO:  --Done  Add one Global List for.
        {
            ObservableCollection<Security> listOfSecurity = new ObservableCollection<Security>();
            var securityDetails = ConfigElements.Elements("UaEndpoint").Elements("SecuritySetting");  //from s in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("UaEndpoint").Elements("SecuritySetting") select s;
            //var securityDetails = from s in XElement.Load(@"C:\\Program Files (x86)\\Software Toolbox\\SLIK-DA5\Redistributables\\SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("UaEndpoint").Elements("SecuritySetting") select s;

            foreach (var securitynone in securityDetails)
            {
                Security SSecurity = new Security()
                {

                    securityType = securitynone.Value,
                    //security128 = security.Value,
                    //security256 = security.Value
                };

                listOfSecurity.Add(SSecurity);

            }

            return listOfSecurity;
        }
        #endregion LoadSecurityType

        #region LoadAuthentication
        /// <summary>
        /// It Loads the Authentication information from the Configuration File.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Authentication> LoadAuthentication()
        {
            ObservableCollection<Authentication> listOfAuthentication = new ObservableCollection<Authentication>();
            var authenticationDetails = ConfigElements.Elements("UserIdentityTokens"); //from a in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("UserIdentityTokens") select a;

            foreach (var authenticationElement in authenticationDetails)
            {
                Authentication authentication = new Authentication()
                {
                    anonymousUser = authenticationElement.Element("EnableAnonymous").Value,
                    authorisedUser = authenticationElement.Element("EnableUserPw").Value

                };
                listOfAuthentication.Add(authentication);
            }
            return listOfAuthentication;
        }
        #endregion LoadAuthentication

        #region LoadDiscoveryElements
        /// <summary>
        /// It Load all Discovery Elements from the Configuration File.
        /// </summary>
        /// <returns></returns>
        internal static ObservableCollection<DiscoveryRegistration> LoadDiscoveryElements()
        {
            ObservableCollection<DiscoveryRegistration> listOfDiscovery = new ObservableCollection<DiscoveryRegistration>();
            var discoveryInterval = ConfigElements.Elements("DiscoveryRegistration"); //from d in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("DiscoveryRegistration") select d;
            foreach (var discoveryDetail in discoveryInterval)
            {
                DiscoveryRegistration discoveryRegistration = new DiscoveryRegistration()
                {
                    discoveryInterval = discoveryDetail.Element("RegistrationInterval").Value,
                    discoveryURL = discoveryDetail.Element("Url").Value
                };

                listOfDiscovery.Add(discoveryRegistration);
            }
            return listOfDiscovery;

        }
        #endregion LoadDiscoveryElements

        #region GetMaxUpdateRate
        /// <summary>
        /// Get the maximum update from the Configuration File.
        /// </summary>
        /// <returns></returns>
        internal static int GetMaxUpdateRate()
        {
            try
            {
                var updateRate = ConfigElements.Elements("ServerUpdateRate"); //from d in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("ServerUpdateRate") select d;
                if (updateRate != null && updateRate.Count() > 0)
                {
                    foreach (var item in updateRate)
                    {
                        return int.Parse(item.Value);
                    }
                }
                else
                {
                    MessageBox.Show("The attribute \'Server Update Rate\' is not found in SLIKDAUAConfig file.\nThe Default value set to 10 ms.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);

                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"SLIKDAUAConfig.xml");
                    XmlNode parentNode = doc.SelectSingleNode("/OpcServerConfig/UaServerConfig");
                    XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "ServerUpdateRate", null);
                    newNode.InnerText = "10";
                    parentNode.AppendChild(newNode);
                    doc.Save(@"SLIKDAUAConfig.xml");
                    return 10;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Elpis OPC server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return 10;
        }
        #endregion GetMaxUpdateRate

        #region GetDefaultTagScanRate
        /// <summary>
        /// Get Default Tag Scanrate from the Configuration File.
        /// </summary>
        /// <returns></returns>
        public static int GetDefaultTagScanRate()
        {
            try
            {
                var scanRate = ConfigElements.Elements("TagScanRate"); //from d in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("TagScanRate") select d;
                if (scanRate != null && scanRate.Count() > 0)
                {
                    foreach (var item in scanRate)
                    {
                        return int.Parse(item.Value);
                    }
                }
                else
                {
                    MessageBox.Show("The attribute \'Tag ScanRate\' is not found in SLIKDAUAConfig file.\nThe Default value set to 100 ms.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    //add or edit the details
                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"SLIKDAUAConfig.xml");
                    XmlNode parentNode = doc.SelectSingleNode("/OpcServerConfig/UaServerConfig");
                    XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "TagScanRate", null);
                    newNode.InnerText = "100";
                    parentNode.AppendChild(newNode);
                    doc.Save(@"SLIKDAUAConfig.xml");
                    return 100;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Elpis OPC server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return 100;
        }
        #endregion GetDefaultTagScanRate

        #region GetLogFilePath
        /// <summary>
        /// Get log file path from the Configuration File.
        /// </summary>
        /// <returns></returns>
        internal static string GetLogFilePath()
        {
            try
            {
                var logPath = ConfigElements.Elements("LogFilePath");  // from d in XElement.Load(@"SLIKDAUAConfig.xml").Elements("UaServerConfig").Elements("LogFilePath") select d;
                foreach (var path in logPath)
                {
                    return path.Value.ToString();
                }
            }
            catch (Exception)
            {

            }
            return null;
        }
        #endregion GetLogFilePath

    }
    #endregion SLIKDACONFIG Class



}
#endregion OPCEngine namespace
