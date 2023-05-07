using Elpis.Windows.OPC.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using LicenseManager;

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for License.xaml
    /// </summary>
    public partial class License : UserControl
    {
        #region Private Field

        //public LicenseHandler licenseManager { get; set; }
        public LicenseViewModel licenseViewModel { get; set; }
        public AboutPageUI about { get; set; }


        #endregion End Of Private Field

        public License()
        {
            InitializeComponent();
            //licenseManager = new LicenseHandler();
            licenseViewModel = new LicenseViewModel();
            //licenseViewModel.ComputerId = licenseManager.GetComputerId("LicenseStatus.dat");
            //txtComputerId.Text = licenseViewModel.ComputerId.ToString();
            txtComputerId.Text = GetMacId();
            //MessageBox.Show("computer id is " + licenseViewModel.ComputerId);
            about = new AboutPageUI();
        }             
       
        private void activebtn_Click(object sender, RoutedEventArgs e)
        {
            //licenseViewModel.LiecenceKey = txtLicenceKey.Text;
            //licenseViewModel.ComputerId = (int.Parse)(txtComputerId.Text);
            //licenseViewModel.Activate();
            ActivateLicense();
            txtmessage.Text = licenseViewModel.LicenseStatus;

        }

        private void ActivateLicense()
        {

            if (!string.IsNullOrEmpty(txtLicenceKey.Text))
            {
                string DecrypatedString = GeDecrypted(txtLicenceKey.Text);

                if (DecrypatedString != null && (DecrypatedString == txtComputerId.Text))
                {
                    txtError.Visibility = Visibility.Hidden;
                    txtError.Text = string.Empty;
                    SaveConfig(txtLicenceKey.Text);
                    MessageBox.Show("ElpisOpcServer Activated Successfully please restart the application", "ElpisOpcServer", MessageBoxButton.OK);

                }
                else
                {
                    txtError.Visibility = Visibility.Visible;
                    txtError.Text = "*Invalid License Key.";
                    //MessageBox.Show("Invalid License Key", "ElpisOpcServer", MessageBoxButton.OK);
                }
            }
            else
            {
                txtError.Visibility = Visibility.Visible;
                txtError.Text = "*Please enter License key.";

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



        private string GetMacId()
        {
            String sMacAddress = string.Empty;

            NetworkInterface[] netWorkInterface = NetworkInterface.GetAllNetworkInterfaces();
            var NetworkType = netWorkInterface.Select(x => x.NetworkInterfaceType);
            if (NetworkType.Contains(NetworkInterfaceType.Ethernet))
            {
                var ethernet = netWorkInterface.Where(network => (network.Name.Equals("Ethernet") || network.Name.Equals("Local Area Connection"))).FirstOrDefault();
                if (ethernet != null)
                {
                    sMacAddress = ethernet.GetPhysicalAddress().ToString();
                }
                else
                {
                    ethernet = netWorkInterface.Where(network => (!network.Description.Contains("Virtual") || !network.Description.Contains("Pseudo"))).FirstOrDefault();
                    sMacAddress = ethernet.GetPhysicalAddress().ToString();
                }
            }
            else
            {
                var ethernet = netWorkInterface.Where(network => (!network.Description.Contains("Virtual") || !network.Description.Contains("Pseudo"))).FirstOrDefault();
                sMacAddress = ethernet.GetPhysicalAddress().ToString();
            }
            return sMacAddress;

        }

        internal bool CheckActivation()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            string ActivationKey = ConfigurationSettings.AppSettings["ActivationKey"];
#pragma warning restore CS0618 // Type or member is obsolete
            txtLicenceKey.Text = ActivationKey;
            if (!string.IsNullOrEmpty(ActivationKey))
            {
                string DecryptedString = GeDecrypted(ActivationKey);
                if (DecryptedString != null)
                {
                    List<string> macId = GetMacIDList();
                    foreach (var item in macId)
                    {
                        if (DecryptedString == item)
                        {
                            txtError.Visibility = Visibility.Hidden;
                            txtError.Text = string.Empty;
                            return true;
                        }

                    }
                    txtError.Visibility = Visibility.Visible;
                    txtError.Text = "*Invalid License key.";



                }
                else
                {
                    txtError.Visibility = Visibility.Visible;
                    txtError.Text = "*Invalid License key.";
                    return false;

                }

            }
            else
            {
                txtError.Visibility = Visibility.Visible;
                txtError.Text = "*License Key is not Present";
                return false;
            }
            return false;

        }

        /// <summary>
        /// Decrypt the Activation key
        /// </summary>
        /// <param name="activationKey"></param>
        /// <returns></returns>
        private String GeDecrypted(string activationKey)
        {
            try
            {
                string EncryptionKey = "OpQ3$7F@II26/hdElpisItsolutions";
                byte[] cipherBytes = Convert.FromBase64String(activationKey);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x45, 0x6c, 0x70, 0x49, 0x53, 0x6f, 0x50, 0x43, 0x73, 0x45, 0x52, 0x76, 0x65, 0x72, 0x52, 0x65, 0x70, 0x6f, 0x72, 0x74, 0x74, 0x6f, 0x6f, 0x6c });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        return Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// Get system MACID for activation check
        /// </summary>
        /// <returns></returns>
        private List<string> GetMacIDList()
        {
            //NetworkInterface[] netWorkInterface = NetworkInterface.GetAllNetworkInterfaces();
            //var ethernet = netWorkInterface.Where(network => network.NetworkInterfaceType == NetworkInterfaceType.Ethernet && network.Name.Equals("Ethernet")).FirstOrDefault();
            //String sMacAddress = ethernet == null ? string.Empty : ethernet.GetPhysicalAddress().ToString();
            //return sMacAddress;

            List<string> MacAddressList = new List<string>();

            NetworkInterface[] netWorkInterface = NetworkInterface.GetAllNetworkInterfaces();
            var NetworkType = netWorkInterface.Select(x => x.NetworkInterfaceType);
            if (NetworkType.Contains(NetworkInterfaceType.Ethernet))
            {

                var ethernet = netWorkInterface.Where(network => (!network.Description.Contains("Virtual") || !network.Description.Contains("Pseudo")));
                foreach (var item in ethernet)
                {
                    MacAddressList.Add(item.GetPhysicalAddress().ToString());
                }


            }
            else
            {
                var ethernet = netWorkInterface.Where(network => (!network.Description.Contains("Virtual") || !network.Description.Contains("Pseudo")));
                foreach (var item in ethernet)
                {
                    MacAddressList.Add(item.GetPhysicalAddress().ToString());
                }
            }
            return MacAddressList;
        }

        private bool SaveConfig(string EncryptMacId)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["ActivationKey"].Value = EncryptMacId;
            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
            return true;
        }



        #endregion

        private void TxtLicenceKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLicenceKey.Text))
            {
                txtError.Visibility = Visibility.Visible;
                txtError.Text = "*License Key is not Present";
            }
            else
            {
                txtError.Visibility = Visibility.Hidden;
                txtError.Text = string.Empty;
            }

        }


    }
}
