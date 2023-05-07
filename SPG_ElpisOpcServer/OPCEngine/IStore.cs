#region Namespaces

using System.Security.Cryptography.X509Certificates;

#endregion Namespaces

#region OPCEngine namespace

namespace Elpis.Windows.OPC.Server
{
    //TODO: its required?
    #region IStore interface
    public interface IStore
    {
        string StoreName { get; set; }
        string StorePath { get; set; }
        bool CheckPath();
        bool TrustCertificate(X509Certificate2 certificate);
        bool RejectCertificate(X509Certificate2 certificate);
        bool IssueCertificate(string storePath, string applicationUri, string applicationName, string subjectName, string sizeItem, string keyItem, string lifeTime, string CAKeyPath, string CAKeyPassword, string[] domainNames);
        X509Certificate2Collection GetCertificates(string path);
    }
    #endregion IStore interface
}

#endregion OPCEngine namespace