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
using OPCEngine;
using Elpis.Windows.OPC.Server;
#endregion Namespaces

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for UAConfiguration.xaml
    /// </summary>
    public partial class UACertificateSettingsUI : System.Windows.Controls.UserControl
    {
        #region PublicField

        public Opc.Ua.CertificateManagementLibrary.IStore Store { get; set; }
        public AboutPageUI about { get;  set; }

        UACertificateViewModel opcUa = new UACertificateViewModel();
       

        public UACertificateSettingsUI()
        {
            InitializeComponent();
            Store = opcUa.InitializingStorage();
            about = new AboutPageUI();
        }
        private void slidebtn_Click(object sender, RoutedEventArgs e)
        {
            //  this.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Collapsed;
            //opcUa.isNewStore = true;
        }
        #endregion End Of slidebtn_Click Evnet

        private void createBtn_Click(object sender, RoutedEventArgs e)
        { 
            ObservableCollection<string> trustedValue = new ObservableCollection<string>();
            ObservableCollection<string> rejectedValue = new ObservableCollection<string>();
            string title = titleForUserControl.Content.ToString();
            trustedValue = opcUa.createCertificate(title);
            //trustedListBox.ItemsSource = trustedValue;
           trustedListBox.ItemsSource = opcUa.TrustedValue;
            rejectedListBox.ItemsSource = opcUa.RejectedValue;
        }

        private void trustBtn_Click(object sender, RoutedEventArgs e)
        {
            string sourcePath = Directory.GetCurrentDirectory();
            if (rejectedListBox.SelectedValue != null)
            {
                string selectedRejectedCertificate = sourcePath + "\\PKI\\CA\\rejected\\" + rejectedListBox.SelectedValue.ToString();
                opcUa.trustedClick(opcUa.RejectedValue, opcUa.TrustedValue, selectedRejectedCertificate, sourcePath);
            }
            else
            {

                MessageBox.Show("Select the certificate in Rejected List first!!!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                opcUa.uacertificateload();
            }
            trustedListBox.ItemsSource = opcUa.TrustedValue;
            rejectedListBox.ItemsSource = opcUa.RejectedValue;

        }

        private void rejectBtn_Click(object sender, RoutedEventArgs e)
        {
            string sourcePath = Directory.GetCurrentDirectory();
            if (trustedListBox.SelectedValue != null)
            {                
                string selectedTrustedCertificate = @"" + sourcePath + "\\" + Store.StoreName + "\\trusted\\certs\\" + trustedListBox.SelectedValue.ToString();
                opcUa.rejectClick(opcUa.RejectedValue, opcUa.TrustedValue, selectedTrustedCertificate, sourcePath);
                FileInfo fileInfo = new FileInfo(selectedTrustedCertificate);
                //X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
                //Store.RejectCertificate(certificate);

            }
            else
            {
                MessageBox.Show("Select the certificate in Trusted List first!!!", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                opcUa.uacertificateload();
            }
            trustedListBox.ItemsSource = opcUa.TrustedValue;
            rejectedListBox.ItemsSource = opcUa.RejectedValue;

        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {

            opcUa.importCertificate();
            //opcUa.isNewStore = true;
            trustedListBox.ItemsSource = opcUa.TrustedValue;
           // trustedListBox.DisplayMemberPath=""
        }

        private void UACertificate_loaded(object sender, RoutedEventArgs e)
        {            
            opcUa.uacertificateload();
            trustedListBox.ItemsSource = opcUa.TrustedValue;
            rejectedListBox.ItemsSource = opcUa.RejectedValue;
        }

        private void trustedListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Opc.Ua.UserComponents.Tools.CertificateListItem obj = trustedListBox.SelectedItem as Opc.Ua.UserComponents.Tools.CertificateListItem;
        }

        private void viewBtn_Click(object sender, RoutedEventArgs e)
        {
            if(trustedListBox.SelectedItem!=null || rejectedListBox.SelectedItem!=null)
            {
                //opcUa.viewCertificate();
                if (rejectedListBox.SelectedItem == null)
                    ViewCertificate(trustedListBox.SelectedItem as string,true);
                else
                    ViewCertificate(rejectedListBox.SelectedItem as string,false);
            }
            else
            {
                MessageBox.Show("Select the Certificate to View", @"Elpis OPC Server (Certification)", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }


        private void ViewCertificate(string selectedItem, bool trust)
        {
            string sourcePath = Directory.GetCurrentDirectory();
            string path = string.Empty;
            if (trust)
            {
                path = sourcePath + "\\PKI\\CA\\trusted\\certs";
            }
            else
            {
                path = sourcePath + "\\PKI\\CA\\rejected";
            }

            if (Directory.Exists(path))
            {
                string[] trustedCertificates = Directory.GetFiles(@path);
                for (int i = 0; i < trustedCertificates.Length; i++)
                {
                    FileInfo fileInfo = new FileInfo(trustedCertificates[i]);
                    try
                    {
                        //Create certificate from path
                        X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
                        if (certificate != null)
                        {
                            string filePath = Elpis.Windows.OPC.Server.CertificateUtils.GetCertificateFileName(certificate, "der");                            
                            //To dispaly the certificate
                            if (filePath == selectedItem)
                            {
                                X509Certificate2UI.DisplayCertificate(certificate as X509Certificate2);
                                return;
                            }
                        }
                    }
                    catch (CryptographicException ex)
                    {                       
                        MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Event handling for certificate exporting.. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportCertificate();
        }

        /// <summary>
        /// Exports the selected certificate from Trusted/ Rejected list to required location.
        /// </summary>
        private void ExportCertificate()
        {
            //opcUa.isNewStore = true;
            if (trustedListBox.SelectedItem != null)
            {
                opcUa.exportCertificate(trustedListBox.SelectedItem as string, true);
            }
            else if (rejectedListBox.SelectedItem != null)
            {
                opcUa.exportCertificate(rejectedListBox.SelectedItem as string, false);
            }
            else
            {
                MessageBox.Show("Select certificate in Trusted or Rejected List to Export the Certificate", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            parent.Close();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Find the window that contains the control
            Window window = Window.GetWindow(this);

            // Minimize
            window.WindowState = WindowState.Minimized;

            // Restore
            //window.WindowState = WindowState.Normal;

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

    }
}

