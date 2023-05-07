using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    public class Slip_StickTestInformation : TestInformation,ITestInformation, ICeritificateInformation,ICylinderInformation, IDataErrorInfo
    {
        private string pressure="0";
        private string flow ="0" ;
       // private ushort currentPressure;
        private string initialPressure="0";
        private string cylinderMovement = "0";
        private string customeName=string.Empty;
        private string jobNumber=string.Empty;
        private TestType testName;
        private string reportNumber;
        private string testDateTime;
        private uint boreSize = uint.MinValue;
        private uint rodSize=uint.MinValue;
        private uint strokeLength=uint.MinValue;
        private string cylinderNumber = string.Empty;
        private bool isTestStarted = false;
        private string reportStatus = "";
        public string plctrigger = "0";
        //private string triggerStatus="";
        private string slipStickTestStatus ="OFF";
        private string plcTriggerTagData = "";
        //private string comment = "";
        //private string commentMessage = "";
        private string timeInterval ="00:00:00:000";
        //private int offSetValue ;//jey commented
        private string cyliderNumber=string.Empty;
        private string initialcylinderMovement="0";
        private string pressureAfterCylinderMovement;
        private string cylinderFirstMovement;

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

        public string TimeInterval
        {
            get { return timeInterval; }
            set
            {
                timeInterval = value;
                OnPropertyChanged("TimeInterval");
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

        public string CylinderFirstMovement
        {
            get { return cylinderFirstMovement; }
            set
            {
                cylinderFirstMovement = value;
                OnPropertyChanged("CylinderFirstMovement");
            }
        }

        //jey commented
        //comman for all test 
        //public int OffSetValue
        //{
        //    get { return offSetValue; }
        //    set
        //    {
        //        offSetValue = value;
        //        OnPropertyChanged("OffSetValue");
        //    }
        //}
        public string PlcTriggerTagData
        {
            get { return plcTriggerTagData; }
            set
            {
                plcTriggerTagData = value;
                OnPropertyChanged("PlcTriggerTagData");
            }
        }

        public string SlipStickTestStatus
        {
            get { return slipStickTestStatus; }
            set
            {
                slipStickTestStatus = value;
                OnPropertyChanged("SlipStickTestStatus");
            }
        }
        //public string TriggerStatus
        //{
        //    get { return triggerStatus; }
        //    set
        //    {
        //        triggerStatus = value;
        //        OnPropertyChanged("TriggerStatus");
        //    }
        //}
        public string Pressure
        {
            get { return pressure; }
            set
            {
                pressure = value;
                OnPropertyChanged("Pressure");
            }
        }

        public string PressureAfterFirstCylinderMovement
        {
            get { return pressureAfterCylinderMovement; }
            set
            {
                pressureAfterCylinderMovement = value;
                OnPropertyChanged("PressureAfterFirstCylinderMovement");
            }
        }

        public string Flow
        {
            get { return flow; }
            set
            {
                flow = value;
                OnPropertyChanged("Flow");
            }

        }

        //public ushort CurrentPressure
        //{
        //    get { return currentPressure; }
        //    set
        //    {
        //        currentPressure = value;
        //        OnPropertyChanged("CurrentPressure");
        //    }
        //}

        public string InitialPressure
        {
            get { return initialPressure; }
            set
            {
                initialPressure = value;
                OnPropertyChanged("InitialPressure");
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

        public TestType TestName
        {
            get { return testName; }
            set
            {
                testName = value;
                OnPropertyChanged("TestName");
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

        public bool IsTestStarted
        {
            get { return isTestStarted; }
            set { isTestStarted = value; OnPropertyChanged("IsTestStarted"); }
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


        //public string Comment
        //{
        //    get { return comment; }
        //    set
        //    {
        //        comment = value;
        //        OnPropertyChanged("Comment");
        //    }
        //}

        //public string CommentMessage
        //{
        //    get { return commentMessage; }
        //    set
        //    {
        //        commentMessage = value;
        //        OnPropertyChanged("CommentMessage");
        //    }
        //}

        public uint BoreSize
        {
            get { return boreSize; }
            set
            {
                boreSize = value;
                OnPropertyChanged("BoreSize");
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


        public string CylinderNumber
        {
            get { return cylinderNumber; }
            set
            {
                cylinderNumber = value;
                OnPropertyChanged("CylinderNumber");
            }
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


        public string PLCTrigger
        {
            get
            {
                return plctrigger;
            }
            set
            {
                plctrigger = value;
                OnPropertyChanged("plctrigger");
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
                                validationMessage = "Valid format is,(YYYYMM<D 4>)";
                        }
                        break;
                    case "CustomerName":
                        if (string.IsNullOrWhiteSpace(customeName) || string.IsNullOrEmpty(customeName))
                            validationMessage = "Required";
                        break;
                    case "Flow":
                        if (flow == "" || string.IsNullOrEmpty(flow))
                            validationMessage = "Required";
                        else if (flow.Length > 0)
                        {
                            Regex expr = new Regex("^[0-9]{5}$");
                            if (!expr.IsMatch(flow))
                                validationMessage = "Invalid Value";
                        }
                        break;
                    case "Pressure":
                        if (pressure == "" || string.IsNullOrEmpty(pressure))
                            validationMessage = "Required";
                        else if (pressure.Length > 0)
                        {
                            Regex expr = new Regex("^[0-9]{5}$");
                            if (!expr.IsMatch(pressure))
                                validationMessage = "Invalid Value";
                        }

                        break;
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
                            cylinderNumber = "";
                        }

                        break;

                }
            }
            return validationMessage;
        }
        // Return error message if there is error on else return empty or null string


        //public void OnPropertyChanged(string Property)
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
