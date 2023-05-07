#region Usings
using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
#endregion Usings

#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// This class is defines log information view model
    /// </summary>
    [Serializable]
    public class LoggerViewModel: INotifyPropertyChanged
    {
        private string date { get; set; }
        private string time { get; set; }
        private string source { get; set; }
        private string events { get; set; }
        private string module { get; set; }
        private LogStatus eventType { get; set; }

        private BitmapImage image { get; set; }

        public string Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                OnPropertyChanged("Date");
            }
        }

        public LogStatus EventType
        {
            get
            {
                return eventType;
            }
            set
            {
                eventType = value;
                OnPropertyChanged("EventType");
            }
        }

        public string Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
                OnPropertyChanged("Time");
            }
        }

        public string Module
        {
            get
            {
                return module;
            }
            set
            {
                module = value;
                OnPropertyChanged("Module");
            }
        }

        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
                OnPropertyChanged("Source");
            }
        }

        public string Event
        {
            get
            {
                return events;
            }
            set
            {
                events = value;
                OnPropertyChanged("Event");
            }
        }

        public BitmapImage ImageStatus
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                OnPropertyChanged("ImageStatus");
                //image.Freeze();
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
               
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }
        }


    }
}
#endregion OPCEngine Namespace
