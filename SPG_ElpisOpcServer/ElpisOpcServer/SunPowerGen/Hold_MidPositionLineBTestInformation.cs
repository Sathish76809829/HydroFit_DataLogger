using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    public class Hold_MidPositionLineBTestInformation :TestInformation, ITestInformation, ICeritificateInformation, ICylinderInformation, IDataErrorInfo
    {
        private string holdingTimeLineB="0";
        private string runningHoldingTimeLineB = "0";
        private string holdingPressureLineA="0";
        private string holdingPressureLineB="0";
        private string initialPressureLineB = "0";
        private string allowablePressureDrop="0";
        private string cylinderMovement = "0";
        private string customeName = string.Empty;
        private string jobNumber = string.Empty;
        private TestType testName;
        private string reportNumber;
        private string testDateTime;
        private uint boreSize;
        private uint rodSize;
        private uint strokeLength;
        private string testStatusB="";
        private string cylinderNumber = string.Empty;
        private string reportStatus = string.Empty;
        private bool isTestStarted;
        private string initialcylinderMovement = "0";
        private string comment = "";
        private string commentMessage = "";
        private string triggerStatus="OFF";

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

        public string InitialCylinderMovement
        {
            get { return initialcylinderMovement; }
            set
            {
                initialcylinderMovement = value;
                OnPropertyChanged("InitialCylinderMovement");
            }
        }

        public string InitialPressureLineB
        {
            get { return initialPressureLineB; }
            set
            {
                initialPressureLineB = value;
                OnPropertyChanged("InitialPressureLineB");
            }
        }
        public string HoldingTimeLineB
        {
            get { return holdingTimeLineB; }
            set
            {
                holdingTimeLineB = value;
                OnPropertyChanged("HoldingTimeLineB");
            }

        }

        public string RunningHoldingTimeLineB
        {
            get { return runningHoldingTimeLineB; }
            set
            {
                runningHoldingTimeLineB = value;
                OnPropertyChanged("RunningHoldingTimeLineB");
            }
        }

        public string HoldingPressureLineA
        {
            get { return holdingPressureLineA; }
            set
            {
                holdingPressureLineA = value;
                OnPropertyChanged("HoldingPressureLineA");
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

        public string CustomerName
        {
            get { return customeName; }
            set
            {
                customeName = value;
                OnPropertyChanged("CustomerName");
            }
        }

        public string JobNumber
        {
            get { return jobNumber; }
            set
            {
                jobNumber = value;
                OnPropertyChanged("JobNumber");
            }
        }

        public string TriggerPLCHoldMidB
        {
            get { return triggerStatus; }
            set
            {
                triggerStatus = value;
                OnPropertyChanged("TriggerPLCHoldMid");
            }
        }
        public TestType TestName
        {
            get { return testName; }
            set
            {
                testName = value;
                OnPropertyChanged("TestName");
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

        public string ReportNumber
        {
            get { return reportNumber; }
            set
            {
                reportNumber = value;
                OnPropertyChanged("ReportNumber");
            }
        }

        public string TestDateTime
        {
            get { return testDateTime; }
            set
            {
                testDateTime = value;
                OnPropertyChanged("TestDateTime");
            }
        }

        public uint BoreSize
        {
            get { return boreSize; }
            set
            {
                boreSize = value;
                OnPropertyChanged("BoreSize");
            }
        }

        public uint RodSize
        {
            get { return rodSize; }
            set
            {
                rodSize = value;
                OnPropertyChanged("RodSize");
            }
        }

        public uint StrokeLength
        {
            get { return strokeLength; }
            set
            {
                strokeLength = value;
                OnPropertyChanged("StrokeLength");
            }
        }

        public string TestStatusB
        {
            get
            {
                return testStatusB;
            }
            set
            {
                testStatusB = value;
                OnPropertyChanged("TestStatusB");
            }
        }

        public string CylinderNumber
        {
            get { return cylinderNumber; }
            set
            {
                cylinderNumber = value;
                OnPropertyChanged("CylinderNumber");
            }
        }
        public bool IsTestStarted
        {
            get { return isTestStarted; }
            set { isTestStarted = value; OnPropertyChanged("IsTestStarted"); }
        }

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }


        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
            if (isTestStarted)
            {
                switch (propertyName)
                {
                    case "JobNumber":
                        if (string.IsNullOrWhiteSpace(jobNumber) || string.IsNullOrEmpty(jobNumber))
                            validationMessage = "Required";
                        else if (jobNumber.Length > 0)
                        {
                            Regex expr = new Regex("^[0-9]{10}$");
                            if (!expr.IsMatch(jobNumber))
                                validationMessage = "Valid format is,(YYYYMM < D 4 >)";
                        }
                        break;
                    case "CustomerName":
                        if (string.IsNullOrWhiteSpace(customeName) || string.IsNullOrEmpty(customeName))
                            validationMessage = "Required";
                        break;
                    //case "AllowablePressureDrop":
                    //    if (allowablePressureDrop <= ushort.MinValue || allowablePressureDrop >= ushort.MaxValue)
                    //        validationMessage = "Required";
                    //    break;
                    case "BoreSize":
                        if (boreSize <= uint.MinValue || boreSize >= uint.MaxValue)
                            validationMessage = "Required";
                        break;
                    case "RodSize":
                        if (rodSize <= uint.MinValue || rodSize >= uint.MaxValue)
                            validationMessage = "Required";
                        break;
                    case "StrokeLength":
                        if (strokeLength <= uint.MinValue || strokeLength >= uint.MaxValue)
                            validationMessage = "Required";
                        break;
                    case "CylinderNumber":
                        if (string.IsNullOrWhiteSpace(cylinderNumber) || string.IsNullOrEmpty(cylinderNumber))
                        {
                            validationMessage = "Required";
                            cylinderNumber = string.Empty;
                        }
                        break;
                }
            }

            return validationMessage;
        }

        //public void OnPropertyChanged_not_used(string Property)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        try
        //        {
        //            PropertyChanged(this, new PropertyChangedEventArgs(Property));
        //        }
        //        catch (Exception)
        //        {

        //        }

        //    }
        //}


    }

}
