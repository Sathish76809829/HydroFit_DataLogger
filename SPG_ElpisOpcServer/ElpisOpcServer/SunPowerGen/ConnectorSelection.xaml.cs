using Elpis.Windows.OPC.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Interaction logic for ConnectorSelection.xaml
    /// </summary>
    public partial class ConnectorSelection : Window
    {
        ObservableCollection<IConnector> ConnectorCollection { get; set; }
        internal string ConnectorName { get; set; }
        internal string DeviceName { get; set; }
        public ConnectorSelection()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string projectFilePath = string.Format(@"{0}\opcSunPowerGen.elp", Directory.GetCurrentDirectory());
            ConnectorCollection = null;
            FileHandler FileHandle = new FileHandler();
            if (File.Exists(projectFilePath))
            {
                Stream stream = File.Open(projectFilePath, FileMode.OpenOrCreate);

                BinaryFormatter bformatter = new BinaryFormatter();
                try
                {
                    using (StreamWriter wr = new StreamWriter(stream))
                    {
                        if (FileHandle == null)
                            FileHandle = new FileHandler();
                        if (FileHandle != null)
                        {
                            FileHandle = (FileHandler)bformatter.Deserialize(stream);
                            ConnectorCollection = FileHandle.AllCollectionFileHandling;
                        }
                        wr.Close();
                    }
                    stream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Elpis OPC Server", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                List<string> ConnectorList = ConnectorCollection.Select(c => c.Name).ToList();
                ConnectorList.Insert(0, "--Select--");
                cmbConnectorList.ItemsSource = ConnectorList;
                cmbConnectorList.SelectedIndex = 0;
                // cmbConnectorList.DisplayMemberPath = "ConnectorName";
            }
        }

        private void GridOfWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {

            }
        }

        private void cmbConnectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbConnectorList.SelectedIndex == 0)
                cmbDeviceList.IsEnabled = false;
            else
            {
                cmbDeviceList.IsEnabled = true;
                ConnectorBase connector = ConnectorCollection[cmbConnectorList.SelectedIndex - 1] as ConnectorBase;
                ObservableCollection<DeviceBase> deviceCollection = connector.DeviceCollection;
                List<string> deviceList = deviceCollection.Select(d => d.DeviceName).ToList();
                deviceList.Insert(0, "--Select--");
                cmbDeviceList.ItemsSource = null;
                cmbDeviceList.Items.Clear();
                cmbDeviceList.ItemsSource = deviceList;
                cmbDeviceList.SelectedIndex = 0;
            }
        }

        private void cmbDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.IsLoaded)
            {
                if (cmbDeviceList.SelectedIndex == 0)
                    btnOk.IsEnabled = false;
                else
                    btnOk.IsEnabled = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ConnectorName = cmbConnectorList.SelectedValue.ToString();
            DeviceName = cmbDeviceList.SelectedValue.ToString();
            this.Hide();
        }
    }
}
