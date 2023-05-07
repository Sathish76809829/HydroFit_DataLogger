#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using Elpis.Windows.OPC.Server;
#endregion Namespaces

#region OPCEngine namespace

namespace Elpis.Windows.OPC.Server
{
    #region PropertyInfoViewModel Class
    /// <summary>
    /// PropertyInfoViewModel Class
    /// </summary>
    [Serializable()]
    public class PropertyInfoViewModel : INotifyPropertyChanged
    {
        #region Variable Memebers and Properties

        private Visibility visibletypeTxtblk = new Visibility();
        private Visibility visibletypeTxt = new Visibility();
        private Visibility visibletypeCmb = new Visibility();
        private int index;
        private ControlType visibleType;
        private string propName;
        private string propValue;
        public ObservableCollection<DataAndAccessType> DataAndAccessTypeCollection { get; set; }
        public ObservableCollection<InvalidFloatValues> FloatingPointCollection { get; set; }
        //public ObservableCollection<NetworkAdaptor> NetworkAdaptorCollection { get; set; }
        public ObservableCollection<IPType> IPTypeColletion { get; set; }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public string PropertyName
        {
            get
            {
                return propName;
            }
            set
            {
                propName = value;
                OnPropertyChanged("PropertyName");
            }
        }

        public string PropertyValue
        {
            get
            {
                return propValue;
            }
            set
            {
                propValue = value;
                OnPropertyChanged("PropertyValue");
            }
        }

        public int Selectedindex
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                PropertyValue = value.ToString();
                OnPropertyChanged("Selectedindex");
            }
        }

        public ControlType VisibilityType
        {
            get
            {
                return visibleType;
            }
            set
            {
                visibleType = value;
                OnPropertyChanged("VisibilityType");
            }
        }

        public Visibility TextBlockVisibility
        {
            get
            {
                return visibletypeTxtblk;
            }
            set
            {
                visibletypeTxtblk = value;
                OnPropertyChanged("TextBlockVisibility");
            }
        }

        public Visibility TextBoxVisibility
        {
            get
            {
                return visibletypeTxt;

            }
            set
            {
                visibletypeTxt = value;
                OnPropertyChanged("TextBoxVisibility");
            }
        }

        public Visibility ComboBoxVisibility
        {
            get
            {
                return visibletypeCmb;

            }
            set
            {
                visibletypeCmb = value;
                OnPropertyChanged("ComboBoxVisibility");
            }
        }

        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }
        }

        #endregion End Of Variable Memebers and Properties
    }
    #endregion End Of PropertyInfoViewModel Class
}
#endregion OPCEngine namespace