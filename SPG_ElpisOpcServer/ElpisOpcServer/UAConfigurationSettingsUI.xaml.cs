#region Namespaces
using System.IO;
using System.Windows;
using Opc.Ua.CertificateManagementLibrary;
using Opc.Ua.UserComponents;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.Win32;
using Opc.Ua.UserComponents.Tools;
using Opc.Ua;
using System.Collections.Generic;
using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
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
using Elpis.Windows.OPC.Server;

#endregion Namespaces

#region ElpisOpcServer namespace
namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for UAConfiguration.xaml
    /// </summary>
    public partial class UAConfigurationSettingsUI : System.Windows.Controls.UserControl
    {
        #region PublicField
        public UAConfigurationViewModel configE = new UAConfigurationViewModel();

        public bool ImportNewStore = true;
        public bool isNewStore = false;


        public Opc.Ua.CertificateManagementLibrary.IStore Store { get; set; }
        public ObservableCollection<UserAuthenticationViewModel> UserAuthenticationViewModelCollection { get; set; }
        public AboutPageUI about { get; set; }

        #endregion PublicField

        #region Constructor
        public UAConfigurationSettingsUI()
        {
            InitializeComponent();
            Store = OPCEngine.StoreFactory.GetCertificateStore();
            Store.StoreName = "PKI\\CA";
            Store.StorePath = Directory.GetCurrentDirectory();
            //7/9/17
            userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
            userAuthList.DisplayMemberPath = "UserName";
            about = new AboutPageUI();

        }
        #endregion Constructor

        #region UAConfiguration_loaded event
        /// <summary>
        /// Loading UA configuration will initialize the UI from SLIKDAUAConfig.xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UAConfiguration_loaded(object sender, RoutedEventArgs e)
        {

            #region EndPoint
            configE.LoadEndPoint();
            endPointList.ItemsSource = null;
            endPointList.ItemsSource = configE.endpointCollection;
            ObservableCollection<EndPoint> endPointURL = configE.endpointCollection as ObservableCollection<EndPoint>;

            for (int i = 0; i < endPointList.Items.Count; i++)
            {
                if (endPointURL[i].chckNone == true)
                {
                    checkNone.IsChecked = true;
                }
                else { checkNone.IsChecked = false; }

                if (endPointURL[i].chck128Sign == true)
                {
                    check128Sign.IsChecked = true;
                }
                else { check128Sign.IsChecked = false; }

                if (endPointURL[i].chck128SignEncrypt == true)
                {
                    check128SigndEncrypt.IsChecked = true;
                }
                else { check128SigndEncrypt.IsChecked = false; }

                if (endPointURL[i].chck256Sign == true)
                {
                    check256Sign.IsChecked = true;
                }
                else { check256Sign.IsChecked = false; }
                if (endPointURL[i].chck256SignEncrypt == true)
                {
                    check256SigndEncrypt.IsChecked = true;
                }
                else { check256SigndEncrypt.IsChecked = false; }

                if (check128Sign.IsChecked == false && check128SigndEncrypt.IsChecked == false && check256Sign.IsChecked == false && check256SigndEncrypt.IsChecked == false)
                {
                    checkNone.IsEnabled = false;
                }

            }
            //we need to assign collection for item source
            //inside XAML put DisplayMemberPath as that property name(endPointName) in Endpoint Class
            //endPointList.ItemsSource = endpointCollection;
            #endregion EndPoint

            #region discovery

            configE.discovery_Onload();
            regIntervalTxt.Text = configE.regtext;
            discoveryURList.ItemsSource = configE.discoveryCollection;
            discoveryURList.DisplayMemberPath = "discoveryURL";
            //string regText=configE.discovery_Onload();
            //regIntervalTxt.Text = regText;
            //discoveryURList.ItemsSource = discoveryCollection;
            //discoveryURList.DisplayMemberPath = "discoveryURL";
            #endregion discovery

            #region Security

            string[] level = configE.secgrpOnload();

            #region SecurityLevel
            configE.securityCollection = SLIKDAUACONFIG.LoadSecurityType();
            //For SecurityLevel
            for (int i = 0; i < configE.securityCollection.Count; i++)
            {
                if (level[i] == "None")
                {
                    checkNone.IsChecked = true;
                }
                if (level[i] == "128Sign")
                {
                    check128Sign.IsChecked = true;
                }
                if (level[i] == "128&256Sign")
                {
                    check128Sign.IsChecked = true;
                    check256Sign.IsChecked = true;
                }
                if (level[i] == "128SigndEncrypt")
                {
                    check128SigndEncrypt.IsChecked = true;
                }
                if (level[i] == "128&256SigndEncrypt")
                {
                    check128SigndEncrypt.IsChecked = true;
                    check256SigndEncrypt.IsChecked = true;
                }
            }
            #endregion SecurityLevel

            #region SecurityType
            configE.securityCollection = SLIKDAUACONFIG.LoadSecurityType();

            for (int i = 0; i < configE.securityCollection.Count; i++)
            {
                if (level[i] == "None")
                {
                    checkNone.IsChecked = true;
                }
                if (level[i] == "128")
                {
                    check128Sign.IsChecked = true;
                    check128Sign.IsEnabled = true;
                    check128SigndEncrypt.IsEnabled = true;
                }
                if (level[i] == "256")
                {
                    check256Sign.IsChecked = true;
                    check256Sign.IsEnabled = true;
                    check256SigndEncrypt.IsEnabled = true;
                }
            }
            #endregion SecurityType



            #endregion Security

            #region User Authentication
            //For Authentication CheckBoxes
            string authen = configE.authenticationOnLoad();

            string anon = configE.anonymousOnLoad();
            if (anon == "anonymousUsertrue")
            {
                checkAnonymous.IsChecked = true;
                userAuthList.IsEnabled = false;
                addUserBtn.IsEnabled = false;
            }
            if (anon == "anonymousUserfalse")
            {
                checkAnonymous.IsChecked = false;
                userAuthList.IsEnabled = true;
                addUserBtn.IsEnabled = true;
            }
            if (authen == "authorisedUsertrue")
            {
                checkAuthorised.IsChecked = true;
                userAuthList.IsEnabled = true;
                addUserBtn.IsEnabled = true;
            }
            if (authen == "authorisedUserfalse")
            {
                checkAuthorised.IsChecked = false;
                userAuthList.IsEnabled = false;
                addUserBtn.IsEnabled = false;
            }
            #endregion User Authentication

            #region user Authentication deserialization From file
            configE.UserAuthenticationRetrieveFromFile();
            userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
            userAuthList.DisplayMemberPath = "UserName";

            #endregion user Authentication deserialization From file


            #region Add Logs
            ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("UA Configuration", @"Elpis OPC Server\UA Configuration",
                  "UA Configuration settings are loaded from the SLIKDAUAConfig.xml file successfully.", LogStatus.Information);

            ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("UA Configuration", @"Elpis OPC Server\UA Configuration",
                  "OPC UA Server initialized exposing the following endpoints :", LogStatus.Information);

            foreach (var endpoint in configE.endpointCollection)
            {
                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("UA Configuration", @"Elpis OPC Server\UA Configuration",
                  endpoint.endPointName, LogStatus.Information);
            }
            
            #endregion Add Logs

        }
        #endregion UAConfiguration_loaded event

        #region minbtn_Click Evnet
        private void minbtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            // Minimize
            window.WindowState = WindowState.Minimized;
        }
        #endregion End Of slidebtn_Click Evnet

        #region closebtn_Click Evnet
        private void closebtn_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Hidden;
            //Window window = Window.GetWindow(this);
            Application.Current.Shutdown();
            //// Minimize
            //window.WindowState = WindowState.Minimized;
        }
        #endregion End Of slidebtn_Click Evnet

        #region slidebtn_Click Evnet
        private void slidebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
        #endregion End Of slidebtn_Click Evnet

        #region Refresh
        /// <summary>
        /// Refresh trusted list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_click(object sender, RoutedEventArgs e)
        {
            LoadTrustedItemSrc();
            //TrustedCertList.Filter_Click(null, null);
        }
        #endregion End of Refresh

        #region LoadTrustedItemSrc
        /// <summary>
        /// Load the trusted certificate to the certificate list
        /// </summary>
        private void LoadTrustedItemSrc()
        {
            if (string.IsNullOrEmpty(Store.StorePath)) return;
        }
        #endregion End of LoadRejectedItemSrc

        #region Check_Uncheck
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            userAuthList.IsEnabled = true;
            addUserBtn.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            userAuthList.IsEnabled = false;
            addUserBtn.IsEnabled = false;
        }

        private void checkNone_Unchecked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                checkNone.IsChecked = false;
                configE.endpointCollection[endPointList.SelectedIndex].chckNone = checkNone.IsChecked.Value;
            }
        }

        private void checkNone_Checked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                if(check128Sign.IsChecked ==false && check128SigndEncrypt.IsChecked==false && check256Sign.IsChecked==false && check256SigndEncrypt.IsChecked==false)
                {
                    checkNone.IsEnabled = false;
                }
                checkNone.IsChecked = true;
                configE.endpointCollection[endPointList.SelectedIndex].chckNone = checkNone.IsChecked.Value;
            }
        }

        private void check128Sign_Checked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check128Sign.IsChecked = true;
                checkNone.IsEnabled = true;
                configE.endpointCollection[endPointList.SelectedIndex].chck128Sign = check128Sign.IsChecked.Value;
            }
        }

        private void check128Sign_Unchecked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check128Sign.IsChecked = false;
                if (check128Sign.IsChecked == false && check128SigndEncrypt.IsChecked == false && check256Sign.IsChecked == false && check256SigndEncrypt.IsChecked == false)
                {
                    checkNone.IsEnabled = false;
                    checkNone.IsChecked = true;
                    checkNone.IsEnabled = false;
                }
                configE.endpointCollection[endPointList.SelectedIndex].chck128Sign = check128Sign.IsChecked.Value;
            }
        }

        private void check256Sign_Checked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check256Sign.IsChecked = true;
                checkNone.IsEnabled = true;
                configE.endpointCollection[endPointList.SelectedIndex].chck256Sign = check256Sign.IsChecked.Value;
            }
        }

        private void check256Sign_Unchecked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check256Sign.IsChecked = false;
                if (check128Sign.IsChecked == false && check128SigndEncrypt.IsChecked == false && check256Sign.IsChecked == false && check256SigndEncrypt.IsChecked == false)
                {
                    checkNone.IsEnabled = false;
                    checkNone.IsChecked = true;
                    checkNone.IsEnabled = false;
                }
                configE.endpointCollection[endPointList.SelectedIndex].chck256Sign = check256Sign.IsChecked.Value;
            }
        }

        private void check128SigndEncrypt_Checked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check128SigndEncrypt.IsChecked = true;
                checkNone.IsEnabled = true;
                configE.endpointCollection[endPointList.SelectedIndex].chck128SignEncrypt = check128SigndEncrypt.IsChecked.Value;
            }
        }

        private void check128SigndEncrypt_Unchecked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check128SigndEncrypt.IsChecked = false;
                if (check128Sign.IsChecked == false && check128SigndEncrypt.IsChecked == false && check256Sign.IsChecked == false && check256SigndEncrypt.IsChecked == false)
                {
                    checkNone.IsEnabled = false;
                    checkNone.IsChecked = true;
                    checkNone.IsEnabled = false;
                }
                configE.endpointCollection[endPointList.SelectedIndex].chck128SignEncrypt = check128SigndEncrypt.IsChecked.Value;
            }
        }

        private void check256SigndEncrypt_Checked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check256SigndEncrypt.IsChecked = true;
                checkNone.IsEnabled = true;
                configE.endpointCollection[endPointList.SelectedIndex].chck256SignEncrypt = check256SigndEncrypt.IsChecked.Value;
            }
        }

        private void check256SigndEncrypt_Unchecked(object sender, RoutedEventArgs e)
        {
            EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
            if (endPointURL != null)
            {
                check256SigndEncrypt.IsChecked = false;
                if (check128Sign.IsChecked == false && check128SigndEncrypt.IsChecked == false && check256Sign.IsChecked == false && check256SigndEncrypt.IsChecked == false)
                {
                    checkNone.IsEnabled = false;
                    checkNone.IsChecked = true;
                    checkNone.IsEnabled = false;
                }
                configE.endpointCollection[endPointList.SelectedIndex].chck256SignEncrypt = check256SigndEncrypt.IsChecked.Value;
            }
        }

        #endregion Check_Uncheck

        #region Add
        private void addDiscoveryURLBtn_Click(object sender, RoutedEventArgs e)
        {
            if(urlTxt.Text!=null)
            {
                configE.DiscoveryOnadd(urlTxt.Text);
                discoveryURList.ItemsSource = configE.discoveryCollection;
            }            
        }

        private void addEndPointURLBtn_Click(object sender, RoutedEventArgs e)
        {
            if(endPointText.Text!=null)
            {
                configE.EndpointOnadd(endPointText.Text,checkNone.IsChecked,check128Sign.IsChecked,check128SigndEncrypt.IsChecked,check256Sign.IsChecked,check256SigndEncrypt.IsChecked);
                endPointList.ItemsSource = configE.endpointCollection;
            }      

            //endPointList.DisplayMemberPath = dr.endPointName;
        }

        private void addUserBtn_Click(object sender, RoutedEventArgs e)
        {
            AddNewUserUI newUser = new AddNewUserUI();
            newUser.ShowDialog();

            if (newUser.userAuthViewModel != null)
            {
                if (newUser.userAuthViewModel.UserName != null && newUser.userAuthViewModel.UserName!="")
                {
                    if (newUser.userAuthViewModel.Password == newUser.userAuthViewModel.ConfirmPassWord)
                    {
                        //configE.AddUser(newUser.userAuthViewModel);
                        configE.UserOnadd(newUser.userAuthViewModel.UserName, newUser.userAuthViewModel.Password, newUser.userAuthViewModel.ConfirmPassWord);

                        userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
                        userAuthList.DisplayMemberPath = "UserName";
                    }
                    else
                    {
                        MessageBox.Show("Password must be confirmed");
                    }
                }
            }
        }

        private void addUserContextMenu_Click(object sender, RoutedEventArgs e)
        {
            AddNewUserUI newUser = new AddNewUserUI();
            newUser.ShowDialog();

            if (newUser.userAuthViewModel != null)
            {
                if (newUser.userAuthViewModel.UserName != null && newUser.userAuthViewModel.UserName!="")
                {
                    if (newUser.userAuthViewModel.Password == newUser.userAuthViewModel.ConfirmPassWord)
                    {
                        //configE.AddUser(newUser.userAuthViewModel);
                        configE.UserOnadd(newUser.userAuthViewModel.UserName, newUser.userAuthViewModel.Password, newUser.userAuthViewModel.ConfirmPassWord);

                        userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
                        userAuthList.DisplayMemberPath = "UserName";
                    }
                    else
                    {
                        MessageBox.Show("Password must be confirmed");
                    }
                }
                else
                {
                    MessageBox.Show("User Name not empty.");
                }
            }
        }

        #endregion Add

        #region delete
        private void deleteDiscoveryURLBtn_Click(object sender, RoutedEventArgs e)
        {
            //configE.DiscoveryOnDeleteete(discoveryURList);
            DiscoveryRegistration dr = discoveryURList.SelectedItem as DiscoveryRegistration;
            if(configE.discoveryCollection.Count>1)
            {
                configE.discoveryCollection.Remove(dr);
            }
            else
            {
                MessageBox.Show("At least one Discovery Server will be present.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
           
            urlTxt.Text = "";
        }

        private void deleteEndPointURLBtn_Click(object sender, RoutedEventArgs e)
        {
            EndPoint dr = endPointList.SelectedItem as EndPoint;
            if(configE.endpointCollection.Count>1)
            {
                configE.endpointCollection.Remove(dr);
                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("UA Configuration", "OPC Server//UA Configuration", "End point is deleted successfully.", LogStatus.Information);
            }
            else
            {
                MessageBox.Show("Not able to delete End Point.\nMinimum one End Point will be present.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("UA Configuration", "OPC Server//UA Configuration", "End point is deletion is failed.", LogStatus.Error);
            }
            
            endPointText.Text = "";
        }

        private void removeUserContextMenu_Click(object sender, RoutedEventArgs e)
        {

            if (userAuthList.SelectedItem != null)
            {
                UserAuthenticationViewModel dr = userAuthList.SelectedItem as UserAuthenticationViewModel;
                configE.UserAuthenticationViewModelCollection.Remove(dr);
            }
            else
            {
                MessageBox.Show("Select UserName from the list", "Elpis OPC server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion delete

        #region edit
        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            UserAuthenticationViewModel user = userAuthList.SelectedItem as UserAuthenticationViewModel;
            if(user !=null)
            {
                
                AddNewUserUI newUser = new AddNewUserUI(user);
                //newUser.NewUserPropertyGrid.SelectedObject = user;
                newUser.ShowDialog();
                if (newUser.userAuthViewModel != null)
                {
                    if (newUser.userAuthViewModel.UserName != null && newUser.userAuthViewModel.UserName != "")
                    {
                        if (newUser.userAuthViewModel.Password == newUser.userAuthViewModel.ConfirmPassWord)
                        {
                            configE.UserAuthenticationViewModelCollection.Remove(user);
                            configE.AddUser(newUser.userAuthViewModel);
                            userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
                            userAuthList.DisplayMemberPath = "UserName";
                        }
                        else
                        {
                            MessageBox.Show("Password must be confirmed");
                        }
                    }
                    else
                    {
                        //MessageBox.Show("User Name not empty.");
                        userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
                        userAuthList.DisplayMemberPath = "UserName";
                    }
                }
                else
                {
                    userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
                    userAuthList.DisplayMemberPath = "UserName";
                }
            }
            else
            {
                MessageBox.Show("Select UserName from the list", "Elpis OPC server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }


        #endregion edit

        private void regIntervalTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text.ToString()); /*IsTextAllowed(e.Text.ToString());*/
        }

        private void discoveryURList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (discoveryURList.SelectedItem != null)
            {
                DiscoveryRegistration discoveryRegister = discoveryURList.SelectedItem as DiscoveryRegistration;
                urlTxt.Text = discoveryRegister.discoveryURL;
            }
        }

        private void endPointList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (endPointList.SelectedItem != null)
            {
                EndPoint endPointURL = endPointList.SelectedItem as EndPoint;
                //EndPoint endPointURLl = endPointList.SelectedItem as EndPoint;

                checkNone.IsChecked = endPointURL.chckNone;
                check128Sign.IsChecked = endPointURL.chck128Sign;
                check128SigndEncrypt.IsChecked = endPointURL.chck128SignEncrypt;
                check256Sign.IsChecked = endPointURL.chck256Sign;
                check256SigndEncrypt.IsChecked = endPointURL.chck256SignEncrypt;

                configE.endpointCollection[endPointList.SelectedIndex].chckNone = endPointURL.chckNone;
                configE.endpointCollection[endPointList.SelectedIndex].chck128Sign = endPointURL.chck128Sign;
                configE.endpointCollection[endPointList.SelectedIndex].chck128SignEncrypt = endPointURL.chck128SignEncrypt;
                configE.endpointCollection[endPointList.SelectedIndex].chck256Sign = endPointURL.chck256Sign;
                configE.endpointCollection[endPointList.SelectedIndex].chck256SignEncrypt = endPointURL.chck256SignEncrypt;
                endPointList.ItemsSource = configE.endpointCollection;
                endPointText.Text = endPointURL.endPointName;
            }

        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            //for UserIdentityTokens
            configE.userIdentityTokens(checkAnonymous.IsChecked.Value, checkAuthorised.IsChecked.Value);

            //for DiscoveryRegistration
            string intText = regIntervalTxt.Text;
            configE.DiscoveryRegistration(intText);
            //check128Sign.IsChecked
            //for uaEndpoint
            int endPointsCount = endPointList.Items.Count;
            configE.uaEndpoint(checkNone.IsChecked.Value, check128Sign.IsChecked.Value, check128SigndEncrypt.IsChecked.Value, check256Sign.IsChecked.Value, check256SigndEncrypt.IsChecked.Value);

            //for  user Authentication serialization to file
            configE.UserAuthenticationSaveToFile();

            #region Add Logs
            ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("Internet Of Things", @"Elpis OPC Server\UA Configuration",
                 "UA Configuration settings are changed and saved back to the SLIKDAUAConfig.xml file successfully.", LogStatus.Information);
            #endregion Add Logs
            // to hide the user control
          //  this.Visibility = Visibility.Hidden;
        }

        private void helpBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void checkAnonymous_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
           if( checkBox.Name== "checkAnonymous")
            {
                checkAuthorised.IsChecked = false;
                addUserBtn.IsEnabled = false;
                userAuthList.IsEnabled = false;
            }          
           else
            {
                checkAnonymous.IsChecked = false;
                addUserBtn.IsEnabled = true;
                userAuthList.IsEnabled = true;
            } 
        }

        private void checkAnonymous_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.Name == "checkAnonymous")
            {
                checkAuthorised.IsChecked = true;
                addUserBtn.IsEnabled = true;
                userAuthList.IsEnabled = true;
            }
            else
            {
                checkAnonymous.IsChecked = true;
                addUserBtn.IsEnabled = false;
                userAuthList.IsEnabled = false;
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            if(checkAuthorised.IsChecked==true)
            {
                AddNewUserUI newUser = new AddNewUserUI();
                newUser.ShowDialog();

                if (newUser.userAuthViewModel != null)
                {
                    if (newUser.userAuthViewModel.UserName != null)
                    {
                        if (newUser.userAuthViewModel.Password == newUser.userAuthViewModel.ConfirmPassWord)
                        {
                            configE.AddUser(newUser.userAuthViewModel);

                            userAuthList.ItemsSource = configE.UserAuthenticationViewModelCollection;
                            userAuthList.DisplayMemberPath = "UserName";
                            ConfigurationSettingsUI.elpisServer.LoadDataLogCollection("UA Configuration", "Elpis OPC Server\\UA Configuration", "User is Created", LogStatus.Information);                            
                        }
                        else
                        {
                            MessageBox.Show("Password must be confirmed");
                        }
                    }
                }
            }
        }

        #region about window
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            Window aboutWindow = new Window();
            
            aboutWindow.Content = about;
            aboutWindow.WindowStyle = WindowStyle.ToolWindow;
            aboutWindow.Show();
            //about.Visibility = Visibility.Visible;
            //MainWindow2.contentPresenter.Content = about;
        }
        #endregion

        //private void regIntervalTxt_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox textBox = sender as TextBox;
        //    if(textBox.Text == "" || int.Parse(textBox.Text)<=0 )
        //    {
        //        MessageBox.Show("Discovery Server Intervals must be grater than 0.","Elpis OPC Server",MessageBoxButton.OK,MessageBoxImage.Warning);
        //        textBox.Text = "10";
        //    }
        //}
    }
}
#endregion ElpisOpcServer namespace