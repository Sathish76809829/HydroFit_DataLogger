using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Threading;

namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// This Class defines creation of tags under a Group.
    /// </summary>
    [Serializable()]
    public class TagGroup:SerializableDependencyObject, INotifyPropertyChanged
    {
        #region Constructor
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        public TagGroup():base()
        {

        }
        /// <summary>
        /// Deserialization Construnctor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public TagGroup(SerializationInfo info, StreamingContext context):base(info,context)
        {

        }
        #endregion Constructor

        

        private string groupName="PumpTest";
        [DisplayName("Group Name *"), Description("Name of the Tag Group on Device.")]
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
                OnPropertyChanged("GroupName");
            }
        }
        //public string GroupName
        //{
        //    get { return (string)GetValue(GroupNameProperty); }
        //    set { SetValue(GroupNameProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for GroupName.
        //// This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty GroupNameProperty =
        //    DependencyProperty.Register("GroupName", typeof(string), typeof(TagGroup), new PropertyMetadata(null));


        private string deviceName;

      

        [Browsable(false)]
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
        //    DependencyProperty.Register("DeviceName", typeof(string), typeof(TagGroup), new PropertyMetadata(null));
       

        [Browsable(false)]
        public ObservableCollection<Tag> TagsCollection { get; set; }

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

}
