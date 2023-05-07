#region Usings
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
#endregion Usings

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// This Class defines all the common properties of devices.
    /// </summary>
    [Serializable()]
    public class DeviceBase : SerializableDependencyObject, INotifyPropertyChanged
    {
        #region Constructor
        /// <summary>
        /// Constructor for Serialization
        /// </summary>
        public DeviceBase() : base()
        {

        }
        /// <summary>
        /// Constructor for Deserialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public DeviceBase(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor
        private string connectorAssignment;
        [ReadOnly(true), Browsable(false), DisplayName("Connector Assignment"), Description("Under which connector this device is connected.")]

        public string ConnectorAssignment
        {
            get
            {
                return connectorAssignment;
            }
            set
            {
                connectorAssignment = value;
                OnPropertyChanged("ConnectorAssignment");
            }
        }
        //public string ConnectorAssignment
        //{
        //    get { return (string)GetValue(ConnectorAssignmentProperty); }
        //    set { SetValue(ConnectorAssignmentProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ProtocolAssignment.
        //// This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ConnectorAssignmentProperty =
        //    DependencyProperty.Register("ConnectorAssignment", typeof(string), typeof(DeviceBase), new PropertyMetadata(null));

        private string deviceName;
        [Description("Specify the identity of the device."), DisplayName("Name*"), PropertyOrder(1)]
        public string DeviceName
        {
            get
            {
                return deviceName;
            }
            set
            {
                deviceName = value;
                OnPropertyChanged("DeviceName");
            }
        }

        //public string DeviceName
        //{
        //    get { return (string)GetValue(DeviceNameProperty); }
        //    set { SetValue(DeviceNameProperty, value); }
        //}
        //// Using a DependencyProperty as the backing store for DeviceName.
        //// This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DeviceNameProperty =
        //    DependencyProperty.Register("DeviceName", typeof(string), typeof(DeviceBase), new PropertyMetadata(null));


        [Description("Provide the brief summary of the device or its use."), DisplayName("Description"), PropertyOrder(2)]
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(DeviceBase), new PropertyMetadata(null));

        private DeviceType deviceType;
        [Browsable(false), DisplayName("Device Type"), Description("Specify type of protocol used to communicate.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]


        public DeviceType DeviceType
        {
            get
            {
                return deviceType;
            }
            set
            {
                deviceType = value;
                OnPropertyChanged("DeviceType");
            }
        }

        //public DeviceType DeviceType 
        //{
        //    get { return (DeviceType)GetValue(DeviceTypeProperty); }
        //    set { SetValue(DeviceTypeProperty, value); }
        //}
        //// Using a DependencyProperty as the backing store for Type.
        //// This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DeviceTypeProperty =
        //    DependencyProperty.Register("DeviceType", typeof(DeviceType), typeof(DeviceBase), new PropertyMetadata(null));


        [Description("Select the specific type of device associated with this ID. Options depend on the type of communications in use."), DisplayName("Model"),PropertyOrder(5)]
        [Browsable(false)]
        public string Model
        {
            get { return (string)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Model.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(string), typeof(DeviceBase), new PropertyMetadata(null));
        #region RetryCount
        private uint retryCount;

        [Browsable(false), DisplayName("Retry Count"), Description("Number of times tries to connect device, when device communication is fails.")]
        public uint RetryCount
        {
            get
            {
                return retryCount;
            }
            set
            {
                retryCount = value;
                OnPropertyChanged("RetryCount");
            }
        }
        #endregion RetryCount
        //public uint RetryCount
        //{
        //    get { return (uint)GetValue(RetryCountProperty); }
        //    set { SetValue(RetryCountProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for RetryCount.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty RetryCountProperty =
        //    DependencyProperty.Register("RetryCount", typeof(uint), typeof(DeviceBase), new PropertyMetadata(null));

        private int retryCounter;
        [Browsable(false), DisplayName("Retry Counter"), Description("Number of times tries to connect device, when device communication is fails.")]

            public int RetryCounter
            {
                get
                {
                    return retryCounter;
                }
                set
                {
                    retryCounter = value;
                    OnPropertyChanged("RetryCounter");
                }
            }

        //private int samplingRate = 100;
        //[Description("Please enter the sampling rate in milliseconds."), DisplayName("SamplingRate(ms)*"), DefaultValue((int)100), PropertyOrder(6)]
        //public int SamplingRate
        //{
        //    get
        //    {
        //        return samplingRate;
        //    }
        //    set
        //    {
        //        samplingRate = value;
        //        OnPropertyChanged("SamplingRate");
        //    }
        //}
        private int samplingRate = 100;
        [Description("Please enter the sampling rate in milliseconds."), DisplayName("SamplingRate(ms)*"), DefaultValue((int)100), PropertyOrder(6)]
        public int SamplingRate
        {
            get { return samplingRate; }
            set
            {
                if (value < 100)
                {
                    samplingRate = 100;

                }
                else if (value > 500)
                {
                    samplingRate = 500;

                }
                else
                {
                    samplingRate = value;

                }
                OnPropertyChanged("SamplingRate");
            }
        }



        //private int[] _samplingFrequencyValues = new int[] { 100, 200, 300, 400, 500 };
        //private int _SamplingRate;

        //public int[] SamplingFrequencyValues
        //{
        //    get { return _samplingFrequencyValues; }
        //    set
        //    {
        //        _samplingFrequencyValues = value;
        //        _SamplingRate = _samplingFrequencyValues[0];
        //    }
        //}
        //[Description("Please enter the sampling rate in milliseconds."), DisplayName("SamplingRate(ms)*"), DefaultValue((int)100), PropertyOrder(6)]
        //public int SamplingRate
        //{
        //    get { return _SamplingRate; }
        //    set
        //    {
        //        if (_samplingFrequencyValues.Contains(value))
        //        {
        //            _SamplingRate = value;
        //        }
        //        else
        //        {
        //            throw new ArgumentException("Value not in SamplingFrequencyValues array.");
        //        }
        //    }
        //}


        //private ComboBox _SamplingReate;
        //[Description("Please enter the sampling rate in milliseconds."), DisplayName("SamplingRateCmb(ms)*"), DefaultValue((int)100), PropertyOrder(7)]
        //public ComboBox SamplindRate
        //{
        //    get { return _SamplingReate; }
        //    set
        //    {
        //        _SamplingReate = value;
        //        _SamplingReate.Items.Add(100);
        //        _SamplingReate.Items.Add(200);
        //        _SamplingReate.Items.Add(300);
        //        _SamplingReate.Items.Add(400);
        //        _SamplingReate.Items.Add(500);
        //    }
        //}


        //private int[] _SamplingReate = new int[] { 100, 200, 300, 400, 500 };
        //[Description("Please enter the sampling rate in milliseconds."), DisplayName("SamplingRateCmb(ms)*"), DefaultValue((int)100), PropertyOrder(7)]
        //public int[] SamplindRate
        //{
        //    get { return _SamplingReate; }
        //    set { _SamplingReate = value; }

        //}
        //private int samplingRate;
        //[Browsable(false), DisplayName("SamplingRate"), Description("the number of samples of a sound that are taken per second to represent the event digitally ")]
        //public int _samplingRate
        //{
        //    get
        //    {
        //        return samplingRate;
        //    }
        //    set
        //    {
        //        samplingRate = value;
        //        OnPropertyChanged("_samplingRate");
        //    }
        //}
        //private List<int> samplingRate = new List<int>() { 100, 200, 300 };

        //public List<int> SamplingRate
        //{
        //    get { return samplingRate; }
        //    set { samplingRate = value; }
        //}
        //private int[] samplingRate = new int[] { 100, 200, 300, 400, 500 };

        //public int SamplingRate
        //{
        //    get { return samplingRate[0]; }
        //    set { samplingRate[0] = value; }
        //}
        //private int[] samplingRate = new int[] { 100, 200, 300, 400, 500 };

        //public int[] SamplingRate1
        //{
        //    get { return samplingRate; }
        //    set { samplingRate = value; }
        //}
        //public int RetryCounter
        //{
        //    get { return (int)GetValue(RetryCounterProperty); }
        //    set { SetValue(RetryCounterProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for RetryCount.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty RetryCounterProperty =
        //    DependencyProperty.Register("RetryCounter", typeof(int), typeof(DeviceBase), new PropertyMetadata(null));


        [Browsable(false)]
        public ObservableCollection<Tag> TagsCollection { get; set; }

        [Browsable(false)]
        public ObservableCollection<TagGroup> GroupCollection { get; set; }

        [Browsable(false)]
        public string Name { get; set; }

        #region Hide Base Class Properties
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
        #endregion

        [Browsable(false)]
        public ObservableCollection<PropertyInfoViewModel> PropertyInfoViewModelCollectionDevices { get; set; }
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
                try
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(Property));
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

            }
        }
    }
}

#endregion OPCEngine namespace