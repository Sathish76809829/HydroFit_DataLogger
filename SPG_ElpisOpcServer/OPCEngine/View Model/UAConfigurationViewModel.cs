#region Namespaces
using System.IO;
using System.Windows;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Collections.Generic;
using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OPCEngine;

#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    public class UAConfigurationViewModel
    {
        #region PublicField
        public bool ImportNewStore = true;
        public bool isNewStore = false;


        public ObservableCollection<EndPoint> endpointCollection = new ObservableCollection<EndPoint>();
        public ObservableCollection<Security> securityCollection = new ObservableCollection<Security>();
        public ObservableCollection<DiscoveryRegistration> discoveryCollection = new ObservableCollection<DiscoveryRegistration>();
        public ObservableCollection<UserAuthenticationViewModel> UserAuthenticationViewModelCollection { get; set; }

        public FileHandler fileHandler { get; set; }

        public XmlNode newNodeUaEndpoint { get; set; }
        public XmlNode parentNode1 { get; set; }
        public XmlDocument doc = new XmlDocument();
        public string regtext { get; set; }

        #endregion PublicField

        public UAConfigurationViewModel() { }

        public void windowState(string state)
        {
            if (state == "close")//close button
            {
                Application.Current.Shutdown();
            }
            else
            {
                if (state == "minimize")//minimize
                {

                }
                else//back button
                {
                }
            }
        }

        public string[] secgrpOnload()
        {
            //For Security CheckBoxes
            int signCount = 0;
            int signAndEncryptCount = 0;
           
            securityCollection = SLIKDAUACONFIG.LoadSecurityLevel();
            string[] level = new string[securityCollection.Count];
            #region SecurityLevel
            for (int i = 0; i < securityCollection.Count; i++)
            {
                Security security = securityCollection[i] as Security;

                if (security.securityLevel == "None")
                {
                    level[i] = "None";
                   // return level;
                }
                else if (security.securityLevel == "Sign")
                {
                    signCount++;
                    if (signCount == 1)
                    {
                        level[i] = "128Sign";
                      //  return level;
                    }
                    else
                    {
                        level[i] = "128&256Sign";
                        //return level;
                    }
                }
                else if (security.securityLevel == "SignAndEncrypt")
                {
                    signAndEncryptCount++;
                    if (signAndEncryptCount == 1)
                    {
                        level[i] = "128SigndEncrypt";
                       // return level;
                    }
                    else
                    {
                        level[i] = "128&256SigndEncrypt";
                       // return level;
                    }
                }
                //else return null;
            }
            return level;
            #endregion SecurityLevel

            #region SecurityType

            //securityCollection = SLIKDAUACONFIG.LoadSecurityType();

            //for (int j = 0; j < securityCollection.Count; j++)
            //{
            //    Security security = securityCollection[j] as Security;
            //    if (security.securityType.Contains("None"))
            //    {
            //        level = "None";
            //        return level;
            //    }
            //    else if (security.securityType.Contains("128"))
            //    {
            //        level = "128";
            //        return level;
            //    }

            //    else if (security.securityType.Contains("256"))
            //    {
            //        level = "128";
            //        return level;
            //    }
            //    else return null;
            //}
            //return null;

            #endregion SecurityLevel
        }

        public string authenticationOnLoad()
        {
            ObservableCollection<Authentication> authenticationCollection = new ObservableCollection<Authentication>();
            authenticationCollection = SLIKDAUACONFIG.LoadAuthentication();
            string authen;
            for (int i = 0; i < authenticationCollection.Count; i++)
            {
                Authentication authentication = authenticationCollection[i] as Authentication;
                if (authentication.authorisedUser == "true")
                {
                    authen = "authorisedUsertrue";
                    return authen;
                }
                else
                {
                    authen = "authorisedUserfalse";
                    return authen;
                }
            }
            return null;
        }

        public string anonymousOnLoad()
        {
            ObservableCollection<Authentication> authenticationCollection = new ObservableCollection<Authentication>();
            authenticationCollection = SLIKDAUACONFIG.LoadAuthentication();
            string authen;
            for (int i = 0; i < authenticationCollection.Count; i++)
            {
                Authentication anonymous = authenticationCollection[i] as Authentication;
                if (anonymous.anonymousUser == "true")
                {
                    authen = "anonymousUsertrue";
                    return authen;
                }
                else if (anonymous.anonymousUser == "false")
                {
                    authen = "anonymousUserfalse";
                    return authen;
                }
                else if (anonymous.authorisedUser == "true")
                {
                    authen = "authorisedUsertrue";
                    return authen;
                }
                else
                {
                    authen = "authorisedUserfalse";
                    return authen;
                }
            }
            return null;
        }

        public ObservableCollection<EndPoint> LoadEndPoint()
        {
            endpointCollection.Clear();

            string EndPointText;
            XmlDocument doc = new XmlDocument();
            doc.Load(@"SLIKDAUAConfig.xml");
            XmlNodeList UAEndPoint = doc.SelectNodes("/OpcServerConfig/UaServerConfig/UaEndpoint");           
            for (int i = 0; i < UAEndPoint.Count; i++)
            {
                EndPoint endPointURL = new EndPoint();
                if (UAEndPoint[i] != null)
                {
                    int childNodesCount = UAEndPoint[i].ChildNodes.Count;

                    for (int j = 0; j < childNodesCount; j++)
                    {
                        if (j == 1)
                        {
                            var endPointName = UAEndPoint[i].ChildNodes[j].InnerText;
                            endPointURL.endPointName = endPointName;
                        }
                        else if (j > 2)
                        {
                            var SP128 = UAEndPoint[i].ChildNodes[j].ChildNodes[0].InnerText;
                            var SP256 = UAEndPoint[i].ChildNodes[j].ChildNodes[0].InnerText;
                            var MSM = UAEndPoint[i].ChildNodes[j].ChildNodes[1].InnerText;
                            var MSM2 = "";
                            if (UAEndPoint[i].ChildNodes[j].ChildNodes.Count > 2)
                                MSM2 = UAEndPoint[i].ChildNodes[j].ChildNodes[2].InnerText;
                            if (MSM == "None")
                            {
                                endPointURL.chckNone = true;
                                //checkNone.IsChecked = endPointURL.chckNone;
                            }
                            if (MSM == "Sign" && SP128.Contains("128"))
                            {
                                endPointURL.chck128Sign = true;

                            }
                            if (MSM2 == "SignAndEncrypt" && SP128.Contains("128"))
                            {
                                endPointURL.chck128SignEncrypt = true;
                            }
                            if (MSM == "Sign" && SP256.Contains("256"))
                            {
                                endPointURL.chck256Sign = true;

                            }
                            if (MSM2 == "SignAndEncrypt" && SP256.Contains("256"))
                            {
                                endPointURL.chck256SignEncrypt = true;

                            }
                        }
                    }



                    #region Previous code

                    //#region Name
                    //if (UAEndPoint[i].ChildNodes.Count > 1)
                    //{
                    //    //var endPointName = UAEndPoint[i].ChildNodes[1].InnerText;
                    //    //endPointURL.endPointName = endPointName;
                    //}
                    //#endregion name

                    //#region None
                    //if (UAEndPoint[i].ChildNodes[2].ChildNodes.Count > 1)
                    //{
                    //    var None = UAEndPoint[i].ChildNodes[2].ChildNodes[1].InnerText;
                    //    if (None == "None")
                    //    {
                    //        endPointURL.chckNone = true;
                    //        //checkNone.IsChecked = endPointURL.chckNone;
                    //    }// var C128w = UAEndPoint[endPointList.SelectedIndex].ChildNodes[4].ChildNodes[0].InnerText;

                    //}
                    //else
                    //{
                    //    endPointURL.chckNone = false;
                    //    //checkNone.IsChecked = endPointURL.chckNone;
                    //}
                    //#endregion None

                    //#region 128S

                    //if (UAEndPoint[i].ChildNodes[3].ChildNodes.Count > 1)
                    //{
                    //    var C128SecurityPolicy = UAEndPoint[i].ChildNodes[3].ChildNodes[0].InnerText.Contains("128");
                    //    var C128S = UAEndPoint[i].ChildNodes[3].ChildNodes[1].InnerText;
                    //    if (C128S == "Sign" && C128SecurityPolicy == true)
                    //    {
                    //        endPointURL.chck128Sign = true;
                    //        //check128Sign.IsChecked = endPointURL.chck128Sign;
                    //    }
                    //}
                    //else
                    //{
                    //    endPointURL.chck128Sign = false;
                    //    //check128Sign.IsChecked = endPointURL.chckNone;
                    //}
                    //#endregion 128S

                    //#region 128SE

                    //if (UAEndPoint[i].ChildNodes[3].ChildNodes.Count > 2)
                    //{
                    //    var C128SecurityPolicy = UAEndPoint[i].ChildNodes[3].ChildNodes[0].InnerText.Contains("128");
                    //    var C128SE = UAEndPoint[i].ChildNodes[3].ChildNodes[2].InnerText;

                    //    if (C128SE == "SignAndEncrypt" && C128SecurityPolicy == true) 
                    //    {
                    //        endPointURL.chck128SignEncrypt = true;
                    //        //check128SigndEncrypt.IsChecked = endPointURL.chck128SignEncrypt;
                    //    }
                    //}
                    //else
                    //{
                    //    endPointURL.chck128SignEncrypt = false;
                    //    //check128SigndEncrypt.IsChecked = endPointURL.chckNone;
                    //}
                    //#endregion 128SE

                    //#region 256S
                    //if (UAEndPoint[i].ChildNodes.Count == 5)
                    //{
                    //    if (UAEndPoint[i].ChildNodes[4].ChildNodes.Count > 1)
                    //    {
                    //        var C256SecurityPolicy = UAEndPoint[i].ChildNodes[3].ChildNodes[1].InnerText.Contains("256");                           
                    //        var C256S = UAEndPoint[i].ChildNodes[4].ChildNodes[1].InnerText;
                    //        if (C256S == "Sign" && C256SecurityPolicy == true) 
                    //        {
                    //            endPointURL.chck256Sign = true;
                    //            //check256Sign.IsChecked = endPointURL.chck256Sign;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        endPointURL.chck256Sign = false;
                    //        //check256Sign.IsChecked = endPointURL.chckNone;
                    //    }
                    //}
                    //else
                    //{
                    //    endPointURL.chck256Sign = false;
                    //    //check256Sign.IsChecked = endPointURL.chckNone;
                    //}
                    //#endregion 256S

                    //#region 256SE
                    //if (UAEndPoint[i].ChildNodes.Count == 5)
                    //{
                    //    if (UAEndPoint[i].ChildNodes[4].ChildNodes.Count > 2)
                    //    {
                    //        var C256SE = UAEndPoint[i].ChildNodes[3].ChildNodes[2].InnerText;
                    //        if (C256SE == "SignAndEncrypt")
                    //        {
                    //            endPointURL.chck256SignEncrypt = true;
                    //            //check256SigndEncrypt.IsChecked = endPointURL.chck256SignEncrypt;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        endPointURL.chck256SignEncrypt = false;
                    //        //check256SigndEncrypt.IsChecked = endPointURL.chckNone;
                    //    }
                    //}
                    //#endregion 256SE

                    #endregion Previous code

                    EndPointText = endPointURL.endPointName;
                    endpointCollection.Add(endPointURL);
                }
            }
            //we need to assign collection for itemsource
            //inside XAML put DisplayMemberPath as that property name(endPointName) in Endpoint Class
            return endpointCollection;
        }

        public ObservableCollection<DiscoveryRegistration> discovery_Onload()
        {

            // discoveryCollection = SLIKDAUACONFIG.LoadDiscoveryInterval();
            discoveryCollection = SLIKDAUACONFIG.LoadDiscoveryElements();

            for (int i = 0; i < discoveryCollection.Count; i++)
            {

                DiscoveryRegistration discoveryRegisterURL = discoveryCollection[i] as DiscoveryRegistration;
                int interval = int.Parse(discoveryRegisterURL.discoveryInterval) / 1000;
                regtext = interval.ToString();

                discoveryRegisterURL.discoveryInterval = interval.ToString();
                //return regtext;
            }
            //discoveryCollection = SLIKDAUACONFIG.LoadDiscoveryURL();

            return discoveryCollection;
        }

        public ObservableCollection<EndPoint> EndpointOnadd(string text,bool? none,bool? sign_128,bool? ensingn_128,bool? sing_256,bool? ensing_256)
        {
            Regex regularExp = new Regex(@"(opc.tcp:\/\/)((\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|\w{1,})\:\d{1,5}");
            regularExp.IsMatch(text);
            if (regularExp.IsMatch(text))
            {               
                EndPoint dr = new EndPoint();
                foreach (var check in endpointCollection)
                {
                    if (check.endPointName == text)
                    {
                        MessageBox.Show("Enter the different endPoint address","Elpis OPC Server",MessageBoxButton.OK,MessageBoxImage.Error);
                        
                        return null;
                    }
                }
                dr.endPointName = text;
                dr.chckNone = (bool)none;
                dr.chck128Sign = (bool)sign_128 ;
                dr.chck128SignEncrypt = (bool)ensingn_128;
                dr.chck256Sign = (bool)sing_256;
                dr.chck256SignEncrypt = (bool)ensing_256;
               
                endpointCollection.Add(dr);
                #region add endpoint in Config file(SLIKDACONGFIG.XML)
                //XmlDocument doc = new XmlDocument();
                //doc.Load(@"C:\Users\harikrishnan.s\Desktop\123.xml");              
                //XmlNode server = doc.SelectSingleNode("OpcServerConfig");
                //XmlNode uaserver = server.SelectSingleNode("UaServerConfig");                     
                //XmlElement serial = doc.CreateElement("SerializerType");
                //serial.InnerText = "Binary";                
                ////uaserver.AppendChild(serial);
                //XmlElement uaConfig = doc.CreateElement("UaEndpoint");
                ////doc.AppendChild(server);
                //uaserver.AppendChild(uaConfig);
                //uaConfig.AppendChild(serial);                
                //XmlElement url = doc.CreateElement("Url");
                //url.InnerText = text;
                //uaConfig.AppendChild(url);
                //doc.Save(@"C:\Users\harikrishnan.s\Desktop\123.xml");
                #endregion
                return endpointCollection;
            }
            else
            {
                MessageBox.Show("Incorrect End Point Format. Enter Valid Format.\nThe Format is :opc.tcp://[MachineName/IP]:[PortNumber]", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                return endpointCollection;
            }       
        }
       

        public ObservableCollection<DiscoveryRegistration> DiscoveryOnadd(string text)
        {
            Regex regularExp = new Regex(@"(opc.tcp:\/\/)((\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|\w{1,})\:\d{1,5}");
            regularExp.IsMatch(text);
            if (regularExp.IsMatch(text))
            {

                DiscoveryRegistration dr = new DiscoveryRegistration();
                foreach (var check in discoveryCollection)
                {
                    if (check.discoveryURL == text)
                    {
                        MessageBox.Show("Enter the different URL for discovery");
                        return null;
                    }
                }
                //XDocument doc = XDocument.Load(@"C:\Users\harikrishnan.s\Desktop");
                //XmlDocument configDoc = new XmlDocument();
                //configDoc.LoadXml(@"C:\Users\harikrishnan.s\Desktop");
               
                dr.discoveryURL = text;
                discoveryCollection.Add(dr);
                return discoveryCollection;
            }
            return discoveryCollection;
        }        

        public ObservableCollection<UserAuthenticationViewModel> UserOnadd(string Name, string paswrd, string cpaswrd)
        {
            if (Name != null)
            {
                if (paswrd == cpaswrd)
                {
                    if (UserAuthenticationViewModelCollection == null)
                        UserAuthenticationViewModelCollection = new ObservableCollection<UserAuthenticationViewModel>();
                    UserAuthenticationViewModel user = new UserAuthenticationViewModel();
                    user.UserName = Name;
                    user.Password = paswrd;
                    user.ConfirmPassWord = cpaswrd;
                    UserAuthenticationViewModelCollection.Add(user);
                    return UserAuthenticationViewModelCollection;
                }
                else
                {
                    MessageBox.Show("Password must be confirmed");
                }
            }
            return null;
        }

        #region OK

        public void LinQDelete()
        {
            XDocument xdoc = XDocument.Load(@"SLIKDAUAConfig.xml");

            xdoc.Descendants("OpcServerConfig").Descendants("UaServerConfig").Descendants("DiscoveryRegistration").Descendants("Url").Remove();

            xdoc.Descendants("OpcServerConfig").Descendants("UaServerConfig").Descendants("UaEndpoint").Remove();

            xdoc.Save(@"SLIKDAUAConfig.xml");
        }

        public void userIdentityTokens(bool Ano, bool Auth)
        {
            //delete the xml nodes completely using LINQ
            LinQDelete();

            doc.Load(@"SLIKDAUAConfig.xml");
            XmlNodeList EnableAnonymous = doc.SelectNodes("/OpcServerConfig/UaServerConfig/UserIdentityTokens/EnableAnonymous");
            XmlNodeList EnableUserPw = doc.SelectNodes("/OpcServerConfig/UaServerConfig/UserIdentityTokens/EnableUserPw");
            foreach (XmlNode xmlNode in EnableAnonymous)
            {
                if (Ano == true)
                {
                    xmlNode.InnerText = "true";
                }
                else
                    xmlNode.InnerText = "false";
            }
            foreach (XmlNode xmlNode in EnableUserPw)
            {
                if (Auth == true)
                {
                    xmlNode.InnerText = "true";
                }
                else
                    xmlNode.InnerText = "false";
            }
        }

        public void DiscoveryRegistration(string intText)
        {
            XmlNodeList RegistrationInterval = doc.SelectNodes("/OpcServerConfig/UaServerConfig/DiscoveryRegistration/RegistrationInterval");
            XmlNodeList Url = doc.SelectNodes("/OpcServerConfig/UaServerConfig/DiscoveryRegistration/Url");
            //for editing the RegistrationInterval
            foreach (XmlNode xmlNode in RegistrationInterval)
            {
                if(intText==""||int.Parse(intText)==0) // if interval is empty or 0 making default to 30
                {
                    int interval = 30;//
                    xmlNode.InnerText = (interval * 1000).ToString();
                    MessageBox.Show("Interval value not be empty or  0.\nThe Interval to be changed to default 30 ms.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    int interval = int.Parse(intText);
                    xmlNode.InnerText = (interval * 1000).ToString();
                }
                
                
            }

            //for edit or add or delete the Url's
            //delete old details 

            XmlNodeList nodeURL = doc.SelectNodes("/OpcServerConfig/UaServerConfig/DiscoveryRegistration/Url");
            for (int i = nodeURL.Count - 1; i >= 0; i--)
            {
                nodeURL[i].ParentNode.RemoveChild(nodeURL[i]);
            }

            //add or edit the details
            XmlNode parentNode = doc.SelectSingleNode("/OpcServerConfig/UaServerConfig/DiscoveryRegistration");
            foreach (var s in discoveryCollection)
            {
                XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Url", null);
                newNode.InnerText = s.discoveryURL;
                parentNode.AppendChild(newNode);
            }
        }

        public void uaEndpoint(bool none, bool sign128, bool signEncrpt128, bool sign256, bool signEncrpt256) 
        {

            //deleting the url endpoints
            XmlNodeList UaEndpoint = doc.SelectNodes("/OpcServerConfig/UaServerConfig/UaEndpoint");
            for (int i = UaEndpoint.Count - 1; i >= 0; i--)
            {
                UaEndpoint[i].ParentNode.RemoveChild(UaEndpoint[i]);
            }
            parentNode1 = doc.SelectSingleNode("/OpcServerConfig/UaServerConfig");
            newNodeUaEndpoint = doc.CreateNode(XmlNodeType.Element, "UaEndpoint", null);

            foreach (var endPoint in endpointCollection)
            {

                XmlNode newNodeSerializerType = doc.CreateNode(XmlNodeType.Element, "SerializerType", null);
                newNodeSerializerType.InnerText = "Binary";

                XmlNode newNodeUrl = doc.CreateNode(XmlNodeType.Element, "Url", null);
                newNodeUrl.InnerText = endPoint.endPointName;

                #region 22 02 2017 
                XmlNode newNodeCertificateStore = doc.CreateNode(XmlNodeType.Element, "CertificateStore", null);
                XmlNode newNodeOpenSSLStore = doc.CreateNode(XmlNodeType.Element, "OpenSSLStore", null);



                XmlNode newNodeCertificateTrustListLocation = doc.CreateNode(XmlNodeType.Element, "CertificateTrustListLocation", null);
                newNodeCertificateTrustListLocation.InnerText = "[ApplicationPath]/PKI/CA/certs/";

                XmlNode newNodeCertificateLocation = doc.CreateNode(XmlNodeType.Element, "CertificateLocation", null);
                newNodeCertificateLocation.InnerText = "[ApplicationPath]/PKI/CA/certs/";

                XmlNode newNodeCertificateRevocationListLocation = doc.CreateNode(XmlNodeType.Element, "CertificateRevocationListLocation", null);
                newNodeCertificateRevocationListLocation.InnerText = "[ApplicationPath]/PKI/CA/crl/examplecrl.crl";
                

                XmlNode newNodeServerCertificate = doc.CreateNode(XmlNodeType.Element, "ServerCertificate", null);
                newNodeServerCertificate.InnerText = "[ApplicationPath]/PKI/CA/certs/MyCertificate.der";

                XmlNode newNodeServerPrivateKey = doc.CreateNode(XmlNodeType.Element, "ServerPrivateKey", null);
                newNodeServerPrivateKey.InnerText = "[ApplicationPath]/PKI/CA/private/MyPrivateKey.pem";



                newNodeOpenSSLStore.AppendChild(newNodeCertificateTrustListLocation);
                newNodeOpenSSLStore.AppendChild(newNodeCertificateLocation);
                newNodeOpenSSLStore.AppendChild(newNodeCertificateRevocationListLocation);
                newNodeOpenSSLStore.AppendChild(newNodeServerCertificate);
                newNodeOpenSSLStore.AppendChild(newNodeServerPrivateKey);


                XmlNode newNodeGenerateCertificate = doc.CreateNode(XmlNodeType.Element, "GenerateCertificate", null);
                newNodeGenerateCertificate.InnerText = "true";
                
                XmlNode newNodeCertificateSettings = doc.CreateNode(XmlNodeType.Element, "CertificateSettings", null);

                XmlNode newNodeCommonName = doc.CreateNode(XmlNodeType.Element, "CommonName", null);
                newNodeCommonName.InnerText = "[ServerName]";

                XmlNode newNodeOrganization = doc.CreateNode(XmlNodeType.Element, "Organization", null);
                newNodeOrganization.InnerText = "Organization";

                XmlNode newNodeOrganizationUnit = doc.CreateNode(XmlNodeType.Element, "OrganizationUnit", null);
                newNodeOrganizationUnit.InnerText = "Unit";

                XmlNode newNodeLocality = doc.CreateNode(XmlNodeType.Element, "Locality", null);
                newNodeLocality.InnerText = "LocationName";

                XmlNode newNodeState = doc.CreateNode(XmlNodeType.Element, "State", null);
                newNodeState.InnerText = "State";

                XmlNode newNodeCountry = doc.CreateNode(XmlNodeType.Element, "Country", null);
                newNodeCountry.InnerText = "US";


                newNodeCertificateSettings.AppendChild(newNodeCommonName);
                newNodeCertificateSettings.AppendChild(newNodeOrganization);
                newNodeCertificateSettings.AppendChild(newNodeOrganizationUnit);
                newNodeCertificateSettings.AppendChild(newNodeLocality);
                newNodeCertificateSettings.AppendChild(newNodeState);
                newNodeCertificateSettings.AppendChild(newNodeCountry);

                newNodeCertificateStore.AppendChild(newNodeOpenSSLStore);
                newNodeCertificateStore.AppendChild(newNodeGenerateCertificate);
                newNodeCertificateStore.AppendChild(newNodeCertificateSettings);



                newNodeUaEndpoint.AppendChild(newNodeSerializerType);
                newNodeUaEndpoint.AppendChild(newNodeUrl);
                newNodeUaEndpoint.AppendChild(newNodeCertificateStore);

                #endregion 22 02 2017 

                #region none
                if (endPoint.chckNone == true)
                {
                    XmlNode newNodeSecuritySetting1 = doc.CreateNode(XmlNodeType.Element, "SecuritySetting", null);
                    XmlNode newNodeSecurityPolicy = doc.CreateNode(XmlNodeType.Element, "SecurityPolicy", null);
                    newNodeSecurityPolicy.InnerText = "http://opcfoundation.org/UA/SecurityPolicy#None";
                    XmlNode newNodeMessageSecurityMode = doc.CreateNode(XmlNodeType.Element, "MessageSecurityMode", null);
                    newNodeMessageSecurityMode.InnerText = "None";
                    newNodeSecuritySetting1.AppendChild(newNodeSecurityPolicy);
                    newNodeSecuritySetting1.AppendChild(newNodeMessageSecurityMode);
                    newNodeUaEndpoint.AppendChild(newNodeSecuritySetting1);

                }
                #endregion none

                #region 128
                XmlNode newNodeSecuritySetting2 = doc.CreateNode(XmlNodeType.Element, "SecuritySetting", null);
                XmlNode newNodeSecurityPolicy2 = doc.CreateNode(XmlNodeType.Element, "SecurityPolicy", null);
                XmlNode newNodeMessageSecurityMode2 = doc.CreateNode(XmlNodeType.Element, "MessageSecurityMode", null);
                XmlNode newNodeMessageSecurityMode22 = doc.CreateNode(XmlNodeType.Element, "MessageSecurityMode", null);

                if (sign128 == true)
                {
                    newNodeSecurityPolicy2.InnerText = "http://opcfoundation.org/UA/SecurityPolicy#Basic128Rsa15";

                    newNodeMessageSecurityMode2.InnerText = "Sign";

                    newNodeSecuritySetting2.AppendChild(newNodeSecurityPolicy2);
                    newNodeSecuritySetting2.AppendChild(newNodeMessageSecurityMode2);

                    newNodeUaEndpoint.AppendChild(newNodeSecuritySetting2);
                }
                if (signEncrpt128 == true)
                {
                    if (endPoint.chck128Sign != true)
                    {
                        newNodeSecurityPolicy2.InnerText = "http://opcfoundation.org/UA/SecurityPolicy#Basic128Rsa15";
                        newNodeSecuritySetting2.AppendChild(newNodeSecurityPolicy2);
                    }
                    newNodeMessageSecurityMode22.InnerText = "SignAndEncrypt";

                    newNodeSecuritySetting2.AppendChild(newNodeMessageSecurityMode22);

                    newNodeUaEndpoint.AppendChild(newNodeSecuritySetting2);
                }




                #endregion 128

                #region 256
                XmlNode newNodeSecuritySetting3 = doc.CreateNode(XmlNodeType.Element, "SecuritySetting", null);
                XmlNode newNodeSecurityPolicy3 = doc.CreateNode(XmlNodeType.Element, "SecurityPolicy", null);
                XmlNode newNodeMessageSecurityMode3 = doc.CreateNode(XmlNodeType.Element, "MessageSecurityMode", null);
                XmlNode newNodeMessageSecurityMode33 = doc.CreateNode(XmlNodeType.Element, "MessageSecurityMode", null);
                if (sign256 == true)
                {
                    newNodeSecurityPolicy3.InnerText = "http://opcfoundation.org/UA/SecurityPolicy#Basic256";
                    newNodeMessageSecurityMode3.InnerText = "Sign";

                    newNodeSecuritySetting3.AppendChild(newNodeSecurityPolicy3);
                    newNodeSecuritySetting3.AppendChild(newNodeMessageSecurityMode3);

                    newNodeUaEndpoint.AppendChild(newNodeSecuritySetting3);

                }
                if (signEncrpt256 == true)
                {
                    if (endPoint.chck256Sign != true)
                    {
                        newNodeSecurityPolicy3.InnerText = "http://opcfoundation.org/UA/SecurityPolicy#Basic256";
                        newNodeSecuritySetting3.AppendChild(newNodeSecurityPolicy3);
                    }
                    newNodeMessageSecurityMode33.InnerText = "SignAndEncrypt";
                    newNodeSecuritySetting3.AppendChild(newNodeMessageSecurityMode33);

                    newNodeUaEndpoint.AppendChild(newNodeSecuritySetting3);
                }
                #endregion 256



                parentNode1.AppendChild(newNodeUaEndpoint);
            }
            //MessageBox.Show("Your settings are saved in to the config file", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            doc.Save(@"SLIKDAUAConfig.xml");
        }

        public void UserAuthenticationSaveToFile()
        {
            fileHandler = new FileHandler();
            fileHandler.UserCollectionFileHandling = UserAuthenticationViewModelCollection;
            if (UserAuthenticationViewModelCollection != null)
            {

                Stream stream = File.Open("auth.elp", FileMode.OpenOrCreate);
                BinaryFormatter bformatter = new BinaryFormatter();
                try
                {
                    using (StreamWriter wr = new StreamWriter(stream))
                    {
                        bformatter.Serialize(stream, fileHandler);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //stream.Close();
                stream.Close();
            }
        }

        #endregion OK

        public void UserAuthenticationRetrieveFromFile()
        {
            //07 11 2016
            if (File.Exists("auth.elp"))
            {
                Stream stream = File.Open("auth.elp", FileMode.OpenOrCreate);
                BinaryFormatter bformatter = new BinaryFormatter();

                using (StreamReader wr = new StreamReader(stream))
                {
                    fileHandler = (FileHandler)bformatter.Deserialize(stream);
                }
                stream.Close();
                UserAuthenticationViewModelCollection = fileHandler.UserCollectionFileHandling;

            }
        }

        public void AddUser(UserAuthenticationViewModel userAuthViewModel)
        {
            if (userAuthViewModel.UserName != "")
            {
                if (UserAuthenticationViewModelCollection == null)
                    UserAuthenticationViewModelCollection = new ObservableCollection<UserAuthenticationViewModel>();

                UserAuthenticationViewModelCollection.Add(userAuthViewModel);

            }
        }

    }
}
#endregion Namespaces
