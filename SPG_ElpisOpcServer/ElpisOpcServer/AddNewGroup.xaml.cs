#region Usings
using System.Windows;
using System.Windows.Media;
using Elpis.Windows.OPC.Server;
using System.Text;
#endregion Usings

namespace ElpisOpcServer
{
    /// <summary>
    /// Interaction logic for AddNewGroup.xaml
    /// </summary>
    public partial class AddNewGroup : Window
    {
        private bool flag { get; set; }
        private TagGroup tagGroup { get; set; }
        public AddNewGroup(dynamic protocol)
        {
            InitializeComponent();
            tagGroup = new TagGroup();
            TagGroupPropertGrid.SelectedObject = tagGroup;
            cmboxDevice.ItemsSource = protocol.DeviceCollection;
            cmboxDevice.DisplayMemberPath = "DeviceName";
           // cmboxDevice.SelectedIndex=
        }

        public AddNewGroup(ConnectorBase connector, object dataContext) 
        {
            InitializeComponent();
            tagGroup = new TagGroup();
            TagGroupPropertGrid.SelectedObject = tagGroup;
            cmboxDevice.ItemsSource = connector.DeviceCollection;
            cmboxDevice.DisplayMemberPath = "DeviceName";
            
            DeviceBase device = dataContext as DeviceBase;

            int index = cmboxDevice.Items.IndexOf(device);

            if (index < 0)
                cmboxDevice.IsEnabled = true;
            cmboxDevice.SelectedIndex = index;
        }

        private void FinishBtn_click(object sender, RoutedEventArgs e)
        {
            dynamic currentObject = TagGroupPropertGrid.SelectedObject as object;
            if (TagGroupPropertGrid.SelectedObjectType.Name.ToString() == "TagGroup")
            {
                if (currentObject.GroupName != null)
                {
                    bool validName = Util.CheckValidName(tagGroup.GroupName);
                    if (validName )
                    {                        
                        if (cmboxDevice.SelectedIndex != -1)
                        {
                            if (tagGroup.GroupName.ToLower() == "none")
                            {
                                StringBuilder message = new StringBuilder();
                                message.AppendLine(@"Group Name cannot contains 'None'.");
                                message.AppendLine("The Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.");
                                MessageBox.Show(message.ToString(), "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                flag = true;
                                this.Hide();
                            }
                            
                        }
                        else
                        {
                            MessageBox.Show("Please Select Device Name from the list to create a Tag Group.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid Tag Group Name.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Tag Group Name will not be empty.\nThe Group Name will contain only [a-z, A-Z, 0-9, _] and Maximum Length is 40 characters without space characters.", "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelBtn_click(object sender, RoutedEventArgs e)
        {
            tagGroup = null;
            this.Close();
            CancelBtn.Background = Brushes.Red;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (flag == false)
            {
                tagGroup = null;
            }
            CancelBtn.Background = Brushes.Red;
        }
    }
}
