#region Namespaces
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    #region Tag Class
    /// <summary>
    /// This Class defines all the properties of tag.
    /// </summary>
    [Serializable(), DisplayName("Tag")]
    public class Tag : SerializableDependencyObject, ITag, INotifyPropertyChanged
    {
        #region Constructor       
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public Tag() : base()
        {

        }
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public Tag(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
        #endregion Constructor

        #region Variable Members and Properties

        private string tagName;
        private string address;
        private ChannelType _channelType;
        private DataType datatype;
        private DataAccess dataAccessRight;
        
        #region TagName UnderConcat
        [Description("Specify the identity of the tag."), DisplayName("Channel Name *"), PropertyOrder(1)]
        public string TagName
        {
            get
            {
                if (BlockType == BlockTypes.None)
                {
                    return tagName;
                }
                else
                {
                    return $"{tagName}_{BlockType}";
                }
            }
            set
            {
                int underscoreIndex = value.LastIndexOf("_");
                if (underscoreIndex >= 0 && Enum.TryParse<BlockTypes>(value.Substring(underscoreIndex + 1), out BlockTypes blockType))
                {
                    tagName = value.Substring(0, underscoreIndex);
                    BlockType = blockType;
                }
                else
                {
                    tagName = value;
                    BlockType = BlockTypes.None;
                }

                OnPropertyChanged("TagName");
            }
        }
        #endregion TagName UnderConcat
        //private string tagName;//= "TagName";
        //private BlockTypes blockType = BlockTypes.Block1;
        //[Description("Specify the identity of the tag."), DisplayName("Channel Name *"), PropertyOrder(1)]
        //public string TagName
        //{
        //    get
        //    {  
        //        if(blockType == BlockTypes.Block1)
        //        {
        //            return tagName;
        //        }
        //        else
        //        {
        //            return $"{tagName}_{blockType}";
        //        }
        //        //return $"{tagName}_{blockType}";
        //    }
        //    set
        //    {
        //        int underscoreIndex = value.LastIndexOf("_");
        //        if (underscoreIndex >= 0 && Enum.TryParse<BlockTypes>(value.Substring(underscoreIndex + 1), out BlockTypes newBlockType))
        //        {
        //            tagName = value.Substring(0, underscoreIndex);
        //            blockType = newBlockType;
        //        }
        //        else
        //        {
        //            tagName = value;
        //            blockType = BlockTypes.Block1;
        //        }

        //        OnPropertyChanged("TagName");
        //    }
        //}
        #region simple TagName without concat block_type
        //[Description("Specify the identity of the tag."), DisplayName("Name *"), PropertyOrder(1)]
        //public string TagName
        //{
        //    get
        //    {
        //        return tagName;
        //    }
        //    set
        //    {
        //        tagName = value;
        //        OnPropertyChanged("TagName");
        //    }
        //}
        //public string TagName
        //{
        //    get { return (string)GetValue(TagNameProperty); }
        //    set { SetValue(TagNameProperty, value); }
        //}
        #endregion simple TagName without concat block_type
        //// Using a DependencyProperty as the backing store for TagName.
        //// This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty TagNameProperty =
        //    DependencyProperty.Register("TagName", typeof(string), typeof(Tag), new PropertyMetadata(null));
        
       

        [Description("Provide the brief summary of the tag or its use."), DisplayName("Description"), PropertyOrder(2)]
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Tag), new PropertyMetadata(null));
        [Description("Specify the channeltype"),DisplayName("ChannelType *"), Browsable(false),PropertyOrder(3)]
        public ChannelType ChannelType
        {
            set
            {
                _channelType = value;
                OnPropertyChanged("ChannelType");
            }
            get
            {
                return _channelType;
            }
        }
        private int[] adcValues = new int[] { 100, 200, 300, 400, 500 };
        private int[] rpmValues = new int[] { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330, 360 };

        // public ChannelType SelectedChannel { get; set; }


        //public string Address
        //{
        //    get { return address; }
        //    set
        //    {
        //        if (ChannelType == ChannelType.ADC)
        //        {
        //            int inputValue;
        //            if (int.TryParse(value, out inputValue) && adcValues.Contains(inputValue))
        //            {
        //                address = value;
        //            }
        //        }
        //        else if (ChannelType == ChannelType.RPM)
        //        {
        //            int inputValue;
        //            if (int.TryParse(value, out inputValue) && rpmValues.Contains(inputValue))
        //            {
        //                address = value;
        //            }
        //        }
        //    }
        //}
        [Description("Specify the address of the tag at device."), DisplayName("Channel Address *"), PropertyOrder(4)]
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }
        private string channelNo;
        [DisplayName("Channel No *"),Browsable(false), Description("Specify the ChannelNo"),PropertyOrder(1)]
        public string ChannelNo
        {
            get
            {
                return channelNo;
            }
            set
            {
                channelNo = value;
                OnPropertyChanged("ChannelNo");
            }
        }

        private int _MinValue;
        [Description("Specify the Minimum value of tag."), DisplayName("Sensor Min Range *"),PropertyOrder(8)]
        
        public int MinValue
        {
            get { return _MinValue; }
            set { _MinValue = value; }
        }


        private int _MaxValue;
        [Description("Specify the maximum value of tag."), DisplayName("Sensor Max Range *"), PropertyOrder(9)]
        
        public int MaxValue
        {
            get { return _MaxValue; }
            set { _MaxValue = value; }
        }
         private int divisions;
        [Description("Specify the no of divisions."), DisplayName("Division Value *"), DefaultValue((int)10), PropertyOrder(10)]
        public int Divisions
        {
            get { return divisions; }
            set { divisions = value; OnPropertyChanged("Divisions"); }
        }
        private string units;
        [Description("Specify the Parameter Units."), DisplayName("Parameter Units *"), DefaultValue((int)10), PropertyOrder(5)]
        public string Units
        {
            get { return units; }
            set { units = value; OnPropertyChanged("Units"); }
        }
       
        private BlockTypes _BlockType;
        [Description("Specify the BlockType Parametrs."), DisplayName("BlockType"),PropertyOrder(6)]
       
        public BlockTypes BlockType
        {
            get { return _BlockType; }
            set {
                _BlockType = value;
                var str = (value + "-" + tagName).ToString();
            }
        }

        //public string Address
        //{
        //    get { return (string)GetValue(AddressProperty); }
        //    set
        //    {
        //        try
        //        {
        //            SetValue(AddressProperty, value);
        //        }
        //        catch (Exception e)
        //        {
        //            MessageBox.Show(e.Message);
        //        }
        //    }
        //}

        //// Using a DependencyProperty as the backing store for Address.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty AddressProperty =
        //    DependencyProperty.Register("Address", typeof(string), typeof(Tag), new PropertyMetadata(null));



        [Description("Select the format of the incoming tag data."), DisplayName("Data Type"),PropertyOrder(7)]

        public DataType DataType
        {
            get
            {
                return datatype;
            }
            set
            {
                datatype = value;
            }
        }

        //public DataType DataType
        //{
        //    get { return (DataType)GetValue(DataTypeProperty); }
        //    set { SetValue(DataTypeProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for DataType.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DataTypeProperty =
        //    DependencyProperty.Register("DataType", typeof(DataType), typeof(Tag), new PropertyMetadata(null));


        
        [Description("Indicate the rights the client has in accessing the data."), DisplayName("Data Access"), DefaultValue(11)]
        public DataAccess DataAccessRights
        {
           get
            {
                return dataAccessRight;
            }
            set
            {
                dataAccessRight = value;
            }
        }
        //public DataAccess DataAccessRights //TODO: --Done  Make sure we are using this property setting before read/write 
        //{
        //    get { return (DataAccess)GetValue(DataAccessRightsProperty); }
        //    set { SetValue(DataAccessRightsProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for DataAccess.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DataAccessRightsProperty =
        //    DependencyProperty.Register("DataAccessRights", typeof(DataAccess), typeof(Tag), new PropertyMetadata(null));
        private int scanRate;
        [Description("Specify the poll interval, in milliseconds, for this tag."), DisplayName("Scan Rate (ms)")]
        [Browsable(false)]
        public int ScanRate
        {
            get
            {
                return scanRate;
            }

            set
            {
                if(value==0)
                {
                    scanRate = SLIKDAUACONFIG.GetDefaultTagScanRate();
                }
                else if (value % 10 == 0 && value != 0)
                {
                    scanRate = value;
                }
                else
                {
                    int addNum = 10 - (value % 10);
                    if (addNum > 5)
                        value = value + addNum;
                    else
                        value = value - (value % 10);
                    scanRate = value;
                }
                OnPropertyChanged("ScanRate");
            }

        }

        //public int ScanRate
        //{
        //    get { return (int)GetValue(ScanRateProperty); }
        //    set
        //    {
        //        if (value % 10 == 0 && value != 0)
        //        {
        //            SetValue(ScanRateProperty, value);
        //        }
        //        else
        //        {
        //            int addNum = 10 - (value % 10);
        //            if (addNum > 5)
        //                value = value + addNum;
        //            else
        //                value = value - (value % 10);
        //            SetValue(ScanRateProperty, value);
        //        }
        //    }
        //}

        //// Using a DependencyProperty as the backing store for ScanRate.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ScanRateProperty =
        //    DependencyProperty.Register("ScanRate", typeof(int), typeof(Tag), new PropertyMetadata(SLIKDAUACONFIG.GetDefaultTagScanRate()));

        [DisplayName("Selected Group *"), Browsable(false), Description("Specify the on which group tag is created.")]
        [ItemsSource(typeof(TagGroupItemsSource)), DefaultValue("PumpTest") /*DefaultValue("--Select--")*/ ]
        public string SelectedGroup
        {
            get { return (string)GetValue(SelectedGroupProperty); }
            set { SetValue(SelectedGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedGroupProperty =
            DependencyProperty.Register("SelectedGroup", typeof(string), typeof(Tag), new PropertyMetadata(null));


        //23 01 2017
        //update if and only if the value of the each tag changes

        [Browsable(false)]
        public bool PrevoiusBooleanTagValue { get; set; }

        [Browsable(false)]
        public int PrevoiusIntegerTagValue { get; set; }

        [Browsable(false)]
        public short PrevoiusShortTagValue { get; set; }

        [Browsable(false)]
        public string PrevoiusStringTagValue { get; set; }

        [Browsable(false)]
        public double PrevoiusDoubleTagValue { get; set; }

        string ITag.Name
        { get; set; }



        //[Browsable(false)]
        //public byte SlaveId
        //{
        //    get { return (byte)GetValue(slaveIdProperty); }
        //    set { SetValue(slaveIdProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for slaveId.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty slaveIdProperty =
        //    DependencyProperty.Register("slaveId", typeof(byte), typeof(Tag), new PropertyMetadata(null));

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
                catch (Exception)
                {

                }

            }
        }


        #endregion End Of Variable Memebers and Properties

        /// <summary>
        /// It collect all the available Groups in a Device for creating a Tag.
        /// </summary>
        public class TagGroupItemsSource : IItemsSource
        {
            public static ObservableCollection<TagGroup> GroupsTags { get; set; }
            public ItemCollection GetValues()
            {

                ItemCollection Groups = new ItemCollection();
                try
                {
                    if (GroupsTags != null)
                    {
                        //Groups.Add("PumpTest");
                        #region GroupName Default As PumpTest For Hydrofit
                        // These lines are commented by sathish for Hydrofit project
                        //Groups.Add("--Select--");
                        //Groups.Add("None");

                        #endregion GroupName Default As PumpTest For Hydrofit
                        foreach (var item in GroupsTags) //tagGroups
                        {
                            Groups.Add(item.GroupName);
                        }

                    }
                }
                catch (Exception)
                {
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                        ElpisServer.Addlogs("Configuration", @"Elpis/Tag", "Problem in getting the tag groups", LogStatus.Error);
                    //}), DispatcherPriority.Normal, null);

                }
                return Groups;
            }
        }

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
        #endregion Hiding Dependency Object Properties
       
    }
    #endregion End Of Tags Class
}
#endregion OPCEngine namespace