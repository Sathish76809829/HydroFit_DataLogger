using Elpis.Windows.OPC.Server;
using OPCEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for NewUser.xaml
    /// </summary>
    public partial class AddNewUserUI : Window
    {
        #region Private Field

        public UserAuthenticationViewModel userAuthViewModel { get; set; }
        public bool flag { get; set; }
        string oldName { get; set; }
        string oldPassword { get; set; }

        #endregion End Of Private Field

        public AddNewUserUI()
        {
            InitializeComponent();
            
            userAuthViewModel = new UserAuthenticationViewModel();
            NewUserPropertyGrid.SelectedObject = userAuthViewModel;
        }
        public AddNewUserUI(UserAuthenticationViewModel user)
        {
            InitializeComponent();
            userAuthViewModel = user;
            NewUserPropertyGrid.SelectedObject = user;
            oldName = user.UserName;
            oldPassword = user.Password;
        }

        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            //userAuthViewModel.UserName = NewUserPropertyGrid.SelectedObject;
            //userAuthViewModel.Password = passwordTxt.Text;
            //userAuthViewModel.ConfirmPassWord = confirmPasswordTxt.Text;
            //userLoad();
            UserAuthenticationViewModel user = NewUserPropertyGrid.SelectedObject as UserAuthenticationViewModel;

            userAuthViewModel.UserName = user.UserName;
            userAuthViewModel.Password = user.Password;
            userAuthViewModel.ConfirmPassWord = user.ConfirmPassWord;
            flag = true;
            if (userAuthViewModel.UserName == null || userAuthViewModel.UserName == "")
            {
                MessageBox.Show("Please Enter User details");

            }
            if (userAuthViewModel.Password != null && userAuthViewModel.Password != "")
            {
                if (userAuthViewModel.Password == userAuthViewModel.ConfirmPassWord)
                {

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Passwords and Conform Password fields are not matching.");
                }
            }
            else
            {
                MessageBox.Show("Passwords or Conform Password fields are not empty.");
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            userAuthViewModel.UserName = oldName;
            userAuthViewModel.Password = oldPassword;
            userAuthViewModel.ConfirmPassWord = oldPassword;
            flag = true;
            this.Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs  e)
        {
            if (flag == false) { userAuthViewModel = null; }
        }


        //private void radPropertyGrid1_EditorInitialized(object sender, PropertyGridItemEditorInitializedEventArgs e)
        //{
        //    if (((PropertyGridItem)e.Item).Name == "PasswordProperty")
        //    {
        //        BaseTextBoxEditorElement element = ((PropertyGridTextBoxEditor)e.Editor).EditorElement as BaseTextBoxEditorElement;
        //        element.PasswordChar = '*';
        //    }
        //}
    }
}
