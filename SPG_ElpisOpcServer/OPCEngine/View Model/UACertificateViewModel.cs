#region Namespaces

using System;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua.CertificateManagementLibrary;
using Opc.Ua;
using Opc.Ua.UserComponents;
using Opc.Ua.UserComponents.Tools;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Windows.Threading;
#endregion Namespaces

#region OPCEngine namespace

#region OPCEngine NameSpace
namespace Elpis.Windows.OPC.Server
{
    #region UACertificateViewModel class

    public class UACertificateViewModel : DependencyObject
    {
        

        #region feilds
        public bool ImportNewStore = true;
        public bool isNewStore = false;
        #endregion End of feilds

        #region Properties

        public string TrustedCertificatePath { get; set; }
        public Opc.Ua.CertificateManagementLibrary.IStore Store { get; set; }

        public string StorePath
        {
            get { return (string)GetValue(StorePathProperty); }
            set { SetValue(StorePathProperty, value); }
        }

        public static readonly DependencyProperty StorePathProperty =
            DependencyProperty.Register("StorePath", typeof(string), typeof(TrustedCtrl), new PropertyMetadata(new PropertyChangedCallback(PathChanged)));

        public static void PathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) { }
        
        ObservableCollection<string> trustedValue = new ObservableCollection<string>();
        public ObservableCollection<string> TrustedValue
        {
            get { return trustedValue; }


            set
            {
                if (trustedValue != null)
                    value = trustedValue;
            }
        }

        ObservableCollection<string> rejectedValue = new ObservableCollection<string>();
        public ObservableCollection<string> RejectedValue
        {
            get { return rejectedValue; }

            set
            {
                if (rejectedValue != null)
                    value = rejectedValue;
            }
        }

        #region CAPrivateKey
        /// <summary>
        /// Issue authority key path
        /// </summary>
        public string CAPrivateKey
        {
            get { return (string)GetValue(CAPrivateKeyProperty); }
            set { SetValue(CAPrivateKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CAPrivateKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CAPrivateKeyProperty =
            DependencyProperty.Register("CAPrivateKey", typeof(string), typeof(TrustedCtrl));
        #endregion End Of CAPrivateKey

        #endregion End of Properties

        #region Methods

        //Create and Update the Store
        public Opc.Ua.CertificateManagementLibrary.IStore InitializingStorage()
        {
            //Store = UAcertificateEngine.CertificationClasses.StoreFactory.GetCertificateStore();
            //Store.StoreName = "PKI\\CA";
            //Store.StorePath = Directory.GetCurrentDirectory();
            Store = OPCEngine.StoreFactory.GetCertificateStore();
            Store.StoreName = "PKI\\CA";
            Store.StorePath = Path.Combine(Directory.GetCurrentDirectory(), "PKI\\CA");
            isNewStore = true;
            return Store;

        }

        //Creating the Certificate
        public ObservableCollection<string> createCertificate(string title)
        {
            try
            {
                string[] subDirectories = Directory.GetDirectories(Store.StorePath);
                if (subDirectories.Count() > 0)
                {
                    isNewStore = false;
                    //Store.StoreName = "PKI\\CA\\trusted";
                    Store.StorePath= Path.Combine(Directory.GetCurrentDirectory(), "PKI\\CA\\trusted");
                    ((Opc.Ua.CertificateManagementLibrary.DirectoryStore)Store).TrustedCertificatePath= Path.Combine(Directory.GetCurrentDirectory(), "PKI\\CA\\trusted");
                }
            }
            catch(Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                    ElpisServer.Addlogs("Configuration", @"Elpis/Certificate", ex.Message, LogStatus.Error);
                //}), DispatcherPriority.Normal, null);
            }
            
            
               // string currentStore = null; 
           
                if (Store.GetType() == typeof(Opc.Ua.CertificateManagementLibrary.DirectoryStore))
                {

                    string storePath = Directory.GetCurrentDirectory();
                    System.Windows.Forms.FolderBrowserDialog rootFolder = new System.Windows.Forms.FolderBrowserDialog();
                    if (Directory.Exists(storePath))
                        rootFolder.SelectedPath = storePath;
                    rootFolder.Description = "Store path Selection";

                    //if (rootFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    //    storePath = rootFolder.SelectedPath;
                    //else return;
                }
                

                if (Store != null)
                {
                //    Store.StorePath = Directory.GetCurrentDirectory();
                //if (isNewStore)//TODO Changing trusted path here.
                //{
                    //currentStore = System.IO.Path.Combine(Store.StorePath.Trim(), Store.StoreName.Trim());
                    //Store.StorePath = System.IO.Path.Combine(Store.StorePath.Trim(), Store.StoreName.Trim());
                    //try
                    //{
                    //    ((Opc.Ua.CertificateManagementLibrary.DirectoryStore)Store).TrustedCertificatePath = currentStore + "\\certs";
                    //}
                    //catch(Exception ex)
                    //{

                    //}                   

                //}
                if (Store.Type == StoreType.Directory && isNewStore)

                        if (! CertificateUtils.StoreFolderCreator(Store.StorePath, isNewStore))
                            ImportNewStore = false;
                }
                
                //Store =OPCEngine.StoreFactory.GetCertificateStore();


            if ((string.IsNullOrEmpty(Store.StorePath.Trim())) || (!(Store != null && Store.CheckPath())))
            {
                //string title = titleForUserControl.Content.ToString();
                if (Store != null) Store.StorePath = string.Empty;
                System.Windows.MessageBox.Show("Please select proper store path", title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //MessageBox.Show("Please select proper store path", title,MessageBoxButtons.OK, MessageBoxIcon.Exclamation);               
                return null;
            }            

            //Close on success

            if (Store.StorePath != "" && Store.StoreName != "")
            {               
                CreateCertificateWindow cwobj = new CreateCertificateWindow(null, Store.StorePath, Store, CAPrivateKey, "Create Certificate", "Create");
                cwobj.ShowDialog();                

                if (cwobj.Issued != false)
                {
                    Refresh_click(null, null);
                    System.Windows.MessageBox.Show("Certificate Created successfully", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                Store.StorePath= Path.Combine(Directory.GetCurrentDirectory(), "PKI\\CA");
                //for trusted list box every time we need to clear the item                
                TrustedValue.Clear();
                UpdateTrustList();
                

                // for rejected list box, every time we need to clear the item
                RejectedValue.Clear();
                UpdateRejectedList();               
            }           
            isNewStore = false;
            return trustedValue;

        }


        //loading the trusted items
        private void Refresh_click(object sender, RoutedEventArgs e)
        {
            LoadTrustedItemSrc();
            //TrustedCertList.Filter_Click(null, null);
        }

        //check if the storePath is null or not
        private void LoadTrustedItemSrc()
        {
            if (string.IsNullOrEmpty(Store.StorePath)) return;
            //TrustedCertList.BasicCertificateCollection = Helper.ItemCollection(Store, "Trust", Store.StorePath);
            //TrustedCertList.CertList.ItemsSource = TrustedCertList.BasicCertificateCollection;
        }

        //View the Certificate
        public void viewCertificate()
        {
            string sourcePath = Directory.GetCurrentDirectory();
            string pathTrust = sourcePath + "\\PKI\\CA\\certs";
            if (Directory.Exists(pathTrust))
            {
                string[] trustedCertificates = Directory.GetFiles(@pathTrust);
                for (int i = 0; i < trustedCertificates.Length; i++)
                {

                    FileInfo fileInfo = new FileInfo(trustedCertificates[i]);
                    try
                    {
                        //Create certificate from path
                        X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
                        if (certificate != null)
                        {
                            string filePath = CertificateUtils.GetCertificateFileName(certificate, "der");
                            if (certificate.Issuer.Contains("Elpis OPC Server"))
                                if (certificate.SubjectName.Name.Contains(System.Net.Dns.GetHostName()))
                                {
                                    //CertificateListItem obj = trustedListBox.SelectedItem as CertificateListItem;
                                    Opc.Ua.UserComponents.Tools.CertificateListItem listItem = new Opc.Ua.UserComponents.Tools.CertificateListItem();
                                    List<string> fields = Utils.ParseDistinguishedName(certificate.Subject);
                                    //Get Certificate  type
                                    for (int j = 0; j < fields.Count; j++)
                                    {
                                        if (fields[j].StartsWith("CN="))
                                            listItem.Name = fields[j].Substring("CN=".Length);
                                        if (fields[j].StartsWith("DC="))
                                            listItem.Type = fields[j].Substring("DC=".Length);
                                    }
                                    //Get Certificate  Name
                                    if (String.IsNullOrEmpty(listItem.Name))
                                        listItem.Name = String.Format("{0}", certificate.Subject);

                                    fields = Utils.ParseDistinguishedName(certificate.Issuer);

                                    //Get Certificate Issuer name
                                    for (int j = 0; j < fields.Count; j++)
                                    {
                                        if (fields[j].StartsWith("CN="))
                                            listItem.Issuer = fields[j].Substring("CN=".Length);
                                    }
                                    // determine certificate type.
                                    foreach (X509Extension extension in certificate.Extensions)
                                    {
                                        X509BasicConstraintsExtension basicContraints = extension as X509BasicConstraintsExtension;

                                        if (basicContraints != null)
                                        {
                                            if (basicContraints.CertificateAuthority)
                                                listItem.Type = "CA";
                                            else
                                                listItem.Type = "End-Entity";

                                            break;
                                        }
                                    }

                                    //To dispaly the certificate
                                    X509Certificate2UI.DisplayCertificate(certificate as X509Certificate2);
                                }

                        }
                    }
                    catch (CryptographicException ex)
                    {
                        string errMsg = ex.Message;
                    }
                }

            }
        }

        
        //Import the Certificate
        public void importCertificate()
        {
            //TrustedValue.Clear();
            //Get import path

            string importPath = ImportFileDlg();
            //Store.StorePath = @"" + Store.StorePath + "\\" + Store.StoreName;

            if (string.IsNullOrEmpty(importPath))
                return;

            //Import certificate 08 11 2016

            bool check = ImportCertificate(importPath);
            if (check == false)
                System.Windows.MessageBox.Show(Store.ErrorMessage, "Import", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                System.Windows.MessageBox.Show("Certificate successfully Imported", "Elpis OPC Sever 1.0", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Refresh_click(null, null);
                TrustedValue.Clear();
                UpdateTrustList();
            }
            #region oldcode
            //if (Store != null && !Store.ImportCertificate(importPath))
            //System.Windows.MessageBox.Show(Store.ErrorMessage, "Import", MessageBoxButton.OK, MessageBoxImage.Error);

            ////Refresh
            //Refresh_click(null, null);

            //for trusted listbox

            ////EveryTime we need to clear the item
            //TrustedValue.Clear();

            ////08 11 2016
            ////for moving to pki folder
            //string pathToDelete = Store.StorePath + "\\trusted\\certs";
            //string[] currentTrustedCertificate = Directory.GetFiles(@pathToDelete);
            //FileInfo fileInfo1 = new FileInfo(currentTrustedCertificate[0]);
            //try
            //{
            //    //Create certificate from path
            //    X509Certificate2 certificate = new X509Certificate2(fileInfo1.FullName, (string)null, X509KeyStorageFlags.Exportable);
            //    // 12 10 2016
            //    string filePath = OPCEngine.CertificateUtils.GetCertificateFileName(certificate, "der");
            //    string sourcePath = pathToDelete + "\\" + filePath;
            //    if (File.Exists(sourcePath))
            //    {
            //        string targetpath = Store.StorePath + "\\PKI\\CA\\certs";
            //        string targetFile = targetpath + "\\" + filePath;
            //        File.Move(sourcePath, targetFile);
            //        //File.Delete(pathToDelete);
            //    }
            //}
            //catch (CryptographicException ex)
            //{
            //    //Get error message. It will be used by the called method
            //    //DirectoryStore ErrorMessage = ex.Message;
            //}

            #endregion


        }

        /// <summary>
        /// Update the Trusted certificate list.
        /// </summary>
        /// <returns></returns>
        private void UpdateTrustList()
        {
            TrustedValue.Clear();
            //string pathTrust = Store.StorePath + "\\PKI\\CA\\certs";

            string pathTrust = Store.StorePath + "\\trusted\\certs";
            //string pathTrust1 = string.Format(@"{0}\{1}\certs", Store.StorePath, Store.StoreName);
            string path = pathTrust;//Directory.Exists(pathTrust) ? pathTrust : pathTrust1;
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
                        // 12 10 2016
                        if (certificate != null)//&& !certificate.SubjectName.Name.Contains("Elpis OPC Server"))
                        {
                            string filePath = CertificateUtils.GetCertificateFileName(certificate, "der");
                            // trustedListBox.ItemsSource = filePath;
                            TrustedValue.Add(filePath);
                        }
                    }
                    catch (CryptographicException ex)
                    {
                        string errMsg = ex.Message;
                    }
                }
            }
            
        }
        
        /// <summary>
        /// Update the rejected certificate list.
        /// </summary>
        private void UpdateRejectedList()
        {
            RejectedValue.Clear();
            string sourcePath = Directory.GetCurrentDirectory();
            string pathReject = string.Format(@"{0}\PKI\CA\rejected", sourcePath);
            if (Directory.Exists(pathReject))
            {
                string[] rejectedCertificates = Directory.GetFiles(@pathReject);
                for (int i = 0; i < rejectedCertificates.Length; i++)
                {

                    FileInfo fileInfo1 = new FileInfo(rejectedCertificates[i]);
                    try
                    {
                        X509Certificate2 certificate2 = new X509Certificate2(fileInfo1.FullName, (string)null, X509KeyStorageFlags.Exportable);
                        if (certificate2 != null)
                        {
                            string filePath2 = CertificateUtils.GetCertificateFileName(certificate2, "der");

                            RejectedValue.Add(filePath2);
                        }
                    }
                    catch (CryptographicException ex)
                    {
                        string errMsg = ex.Message;
                    }
                }
            }
        }

        /// <summary>
        ///  Get import certificate file path.
        /// </summary>
        /// <returns>path to store the certificate</returns>        
        private string ImportFileDlg()
        {
            //get import file path
            const string caption = "Import Certificate";
            // open file dialog.
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.DefaultExt = ".der";
            dialog.Filter = "DER Files (*.der)|*.der|PKCS #12 Files (*.pfx)|*.pfx|All Files (*.*)|*.*";
            dialog.Multiselect = false;
            dialog.ValidateNames = true;
            dialog.Title = caption;
            dialog.FileName = null;
            dialog.RestoreDirectory = true;

            if (!dialog.ShowDialog().Value) return null;
            return dialog.FileName;
        }

        /// <summary>
        /// Export the certificate from trusted/rejected list to required location.
        /// </summary>
        /// <param name="certificateName"></param>
        /// <param name="trust"></param>
        public void exportCertificate(string certificateName,bool trust)
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
                            string filePath = CertificateUtils.GetCertificateFileName(certificate, "der");

                            if(filePath==certificateName) //(certificate.Issuer.Contains("Elpis OPC Server"))
                            {

                                //CertificateListItem obj = trustedListBox.SelectedItem as CertificateListItem;
                                Opc.Ua.UserComponents.Tools.CertificateListItem listItem = new Opc.Ua.UserComponents.Tools.CertificateListItem();
                                List<string> fields = Utils.ParseDistinguishedName(certificate.Subject);
                                //Get Certificate  type
                                for (int j = 0; j < fields.Count; j++)
                                {
                                    if (fields[j].StartsWith("CN="))
                                        listItem.Name = fields[j].Substring("CN=".Length);

                                    if (fields[j].StartsWith("DC="))
                                        listItem.Type = fields[j].Substring("DC=".Length);
                                }
                                //Get Certificate  Name
                                if (String.IsNullOrEmpty(listItem.Name))
                                    listItem.Name = String.Format("{0}", certificate.Subject);

                                fields = Utils.ParseDistinguishedName(certificate.Issuer);

                                //Get Certificate Issuer name
                                for (int j = 0; j < fields.Count; j++)
                                {
                                    if (fields[j].StartsWith("CN="))
                                        listItem.Issuer = fields[j].Substring("CN=".Length);
                                }
                                // determine certificate type.
                                foreach (X509Extension extension in certificate.Extensions)
                                {
                                    X509BasicConstraintsExtension basicContraints = extension as X509BasicConstraintsExtension;

                                    if (basicContraints != null)
                                    {
                                        if (basicContraints.CertificateAuthority)
                                            listItem.Type = "CA";
                                        else
                                            listItem.Type = "End-Entity";

                                        break;
                                    }
                                }

                                // look up domains.
                                IList<string> domains = Utils.GetDomainsFromCertficate(certificate);

                                StringBuilder buffer = new StringBuilder();

                                for (int j = 0; j < domains.Count; j++)
                                {
                                    if (buffer.Length > 0)
                                        buffer.Append(";");

                                    buffer.Append(domains[j]);
                                }

                                //To export the certificate
                                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();

                                string extensions = "der";
                                dialog.CheckFileExists = false;
                                dialog.CheckPathExists = true;
                                dialog.DefaultExt = ".pfx";
                                dialog.Filter = "Certificate Files (*.der)|*.der|All Files (*.*)|*.*";
                                dialog.ValidateNames = true;
                                dialog.Title = "Save Certificate File";
                                dialog.FileName = CertificateUtils.GetCertificateFileName(certificate, extensions);

                                // dialog.ShowDialog();
                                //12 10 2016
                                Nullable<bool> result = dialog.ShowDialog();
                                if (result == false)
                                {
                                    return;
                                }
                                if (!string.IsNullOrEmpty(dialog.FileName) && !Store.ExportCertificate(certificate, dialog.FileName, false, string.Empty))
                                {
                                    //Show error message
                                    System.Windows.MessageBox.Show(Store.ErrorMessage, "Export", MessageBoxButton.OK, MessageBoxImage.Error);
                                    Store.ErrorMessage = string.Empty;
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Certificate Successfully Exported", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Information);
                                }

                            }

                        }
                    }
                    catch (CryptographicException ex)
                    {
                        string errMsg = ex.Message;
                    }
                }

            }
        }

        /// <summary>
        /// Moving selected rejected certificate from rejected list to trusted list to make a trusted certificate.
        /// </summary>
        /// <param name="RejectedValue"></param>
        /// <param name="TrustedValue"></param>
        /// <param name="selectedRejectedCertificate"></param>
        /// <param name="sourcePath"></param>
        public void trustedClick(ObservableCollection<string> RejectedValue, ObservableCollection<string> TrustedValue, string selectedRejectedCertificate, string sourcePath)
        {
            FileInfo fileInfo = new FileInfo(selectedRejectedCertificate);
            try
            {
                //Create certificate from path
                X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
                if (certificate != null)
                {
                    string filePath = CertificateUtils.GetCertificateFileName(certificate, "der");
                    string targetPath = sourcePath + "\\" + Store.StoreName + "\\trusted\\certs";
                    string targetFile = System.IO.Path.Combine(targetPath, filePath);
                    if (!File.Exists(targetFile))
                    {
                        File.Move(selectedRejectedCertificate, targetFile);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("certificate with the same name already exists.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    

                    //clear
                    TrustedValue.Clear();
                    //clear
                    RejectedValue.Clear();

                    //for Trusted Listbox
                    UpdateTrustList();
                    #region trust 
                    //string pathTrust = sourcePath + "\\" + Store.StoreName + "\\certs";
                    //if (Directory.Exists(pathTrust))
                    //{
                    //    string[] trustedCertificates = Directory.GetFiles(@pathTrust);
                    //    for (int i = 0; i < trustedCertificates.Length; i++)
                    //    {

                    //        FileInfo fileInfo1 = new FileInfo(trustedCertificates[i]);
                    //        try
                    //        {
                    //            X509Certificate2 certificate1 = new X509Certificate2(fileInfo1.FullName, (string)null, X509KeyStorageFlags.Exportable);
                    //            // 12 10 2016
                    //            if (certificate1 != null && !certificate1.SubjectName.Name.Contains("Elpis OPC Server"))
                    //            {
                    //                string filePath1 = OPCEngine.CertificateUtils.GetCertificateFileName(certificate1, "der");

                    //                TrustedValue.Add(filePath1);
                    //            }
                    //        }
                    //        catch (CryptographicException ex)
                    //        {
                    //            string errMsg = ex.Message;
                    //        }
                    //    }
                    //}
                    #endregion
                    //for rejected Listbox
                    UpdateRejectedList();
                    #region Reject
                    //string pathReject = sourcePath + "\\" + Store.StoreName + "\\rejected";
                    //if (Directory.Exists(pathReject))
                    //{
                    //    string[] rejectedCertificates = Directory.GetFiles(@pathReject);
                    //    for (int i = 0; i < rejectedCertificates.Length; i++)
                    //    {

                    //        FileInfo fileInfo1 = new FileInfo(rejectedCertificates[i]);
                    //        try
                    //        {
                    //            X509Certificate2 certificate2 = new X509Certificate2(fileInfo1.FullName, (string)null, X509KeyStorageFlags.Exportable);
                    //            if (certificate2 != null)
                    //            {
                    //                string filePath2 = OPCEngine.CertificateUtils.GetCertificateFileName(certificate2, "der");

                    //                RejectedValue.Add(filePath2);
                    //            }
                    //        }
                    //        catch (CryptographicException ex)
                    //        {
                    //            string errMsg = ex.Message;
                    //        }
                    //    }
                    //}
                    #endregion
                }
            }
            catch (CryptographicException ex)
            {
                string errMsg = ex.Message;
            }


        }

        /// <summary>
        /// Maintaining the Certificate in rejected list. Moving selected trusted certificate from trusted list to rejected list to make a untrusted certificate.
        /// </summary>

        public void rejectClick(ObservableCollection<string> RejectedValue, ObservableCollection<string> TrustedValue, string selectedTrustedCertificate, string sourcePath)
        {
            FileInfo fileInfo = new FileInfo(selectedTrustedCertificate);
            try
            {
                //Create certificate from path
                X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
                //12 10 2016
                if (certificate != null)// && !certificate.SubjectName.Name.Contains("Elpis OPC Server"))
                {
                    string filePath = CertificateUtils.GetCertificateFileName(certificate, "der");
                    string targetPath = sourcePath + "\\PKI\\CA\\rejected";
                    string targetFile = System.IO.Path.Combine(targetPath, filePath);
                    if (!File.Exists(targetFile))
                    {
                        try
                        {
                            File.Move(selectedTrustedCertificate, targetFile);
                        }
                        catch (Exception ex)// Display error
                        {
                            System.Windows.MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("certificate with the same name already exists.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    

                    //clear trust list
                    TrustedValue.Clear();
                    //clear reject list
                    RejectedValue.Clear();


                    //update Trusted Listbox
                    UpdateTrustList();
                    #region Trust
                    //string pathTrust = sourcePath + "\\" + Store.StoreName + "\\certs";
                    //if (Directory.Exists(pathTrust))
                    //{
                    //    string[] trustedCertificates = Directory.GetFiles(@pathTrust);
                    //    for (int i = 0; i < trustedCertificates.Length; i++)
                    //    {

                    //        FileInfo fileInfo1 = new FileInfo(trustedCertificates[i]);
                    //        try
                    //        {
                    //            X509Certificate2 certificate1 = new X509Certificate2(fileInfo1.FullName, (string)null, X509KeyStorageFlags.Exportable);
                    //            // 12 10 2016
                    //            if (certificate1 != null && !certificate1.SubjectName.Name.Contains("Elpis OPC Server"))
                    //            {
                    //                string filePath1 = OPCEngine.CertificateUtils.GetCertificateFileName(certificate1, "der");

                    //                TrustedValue.Add(filePath1);
                    //            }
                    //        }
                    //        catch (CryptographicException ex)
                    //        {
                    //            string errMsg = ex.Message;
                    //        }
                    //    }
                    //}
                    #endregion
                    //update rejected Listbox
                    UpdateRejectedList();
                    #region Reject
                    //string pathReject = sourcePath + "\\" + Store.StoreName + "\\rejected";
                    //if (Directory.Exists(pathReject))
                    //{
                    //    string[] rejectedCertificates = Directory.GetFiles(@pathReject);
                    //    for (int i = 0; i < rejectedCertificates.Length; i++)
                    //    {

                    //        FileInfo fileInfo1 = new FileInfo(rejectedCertificates[i]);
                    //        try
                    //        {
                    //            X509Certificate2 certificate2 = new X509Certificate2(fileInfo1.FullName, (string)null, X509KeyStorageFlags.Exportable);
                    //            if (certificate2 != null)
                    //            {
                    //                string filePath2 = OPCEngine.CertificateUtils.GetCertificateFileName(certificate2, "der");

                    //                RejectedValue.Add(filePath2);
                    //            }
                    //        }
                    //        catch (CryptographicException ex)
                    //        {
                    //            string errMsg = ex.Message;
                    //        }
                    //    }
                    //}
                    #endregion
                }
            }
            catch (CryptographicException ex)
            {
                string errMsg = ex.Message;
            }


        }

        //retrive all the existing certificate 
        public void uacertificateload()
        {
            TrustedValue.Clear();
            RejectedValue.Clear();
            //for trusted ListBox
            UpdateTrustList();
            #region Trust
            //string sourcePath = Directory.GetCurrentDirectory();
            //string pathTrust = sourcePath + "\\PKI\\CA\\certs";
            //if (Directory.Exists(pathTrust))
            //{
            //    string[] trustedCertificates = Directory.GetFiles(@pathTrust);
            //    for (int i = 0; i < trustedCertificates.Length; i++)
            //    {

            //        FileInfo fileInfo = new FileInfo(trustedCertificates[i]);
            //        try
            //        {
            //            //Create certificate from path
            //            X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
            //            // 12 10 2016
            //            // if (certificate != null && !certificate.SubjectName.Name.Contains("Elpis OPC Server")) //7/9/17
            //            if (certificate != null && certificate.SubjectName.Name.Contains("Elpis OPC Server"))
            //            {
            //                string filePath = OPCEngine.CertificateUtils.GetCertificateFileName(certificate, "der");
            //                // trustedListBox.ItemsSource = filePath;
            //                TrustedValue.Add(filePath);
            //            }
            //        }
            //        catch (CryptographicException ex)
            //        {
            //            string errMsg = ex.Message;
            //        }
            //    }
            //}
            #endregion
            //for rejected list box
            UpdateRejectedList();
            #region Reject
            //string pathRejected = sourcePath + "\\PKI\\CA\\rejected";
            //if (Directory.Exists(pathRejected))
            //{
            //    string[] rejectedCertificates = Directory.GetFiles(@pathRejected);
            //    for (int i = 0; i < rejectedCertificates.Length; i++)
            //    {

            //        FileInfo fileInfo = new FileInfo(rejectedCertificates[i]);
            //        try
            //        {
            //            //Create certificate from path
            //            X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
            //            if (certificate != null)
            //            {
            //                string filePath = OPCEngine.CertificateUtils.GetCertificateFileName(certificate, "der");
            //                // trustedListBox.ItemsSource = filePath;
            //                RejectedValue.Add(filePath);
            //            }
            //        }
            //        catch (CryptographicException ex)
            //        {
            //            string errMsg = ex.Message;
            //        }
            //    }

            //}
            #endregion
        }

        #endregion End of Method

        #region ImportCertificate
        /// <summary>
        /// Import certificate from given path
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>Returns True on success, Otherwise returns false</returns>      
        public bool ImportCertificate(string path)
        {
            bool returnFlag = false;
            //Store.StorePath = Directory.GetCurrentDirectory();
            //Store.StoreName = "\\PKI\\CA";
            StorePath = Store.StorePath;  // + "\\"+Store.StoreName //TODO remove Concatenation and use stringBuilder
            TrustedCertificatePath = Directory.GetCurrentDirectory() + "\\PKI\\CA\\trusted\\certs";
            //CertificateUtils.StoreFolderCreator(StorePath);
            //Check for import path
            if (string.IsNullOrEmpty(path))
            {
                //Get error message. It will be used by the called method
                Store.ErrorMessage = "Empty Certificate path";
                return returnFlag;
            }
            FileInfo fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                //Get error message. It will be used by the called method
                Store.ErrorMessage = "File does not exist in the path";
                return returnFlag;
            }
            try
            {
                //Create certificate from path
                X509Certificate2 certificate = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
                if (certificate != null)
                {
                    //Get Fiel name
                    string filePath = CertificateUtils.GetCertificateFileName(certificate, "der");

                    //Check for the directory prepares import file path includingfile name
                    if (!Directory.Exists(TrustedCertificatePath))
                        Directory.CreateDirectory(TrustedCertificatePath);
                    string movingPath = Path.Combine(TrustedCertificatePath, filePath);

                    if (File.Exists(movingPath))
                    {
                        Store.ErrorMessage = "Selected file already exists in the trusted store ";
                        return returnFlag;
                    }
                    //Create file using stream
                    using (Stream ostrm = File.Open(movingPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        byte[] data = certificate.RawData;
                        ostrm.Write(data, 0, data.Length);
                    }
                    returnFlag = true;
                }
            }
            catch (CryptographicException ex)
            {
                //Get error message. It will be used by the called method
                Store.ErrorMessage = ex.Message;
            }
            return returnFlag;
        }
        #endregion End of ImportCertificate

    }

    #endregion UACertificateViewModel class
}

#endregion End of UAcertificateEngine NameSpace

#endregion OPCEngine namespace