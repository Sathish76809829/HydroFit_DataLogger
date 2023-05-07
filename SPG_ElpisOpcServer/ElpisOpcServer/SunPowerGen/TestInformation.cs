using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    public class TestInformation
    {
        private int offSetValue; 
        private string triggerTestAddress = "";
        private string triggerStatus = "";
        //private string triggerPLC = "";

        private string comment = "";
        private string commentMessage = "";
        private string testDeatilsMessage;
        private string testDeatilsInfo;

        public event PropertyChangedEventHandler PropertyChanged;
        public int OffSetValue
        {
            get { return offSetValue; }
            set
            {
                offSetValue = value;
                OnPropertyChanged("reportStatus");
            }
        }

        public string TriggerTestAddress
        {
            get { return triggerTestAddress; }
            set
            {
                triggerTestAddress = value;
                OnPropertyChanged("TriggerTestAddress");
            }
        }

        public string TriggerStatus
        {
            get { return triggerStatus; }
            set
            {
                triggerStatus = value;
                OnPropertyChanged("TriggerStatusStroke");
            }
        }

        //public string TriggerPLC
        //{
        //    get { return triggerPLC; }
        //    set
        //    {
        //        triggerPLC = value;
        //        OnPropertyChanged("TriggerPLC");
        //    }
        //}

        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value;
                OnPropertyChanged("Comment");
            }
        }
        public string TestDeatilsInfo
        {
            get { return testDeatilsInfo; }
            set
            {
                testDeatilsInfo = value;
                OnPropertyChanged("TestDeatilsInfo");
            }
        }

        public string CommentMessage
        {
            get { return commentMessage; }
            set
            {
                commentMessage = value;
                OnPropertyChanged("CommentMessage");
            }
        }

        public string TestDeatilsMessage
        {
            get { return testDeatilsMessage; }
            set
            {
                testDeatilsMessage = value;
                OnPropertyChanged("TestDeatilsMessage");
            }
        }

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
    }
}
