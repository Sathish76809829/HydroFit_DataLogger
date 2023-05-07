#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

#endregion Namespaces

#region OPCEngine namespace

namespace Elpis.Windows.OPC.Server
{
    [Serializable()]
    
    public class UserAuthenticationViewModel
    {
        [Description("Specify the User Name")]
        [DisplayName("User Name"), PropertyOrder(1)]
        public string UserName { get; set; }

        [Description("Specify the Password")]
        [PasswordPropertyText(true)]
        [DisplayName("Password"),PropertyOrder(2)]
        public string Password { get; set; }

        [Description("Password and confirm password should be equal")]
        [PasswordPropertyText(true)]
        [DisplayName("Confirm Password"), PropertyOrder(3)]
        public string ConfirmPassWord { get; set; }

    }
}

#endregion OPCEngine namespace