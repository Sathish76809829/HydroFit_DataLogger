#region Usings
using Opc.Ua.CertificateManagementLibrary;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
#endregion Usings

#region OPCEngine namespace

namespace Elpis.Windows.OPC.Server
{
    public class DirectoryStore : IStore
    {
        #region Variable Memebers and Properties
        private string storePath = string.Empty;

        public string TrustedCertificatePath { get; set; }
        public bool isLds { get; set; }
        public bool HasPrivateKey { get; set; }
        public string ErrorMessage { get; set; }

        public X509Certificate2 Authority { get; set; }

        public string StoreName { get; set; }

        public string StorePath
        {
            get { return storePath; }
            set
            {
                storePath = value;
            }
        }

        #endregion Variable Memebers and Properties

        #region CheckPath
        /// <summary>
        /// Check the whether path exists
        /// </summary>
        /// <returns>Return true if storepath directory exists, otherwise return fasle</returns>
        public bool CheckPath()
        {
            return Directory.Exists(StorePath);
        }
        #endregion End of CheckPath

        #region TrustCertificate
        /// <summary>
        /// Move certificate in trusted store
        /// </summary>
        /// <param name="certificate">Type of X509Certificate2, Certificate to be trusted</param>
        /// <returns>Returns True on success otherwise returns false</returns>
        public bool TrustCertificate(X509Certificate2 certificate)
        {
            bool returnFlag = false;
            CertificateUtils.StoreFolderCreator(StorePath);
            string rejectedPath = string.Empty;
            if (string.IsNullOrEmpty(StorePath))
            {
                ErrorMessage = "Path is empty";
                return returnFlag;
            }
            //Get rejected store
            if (!HasPrivateKey)
                rejectedPath = CertificateUtils.PathExplore(Path.Combine(StorePath, "rejected"), certificate.Thumbprint, null);
            else//else Get Revoked store
                rejectedPath = CertificateUtils.PathExplore(Path.Combine(StorePath, "crl"), certificate.Thumbprint, null);

            //Get trusted store.
            string[] pathString = rejectedPath.Split('\\');
            string trustedStoreFilePath = Path.Combine(TrustedCertificatePath, pathString[pathString.Length - 1]);
            try
            {
                //Check file exists in the destination folder
                if (File.Exists(trustedStoreFilePath))
                {
                    ErrorMessage = "Selected file already existed in the Trusted folder";
                    return returnFlag;
                }
                //Move certificate from rejected or revoked folder to trusted folder
                File.Move(rejectedPath, trustedStoreFilePath);
                returnFlag = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                //Get error message. It will be used by the called method
                ErrorMessage = ex.Message;
            }
            catch (ArgumentException ex)
            {
                //Get error message. It will be used by the called method
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                //Get error message. It will be used by the called method
                ErrorMessage = ex.Message;
            }
            return returnFlag;
        }
        #endregion End of TrustCertificate

        #region RejectCertificate
        /// <summary>
        ///  Move certificate to rejected store
        /// </summary>
        /// <param name="certificate">Type of X509Certificate2, Certificate to be Rejected</param>
        /// <returns>Returns True on success, otherwise returns false</returns>
        public bool RejectCertificate(X509Certificate2 certificate)
        {
            //Check for path
            bool returnFlag = false;
            CertificateUtils.StoreFolderCreator(StorePath);
            if (string.IsNullOrEmpty(StorePath))
            {
                ErrorMessage = "Path is empty";
                return returnFlag;
            }
            //Get Trusted path
            string filePath = CertificateUtils.PathExplore(TrustedCertificatePath, certificate.Thumbprint, null);
            string[] pathString = filePath.Split('\\');
            if (pathString.Length < 1)
            {
                ErrorMessage = "Could not able to create trusted store";
                return returnFlag;
            }
            string rejectedStorePath = Path.Combine(StorePath, "rejected");
            //Get Rejected path            
            rejectedStorePath = Path.Combine(StorePath, "rejected", pathString[pathString.Length - 1]);

            try
            {
                if (File.Exists(rejectedStorePath))
                {
                    //Get error message. It will be used by the called method
                    ErrorMessage = "Selected file already exists in the rejected folder";
                    return false;
                }
                //Move to rejected  folder
                File.Move(filePath, rejectedStorePath);
                returnFlag = true;
            }
            catch (Exception ex)
            {
                //Get error message. It will be used by the called method
                ErrorMessage = ex.Message;
            }
            return returnFlag;
        }
        #endregion End of RejectCertificate

        #region IssueCertificate
        /// <summary>
        /// Issue certificate public key
        /// </summary>
        /// <param name="storePath">Path of the store</param>
        /// <param name="applicationUri">Application URI string</param>
        /// <param name="applicationName">Name of the application</param>
        /// <param name="subjectName">Subject Name</param>
        /// <param name="sizeItem">Size of an Item</param>
        /// <param name="keyItem">Key item string</param>
        /// <param name="lifeTime">Life time of certificate</param>
        /// <param name="CAKeyPath">Issuing Authority path</param>
        /// <param name="CAKeyPassword">Password for Issuing Authority</param>
        /// <param name="domainNames">Domain name array</param>
        /// <returns>Returns True on success</returns>  
        public bool IssueCertificate(string storePath, string applicationUri, string applicationName, string subjectName, string sizeItem, string keyItem, string lifeTime, string CAKeyPath, string CAKeyPassword, string[] domainNames)
        {
            bool returnFlag = false;
            CertificateUtils.StoreFolderCreator(StorePath);
            Authority = CertificateUtils.IssueCertificate(StorePath, applicationUri, applicationName, subjectName, sizeItem, keyItem, lifeTime, CAKeyPath, CAKeyPassword, domainNames, isLds, this);
            if (Authority != null)
                returnFlag = true;
            else
                ErrorMessage = CertificateUtils.ErrorMessage;
            return returnFlag;
        }
        #endregion End of IssueCertificate

        #region GetCertificates
        /// <summary>
        /// Get Certificate from path
        /// </summary>
        /// <param name="path">Path to get certificates from</param>
        /// <returns>Return X509Certificate2Collection on success</returns>
        public X509Certificate2Collection GetCertificates(string path)
        {
            X509Certificate2Collection returnCollection = new X509Certificate2Collection();
            if (Directory.Exists(path))
            {
                //Get all files
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    string[] extension = file.Split('.');
                    //Get only .der files
                    if (extension.Length > 1 && extension[extension.Length - 1] == "der")
                    {
                        //Create certifate
                        X509Certificate2 certificate = new X509Certificate2(file);
                        //Check whether private key available or not and add to collection
                        CertificateKey privateKeyStatusObject = GetPrivateKeyStatus(certificate, path);
                        if (privateKeyStatusObject != null && privateKeyStatusObject.CertificateWithPrivateKey != null)
                            returnCollection.Add(privateKeyStatusObject.CertificateWithPrivateKey);
                        else
                            returnCollection.Add(certificate);
                    }
                }
            }
            return returnCollection;
        }
        #endregion GetCertificates

        #region GetPrivateKeyStatus
        /// <summary>
        /// Check whether private key existing in "private" folder check for the access for the given certificate
        /// </summary>
        /// <param name="certificate">Type of X509Certificate2 to which private file to be found</param>
        /// <returns>If success, returns CertificateKey object. Otherwise returns null </returns>
        public CertificateKey GetPrivateKeyStatus(X509Certificate2 certificate, string path)
        {
            CertificateKey certificateKey = new CertificateKey();
            if (certificate != null)
            {
                //Check certificate availability in private folder
                string rootPath = Directory.GetParent(path).FullName;
                string privateKeyPath = string.Empty;
                //Get private key path
                if (Directory.Exists(Path.Combine(StorePath, "Private"))) privateKeyPath = Path.Combine(StorePath, "Private");

                else privateKeyPath = Path.Combine(StorePath, "private");

                if (Directory.Exists(privateKeyPath))
                {
                    //Get certificate path
                    string filePath = CertificateUtils.PathExplore(path, certificate.Thumbprint, null);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        //Check whether private key has access or not
                        string privateFilePath = Path.Combine(privateKeyPath, string.Concat(Path.GetFileNameWithoutExtension(filePath), ".pfx"));
                        if (File.Exists(privateFilePath))
                        {
                            certificateKey.PrivateKeyFile = new FileInfo(privateFilePath);
                            try
                            {
                                //Get private file
                                X509Certificate2 certificatePrivate = new X509Certificate2(
                                    privateFilePath,
                                    new System.Security.SecureString(),
                                    X509KeyStorageFlags.Exportable);

                                if (certificatePrivate.HasPrivateKey)
                                {
                                    //Load private key to the certificate
                                    certificateKey.CertificateWithPrivateKey = certificatePrivate;
                                }
                            }
                            catch (CryptographicException ex)
                            {
                                //Get exception message
                                ErrorMessage = ex.Message;
                                return null;
                            }
                        }
                    }
                }
            }
            return certificateKey;
        }
        #endregion GetPrivateKeyStatus

    }
}
#endregion OPCEngine namespace