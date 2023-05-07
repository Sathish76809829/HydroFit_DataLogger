#region Namespaces
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion Namespaces

#region namespace OPCEngine
namespace Elpis.Windows.OPC.Server
{
    public class CertificateUtils
    {
        #region Fields

        public static string ErrorMessage { get; set; }

        #endregion Fields

        #region ValidateName
        /// <summary>
        /// Validate the name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool ValidateName(string name)
        {
            name = name.Replace("”", "\"");
            Regex regex = new Regex("^([a-zA-Z0-9][^*/><?\"|:]*)$");
            return regex.IsMatch(name);
        }
        #endregion ValidateName

        #region Folder Creator
        /// <summary>
        /// Directory type Store Folder Creator
        /// </summary>
        public static bool StoreFolderCreator(string storePath, bool isNEW = false)
        {
            bool Validation = true;
            DirectoryStore temDirectoryStore =  new DirectoryStore();
            temDirectoryStore.StorePath = storePath;
            string trustedPath = CertificateUtils.GetCertificatePath(temDirectoryStore, storePath, "Trust");
            string rejectedPath = CertificateUtils.GetCertificatePath(temDirectoryStore, storePath, "Reject");
            string revokedPath = CertificateUtils.GetCertificatePath(temDirectoryStore, storePath, "Revoke");
            string IssuersPath = CertificateUtils.GetCertificatePath(temDirectoryStore, storePath, "Issuers");
            string OwnPath = CertificateUtils.GetCertificatePath(temDirectoryStore, storePath, "Own");
            string PrivatePath = CertificateUtils.GetCertificatePath(temDirectoryStore, storePath, "Private");


            if (string.IsNullOrEmpty(trustedPath) || string.IsNullOrEmpty(rejectedPath) || string.IsNullOrEmpty(revokedPath))
                return Validation;

            if (isNEW)
            {
                Directory.CreateDirectory(trustedPath);
                Directory.CreateDirectory(rejectedPath);
                Directory.CreateDirectory(revokedPath);
                Directory.CreateDirectory(IssuersPath);
                Directory.CreateDirectory(OwnPath);
                Directory.CreateDirectory(PrivatePath);
                return true;
            }

            else
            {
                if (!(Directory.Exists(trustedPath) && Directory.Exists(rejectedPath) && Directory.Exists(revokedPath)))
                    return false;
                else return true;

            }
        }
        #endregion Folder Creator

        #region Get Certificate file path
        /// <summary>
        /// Get certificate path
        /// </summary>
        /// <param name="store">IStore to determine store type </param>
        /// <param name="inputPath">Input path to be get path</param>
        /// <param name="typeofControl">Type of control</param>
        /// <returns>returns string file path on success</returns>
        public static string GetCertificatePath(IStore store, string inputPath, string typeofControl)
        {
            //Get directory path
            string returnString = string.Empty;
            if (store.GetType() == typeof(DirectoryStore))
            {
                DirectoryStore dirStore = store as DirectoryStore;
                //Determine path according to control
                switch (typeofControl)
                {
                    case "Trust":
                        if (dirStore != null)// trusted change
                        {
                            if(!inputPath.Contains("trusted"))
                            {

                                returnString = Path.Combine(inputPath, "trusted", "certs");
                                dirStore.TrustedCertificatePath = dirStore.StorePath + @"\trusted\certs";                               
                            }
                            else
                            {                               
                                returnString = Path.Combine(inputPath, "certs");
                                dirStore.TrustedCertificatePath = dirStore.StorePath + @"\certs";                               
                            }

                        }
                        break;
                    case "Reject":
                        //returnString = Path.Combine(inputPath.Substring(0, inputPath.LastIndexOf("\\")), "rejected");
                        returnString = Path.Combine(inputPath, "rejected");
                        break;
                    case "Revoke":
                        returnString = Path.Combine(inputPath, "crl");
                        //returnString = Path.Combine(inputPath.Substring(0, inputPath.LastIndexOf("\\")), "crl");
                        break;

                    case "Issuers":
                        returnString = Path.Combine(inputPath, "Issuers");
                        //returnString = Path.Combine(inputPath.Substring(0, inputPath.LastIndexOf("\\")), "Issuers");
                        break;
                    case "Own":
                        returnString = Path.Combine(inputPath, "Own");
                        //returnString = Path.Combine(inputPath.Substring(0, inputPath.LastIndexOf("\\")), "Own");
                        break;
                    case "Private":
                        if (dirStore.isLds)
                        {
                           // returnString = Path.Combine(inputPath, "Private");
                            returnString = Path.Combine(inputPath, "Private");
                        }
                           
                        else
                        {
                            //returnString = Path.Combine(inputPath, "private");
                            returnString = Path.Combine(inputPath, "private");
                        }
                           
                        break;
                }
            }
            return returnString;
        }
        #endregion Get Certificate file path

        #region PathExplore
        /// <summary>
        /// Gets file Path of the certificate given
        /// </summary>
        /// <param name="rootFolder">Root folder of certificates</param>
        /// <param name="certificate">Certificate to get the path for</param>
        /// <returns>String path of certificate if found, otherwise returns empty</returns>
        public static string PathExplore(string rootFolder, string thumbPrint, byte[] rawData)
        {
            //Get Root and files at root
            DirectoryInfo root = new DirectoryInfo(rootFolder);
            string filePath = string.Empty;


            //Get files in the directory
            FileInfo[] files = root.GetFiles("*.der");
            if (files != null && files.Length > 0)
            {
                foreach (FileInfo fileInfo in files)
                {
                    try
                    {
                        //Check certificate files by thumbprint
                        X509Certificate2 certificateFile = new X509Certificate2(fileInfo.FullName, (string)null, X509KeyStorageFlags.Exportable);
                        if (thumbPrint == certificateFile.Thumbprint)
                        {
                            filePath = fileInfo.FullName;
                            break;
                        }
                    }
                    catch (CryptographicException)
                    {
                        //Return empty filepath
                        filePath = string.Empty;
                    }
                }
            }
            else
            {
                foreach (string file in Directory.EnumerateFiles(rootFolder))
                {
                    string[] extension = file.Split('.');
                    //Get only .crl files
                    if (extension.Length > 1 && extension[extension.Length - 1] == "crl")
                    {
                        try
                        {
                            //Check certificate files by rawData
                            X509CRL revokeCert = new X509CRL(file);
                            if (revokeCert.RawData.SequenceEqual(rawData))
                            {
                                filePath = file;
                                break;
                            }
                        }
                        catch (CryptographicException)
                        {
                            //Return empty filepath
                            filePath = string.Empty;
                        }
                    }
                }
            }
            return filePath;
        }
        #endregion PathExplore

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
        public static X509Certificate2 IssueCertificate(string storePath, string applicationUri, string applicationName, string subjectName, string sizeItem, string keyItem, string lifeTime, string CAKeyPath, string CAKeyPassword, string[] domainNames, bool isLds, IStore store)
        {
            X509Certificate2 certificate = null;
            string thumbprint = string.Empty;
            try
            {
                //Make sure given values are valid
                if (CertificateUtils.CheckAuthorityRequirements(storePath, applicationName, CAKeyPath, CAKeyPassword))
                {
                    //Issue certificate
                    thumbprint = CertificateUtils.CreateCertificateViaProxy(
                        CertificateUtils.GetExecutablePath(),
                             storePath,
                           string.Empty,
                           applicationUri,
                       applicationName,
                       subjectName,
                       domainNames,
                        Convert.ToUInt16(sizeItem, CultureInfo.CurrentCulture),
                       DateTime.MinValue,
                       ushort.Parse(lifeTime, CultureInfo.CurrentCulture),
                       0,
                       false,
                       keyItem == "PEM",
                       CAKeyPath,
                       CAKeyPassword,
                       isLds,
                       store);
                    certificate = GetCertficateFromStore(store, thumbprint, storePath);

                }
                else
                    ErrorMessage = CertificateUtils.ErrorMessage;

            }
            catch (CryptographicException ex)
            {
                //Catch Exception   
                ErrorMessage = ex.Message;
            }
            return certificate;
        }
        #endregion IssueCertificate

        #region Get Executable Path
        /// <summary>
        /// Get Executable path 
        /// </summary>
        /// <returns>return string executablepath on success, otherwise returns null</returns>
        public static string GetExecutablePath()
        {
            //return executablePath;
            return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Opc.Ua.CertificateGenerator.exe");
        }
        #endregion End Of Get Executable Path

        #region GetCertficateFromStore
        /// <summary>
        /// Get appopriate certificate according to the given thumbprint
        /// </summary>
        /// <param name="store">Store to check certificate from</param>
        /// <param name="thumbprint">Thumbprint to select the certificate</param>
        /// <param name="storePath">StorePath of the store</param>
        /// <returns>returns X509Certificate2 certificate which matches the thumbprint on success, otherwise returns null</returns>
        private static X509Certificate2 GetCertficateFromStore(IStore store, string thumbprint, string storePath)
        {
            X509Certificate2 certificate = null;
            if (store.GetType() == typeof(DirectoryStore))
            {
                DirectoryStore dirStore = store as DirectoryStore;
                // load the new certificate from the store.
                if (string.IsNullOrEmpty(thumbprint)) return null;
                string certPath = CertificateUtils.PathExplore(dirStore.TrustedCertificatePath, thumbprint, null);


                if (string.IsNullOrEmpty(certPath))
                {
                    certPath = CertificateUtils.PathExplore(storePath + "\\certs", thumbprint, null);
                }
                if (string.IsNullOrEmpty(certPath)) return null;
                certificate = new X509Certificate2(certPath);
            }
            else
            {
                var xStore = store.GetCertificates(storePath);
                foreach (X509Certificate2 cert in xStore)
                {
                    if (cert.Thumbprint == thumbprint)
                    {
                        certificate = cert;
                        break;
                    }
                }

            }
            return certificate;
        }

        #endregion GetCertficateFromStore

        #region CheckAuthorityRequirements
        /// <summary>
        /// Check Initial requirements to create authority
        /// </summary>
        /// <param name="storePath">Store path for check</param>
        /// <param name="authorityName">Authority name for check</param>
        /// <param name="caKeyPath">Issuer path for check</param>
        /// <param name="caKeyPassword">Issuer password for check</param>
        /// <returns>Return true on success, otherwise returns false</returns>
        public static bool CheckAuthorityRequirements(string storePath, string authorityName, string caKeyPath, string caKeyPassword)
        {
            bool returnFlag = false;
            ErrorMessage = string.Empty;
            if (!String.IsNullOrEmpty(caKeyPath))
            {
                char[] illeagalChar = Path.GetInvalidFileNameChars();
                if (caKeyPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                {
                    ErrorMessage = "CA key file contains invalid characters";
                    return returnFlag;
                }
                //Check for error condition
                if (string.Compare(Path.GetExtension(caKeyPath), ".pfx") != 0)
                {
                    ErrorMessage = "Please specify the proper key file.";
                    return returnFlag;
                }

                //Check for issuer
                X509Certificate2 issuer = null;
                try
                {
                    issuer = new X509Certificate2(caKeyPath, caKeyPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
                    if (issuer == null) returnFlag = false;
                }
                catch (CryptographicException ex)
                {
                    //Get error message. It will be used by the called method
                    ErrorMessage = ex.Message;
                    return returnFlag;
                }

                //Check for private key
                if (!issuer.HasPrivateKey)
                {
                    //Get error message. It will be used by the called method
                    ErrorMessage = "Issuer certificate does not have a private key.";
                    return returnFlag;
                }

                // determine certificate type.
                foreach (X509Extension extension in issuer.Extensions)
                {
                    X509BasicConstraintsExtension basicContraints = extension as X509BasicConstraintsExtension;
                    if (basicContraints != null && !basicContraints.CertificateAuthority)
                    {
                        //Get error message. It will be used by the called method
                        ErrorMessage = "Certificate cannot be used to issue new certificates.";
                        return returnFlag;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(caKeyPassword))
            {
                ErrorMessage = "Please specify the proper key file.";
                return returnFlag;
            }
            //Check for storepath
            if (String.IsNullOrEmpty(storePath))
            {
                //Get error message. It will be used by the called method
                ErrorMessage = "Please specify a store path.";
                return returnFlag;
            }
            //Check for authority name
            if (String.IsNullOrEmpty(authorityName) || !ValidateName(authorityName))
            {
                //Get error message. It will be used by the called method
                ErrorMessage = "Please specify a valid name.";
                return returnFlag;
            }
            return true;
        }
        #endregion CheckAuthorityRequirements

        #region ChangeSubjectNameDelimiter
        /// <summary>
        /// Changes the delimiter used to seperate fields in a subject name.
        /// </summary>
        private static string ChangeSubjectNameDelimiter(string name, char delimiter)
        {
            StringBuilder buffer = new StringBuilder();
            List<string> elements = Utils.ParseDistinguishedName(name);

            for (int ii = 0; ii < elements.Count; ii++)
            {
                string element = elements[ii];
                if (buffer.Length > 0)
                    buffer.Append(delimiter);

                if (element.IndexOf(delimiter) != -1)
                {
                    int index = element.IndexOf('=');
                    buffer.Append(element.Substring(0, index + 1));

                    if (element.Length > index + 1 && element[index + 1] != '"')
                        buffer.Append('"');

                    buffer.Append(element.Substring(index + 1));

                    if (element.Length > 0 && element[element.Length - 1] != '"')
                        buffer.Append('"');

                    continue;
                }
                buffer.Append(elements[ii]);
            }
            return buffer.ToString();
        }
        #endregion ChangeSubjectNameDelimiter

        #region CreateCertificateViaProxy
        /// <summary>
        /// Creates the certificate via a proxy instead of calling the CryptoAPI directly.
        /// </summary>
        /// <param name="executablePath">The executable path.</param>
        /// <param name="storePath">The store path.</param>
        /// <param name="password">The password used to protect the certificate.</param>
        /// <param name="applicationUri">The application URI.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="subjectName">Name of the subject.</param>
        /// <param name="domainNames">The domain names.</param>
        /// <param name="keySize">Size of the key.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="lifetimeInMonths">The lifetime in months.</param>
        /// <param name="hashSizeInBits">The hash size in bits.</param>
        /// <param name="isCA">if set to <c>true</c> if creating a certificate authority.</param>
        /// <param name="usePEMFormat">if set to <c>true</c> the private ket is store in the PEM format.</param>
        /// <param name="issuerKeyFilePath">The path to the PFX file containing the CA private key.</param>
        /// <param name="issuerKeyFilePassword">The  password for the PFX file containing the CA private key.</param>
        /// <param name="isLds">Determining the folder structure for the store</param>
        /// <returns>returns certificate thumbprint on success, otherwise it returns null.</returns>
        private static string CreateCertificateViaProxy(string executablePath,
            string storePath,
            string password,
            string applicationUri,
            string applicationName,
            string subjectName,
            IList<String> domainNames,
            ushort keySize,
            DateTime startTime,
            ushort lifetimeInMonths,
            ushort hashSizeInBits,
            bool isCA,
            bool usePEMFormat,
            string issuerKeyFilePath,
            string issuerKeyFilePassword,
            bool isLds,
            IStore store)
        {

            FileInfo filePath = new FileInfo(executablePath);

            if (!filePath.Exists)
            {
                ErrorMessage = string.Format("Cannnot find the Opc.Ua.CertificateGenerator utility: {0}", executablePath);
                return null;
            }
            if (store.GetType() == typeof(DirectoryStore))
                storePath = Utils.GetAbsoluteDirectoryPath(storePath, true, false, true);

            if (storePath == null)
            {
                ErrorMessage = string.Format("Certificate store does not exist: {0}", storePath);
                return null;
            }

            // reconstruct name using slash as delimeter.
            subjectName = ChangeSubjectNameDelimiter(subjectName, '/');

            string tempFile = Path.GetTempFileName();
            try
            {
                StreamWriter writer = new StreamWriter(tempFile);

                writer.WriteLine("-cmd issue", storePath);

                if (!String.IsNullOrEmpty(storePath))
                    writer.WriteLine("-storePath {0}", storePath);
                if (!String.IsNullOrEmpty(applicationName))
                    writer.WriteLine("-applicationName {0} ", applicationName);

                if (!String.IsNullOrEmpty(subjectName))
                    writer.WriteLine("-subjectName {0}", subjectName);

                if (!String.IsNullOrEmpty(password))
                    writer.WriteLine("-password {0}", password);

                if (!isCA)
                {
                    if (!String.IsNullOrEmpty(applicationUri))
                        writer.WriteLine("-applicationUri {0}", applicationUri);

                    if (domainNames != null && domainNames.Count > 0)
                    {
                        StringBuilder buffer = new StringBuilder();

                        for (int ii = 0; ii < domainNames.Count; ii++)
                        {
                            if (buffer.Length > 0)
                                buffer.Append(",");

                            buffer.Append(domainNames[ii]);
                        }

                        writer.WriteLine("-domainNames {0}", buffer.ToString());
                    }
                }


                writer.WriteLine("-keySize {0}", keySize);

                if (startTime > DateTime.MinValue)
                    writer.WriteLine("-startTime {0}", startTime.Ticks - new DateTime(1601, 1, 1).Ticks);

                writer.WriteLine("-lifetimeInMonths {0}", lifetimeInMonths);
                writer.WriteLine("-hashSize {0}", hashSizeInBits);

                if (isCA)
                    writer.WriteLine("-ca true");

                if (usePEMFormat)
                    writer.WriteLine("-pem true");

                if (!String.IsNullOrEmpty(issuerKeyFilePath))
                    writer.WriteLine("-issuerKeyFilePath {0}", issuerKeyFilePath);

                if (!String.IsNullOrEmpty(issuerKeyFilePassword))
                    writer.WriteLine("-issuerKeyPassword {0}", issuerKeyFilePassword);

                if (isLds)
                    writer.WriteLine("-IsLds true");
                writer.WriteLine("");
                writer.Close();

                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.RedirectStandardInput = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = filePath.FullName;
                process.StartInfo.Arguments = "-f \"" + tempFile + "\"";
                process.StartInfo.WorkingDirectory = filePath.DirectoryName;

                process.Start();
                process.WaitForExit();

                string result = null;
                string thumbprint = null;

                StreamReader reader = new StreamReader(tempFile);


                try
                {
                    while ((result = reader.ReadLine()) != null)
                    {
                        if (String.IsNullOrEmpty(result)) continue;

                        if (result.StartsWith("-cmd"))
                            ErrorMessage = "Input file was not processed properly.";

                        if (result.StartsWith("-thumbprint"))
                        {
                            thumbprint = result.Substring("-thumbprint".Length).Trim();
                            break;
                        }

                        if (result.StartsWith("-error"))
                            ErrorMessage = result;
                    }
                }

                finally
                {
                    reader.Close();
                }
                return thumbprint;
            }
            catch (Exception e)
            {
                throw new ServiceResultException("Could not create a certificate via a proxy: " + e.Message, e);
            }
            finally
            {
                if (tempFile != null)
                {
                    try { File.Delete(tempFile); }
                    catch (Exception ex) { ErrorMessage = ex.Message; }
                }
            }
        }
        #endregion  CheckAuthorityRequirements

        #region Get certificate file name
        /// <summary>
        /// Get certificate file name
        /// </summary>
        /// <param name="certificate">Certificate to extract file name</param>
        /// <param name="extension">Certificate extension</param>
        /// <returns>Returns certificate filepath on success, , otherwise returns null</returns>
        public static string GetCertificateFileName(X509Certificate2 certificate, string extension)
        {
            string displayName = null;

            //Get name element
            foreach (string element in Utils.ParseDistinguishedName(certificate.Subject))
            {
                if (element.StartsWith("CN="))
                {
                    displayName = element.Substring(3);
                    break;
                }
            }
            StringBuilder filePath = new StringBuilder();

            if (!String.IsNullOrEmpty(displayName))
            {
                filePath.Append(displayName);
                filePath.Append(" ");
            }
            filePath.Append("[");
            filePath.Append(certificate.Thumbprint);
            string extended = string.Format("].{0}", extension);
            filePath.Append(extended);
            return filePath.ToString();
        }
        #endregion End of Get certificate file name

    }
}
#endregion namespace OPCEngine