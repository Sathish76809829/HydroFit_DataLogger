using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    public class Hold_MidPositionTestInformation : TestInformation//, ITestInformation, ICeritificateInformation, ICylinderInformation, IDataErrorInfo
    {
        private string holdingTimeLineA = "0";
        private string runningHoldingTimeLineA = "0";
        private string holdingPressureLineA = "0";
        private string holdingPressureLineB = "0";
        private string testStatusA = "";
        private string initialCylinderMovement = "0";
        private string allowablePressureDrop = "0";
        private string cylinderMovement = "0";
        
        private string holdingLineAinitialPressure = "0";
        private string reportStatus = "";
        private string triggerStatus = "OFF";
        private string comment = "";
        private string commentMessage = "";
        private Int64 holdingTimeValue;


        //public event PropertyChangedEventHandler PropertyChanged;
        public string ReportStatus
        {
            get { return reportStatus; }
            set
            {
                reportStatus = value;
                OnPropertyChanged("reportStatus");
            }
        }

        public Int64 HoldingTimeValue
        {
            get { return holdingTimeValue; }
            set
            {
                holdingTimeValue = value;
                OnPropertyChanged("HoldingTimeValue");
            }
        }
        public string TriggerPLCHoldMid
        {
            get { return triggerStatus; }
            set
            {
                triggerStatus = value;
                OnPropertyChanged("TriggerPLCHoldMid");
            }
        }
        public string HoldingLineAInitialPressure
        {
            get { return holdingLineAinitialPressure; }
            set
            {
                holdingLineAinitialPressure = value;
                OnPropertyChanged("HoldingLineAInitialPressure");
            }
        }
        public string InitialCylinderMovement
        {
            get { return initialCylinderMovement; }
            set
            {
                initialCylinderMovement = value;
                OnPropertyChanged("InitialCylinderMovement");
            }
        }

        public string HoldingTimeLineA
        {
            get { return holdingTimeLineA; }
            set
            {
                holdingTimeLineA = value;
                OnPropertyChanged("HoldingTimeLineA");
            }
        }

        public string RunningHoldingTimeLineA
        {
            get { return runningHoldingTimeLineA; }
            set
            {
                runningHoldingTimeLineA = value;
                OnPropertyChanged("RunningHoldingTimeLineA");
            }
        }

        public string HoldingPressureLineA
        {
            get { return holdingPressureLineA; }
            set
            {
                holdingPressureLineA = value;
                OnPropertyChanged("HoldingPressureLineA ");
            }
        }

        public string HoldingPressureLineB
        {
            get { return holdingPressureLineB; }
            set
            {
                holdingPressureLineB = value;
                OnPropertyChanged("HoldingPressureLineB ");
            }
        }


        public string AllowablePressureDrop
        {
            get { return allowablePressureDrop; }
            set
            {
                allowablePressureDrop = value;
                OnPropertyChanged("AllowablePressureDrop");
            }
        }

        

        public string CylinderMovement
        {
            get { return cylinderMovement; }
            set
            {
                cylinderMovement = value;
                OnPropertyChanged("CylinderMovement");
            }
        }

        

        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value;
                OnPropertyChanged("Comment");
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

        public string TestStatusA
        {
            get
            {
                return testStatusA;
            }
            set
            {
                testStatusA = value;
                OnPropertyChanged("TestStatusA");
            }
        }

       

        public string Error
        {
            get { return null; }
        }

        //public string this[string columnName]
        //{
        //    get
        //    {
        //        return Validate(columnName);
        //    }
        //}
    }
}
