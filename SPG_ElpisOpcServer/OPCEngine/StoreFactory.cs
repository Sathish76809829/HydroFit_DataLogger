#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua.CertificateManagementLibrary;

#endregion Namespaces

#region OPCEngine namespace
namespace OPCEngine
{
    #region Get StoreFactory store
    public class StoreFactory
    {
        #region Get Certificate store
        /// <summary>
        /// Get certificate store based on the type given
        /// </summary>
        /// <param name="storeType">Store type (Windows/Directory)</param>
        /// <returns>Returns IStore object success based on given store type. If store type doesn't match, returns DirectoryStore as default</returns>
        public static Opc.Ua.CertificateManagementLibrary.IStore GetCertificateStore()
        {
            //Get store
            return new Opc.Ua.CertificateManagementLibrary.DirectoryStore();
        }
        #endregion Get Certificate store
    }
    #endregion Get StoreFactory store
}
#endregion OPCEngine namespace