#region Using
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
#endregion Using

#region OPCEngine namespace 

namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// This ConnectorBase Class used to save the connectors properties and it's common for all type of Connectors.
    /// </summary>
    [Serializable]
    public class ConnectorBase : SerializableDependencyObject, INotifyPropertyChanged
    {        
        public ConnectorBase():base()
        {

        }
        public ConnectorBase(SerializationInfo info, StreamingContext context):base(info,context)
        {
            
        }
        private string connectorName;
        [DisplayAttribute(Description="Specify the unique name of the Connector.", Name="Name",Order =1),Required(AllowEmptyStrings =false,ErrorMessage ="ConnectorName Required") ]
        public string ConnectorName
        {
            get
            {
                return connectorName;
            }
            set
            {
                //connectorName = "Ethernet";
                connectorName = value;
                OnPropertyChanged("ConnectorName");
            }
        }
        //public string ConnectorName
        //{
        //    get { return (string)GetValue(ConnectorNameProperty); }
        //    set { SetValue(ConnectorNameProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ConnectorName. 
        //// This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ConnectorNameProperty =
        //    DependencyProperty.Register("ConnectorName", typeof(string), typeof(ConnectorBase), new PropertyMetadata(null));

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("just cast me to avoid all this hiding...", true)]
        [Description(@"Specify the brief summary of the Connector"), DisplayName("Description"), PropertyOrder(2)]
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(ConnectorBase), new PropertyMetadata(null));

        private ConnectorType typeofConnector;
        [ReadOnly(true), DisplayName("Connector Type"), Description("Specify the Connector type used for communication.")]

        public ConnectorType TypeofConnector
        {
            get
            {
                return typeofConnector;
            }
            set
            {
                typeofConnector = value;
            }
        }
        //public ConnectorType TypeofConnector
        //{
        //    get { return (ConnectorType)GetValue(TypeofConnectorProperty); }
        //    set { SetValue(TypeofConnectorProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for TypeofConnector.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty TypeofConnectorProperty =
        //    DependencyProperty.Register("TypeofConnector", typeof(ConnectorType), typeof(ConnectorBase), new PropertyMetadata(null));

               
        [Browsable(false),Description("Choose how to send the invalid floating-point numbers to the client."), DisplayName("Invalid Floating Point Value")]
        public InvalidFloatValues InvalidFloatingPointValue
        {
            get { return (InvalidFloatValues)GetValue(InvalidFloatingPointValueProperty); }
            set { SetValue(InvalidFloatingPointValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvalidFloatingPointValue.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvalidFloatingPointValueProperty =
            DependencyProperty.Register("InvalidFloatingPointValue", typeof(InvalidFloatValues), typeof(ConnectorBase), new PropertyMetadata(null));

       
        //[Description("Specify the Name of the network adapter to bind. By selecting \"default\" let OS select the default adapter."), DisplayName("Network Adapter")]
        //public NetworkAdaptor NetworkAdaptor
        //{
        //    get { return (NetworkAdaptor)GetValue(NetworkAdaptorProperty); }
        //    set { SetValue(NetworkAdaptorProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for NetworkAdaptor.
        //// This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty NetworkAdaptorProperty =
        //    DependencyProperty.Register("NetworkAdaptor", typeof(NetworkAdaptor), typeof(ConnectorBase), new PropertyMetadata(null));

        [Browsable(false)]
        public ObservableCollection<DeviceBase> DeviceCollection { get; set; }

        #region Hiding Dependency Object Properties
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public virtual new bool IsSealed
        {
            get
            {
                return base.IsSealed;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public virtual new DependencyObjectType DependencyObjectType
        {
            get
            {
                return base.DependencyObjectType;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public virtual new Dispatcher Dispatcher
        {
            get
            {
                return base.Dispatcher;
            }
        }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("ConnectorName", ConnectorName,typeof(string));
        //    info.AddValue("ConnectorType", ConnectorType, typeof(ConnectorType));
        //    info.AddValue("DeviceCollection", DeviceCollection, typeof(ObservableCollection<IDevice>));

        //}
        #endregion Hiding Dependency Object Properties              

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
                try
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(Property));
                }
                catch (Exception)
                {

                }

            }
        }


    } //End of ConnectorBase Class
} // End of Namespace Elpis.Windows.OPC.Server

#endregion OPCEngine namespace
