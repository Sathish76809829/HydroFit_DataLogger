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
    /// <summary>
    /// This class holds the StrokeTest information
    /// </summary>
    public class StrokeTestInformation :TestInformation, ITestInformation, IDataErrorInfo
    {

        private string pressureLineA = "0";
        private string pressureLineB = "0";
        private string flow = "0";
        private string strokeLengthValue = "0";
        private double noOfCycles= uint.MinValue;// = ushort.MinValue;
        private string lineAPressureInput = "0";// = string.Empty;
        private string lineBPressureInput = "0";// = string.Empty;
        private string customeName;//= string.Empty;
        private string jobNumber;// = string.Empty;
        private TestType testName = TestType.StrokeTest;
        private string reportNumber;
        private string testDateTime;
        private uint boreSize;//= uint.MinValue;
        private uint rodSize;// = uint.MinValue;
        private uint strokeLength;// = uint.MinValue;  
        private bool isTestStarted = false;
        private string cyliderNumber = string.Empty;
        private double noofCyclesCompleted = uint.MinValue;
        //public event PropertyChangedEventHandler PropertyChanged;
        private string reportStatus = "";

        //jey
        //private string triggerStatus="";
        private string triggerStatusStrokeTest = "OFF";
        private string triggerTestAddress="";
        //jey
        /*private string comment="";
        private string commentMessage="";*/

        private int reportCount = 0;


        #region Constructor
        public StrokeTestInformation()
        {
            //noOfCycles = 0;
            //lineAPressureInput = "";
            //lineBPressureInput = "";
            //customeName = "";
            //jobNumber = "";        
            //boreSize = 0;
            //rodSize = 0;
            //strokeLength =0;
        }
        #endregion

        #region Properties
        public string ReportStatus
        {
            get { return reportStatus; }
            set
            {
                reportStatus = value;
                OnPropertyChanged(nameof(ReportStatus));
            }
        }

        //public string TriggerStatus_not_used
        //{
        //    get { return triggerStatus; }
        //    set
        //    {
        //        triggerStatus = value;
        //        OnPropertyChanged("TriggerStatusStroke");
        //    }
        //}

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

        public string TriggerStatusStrokeTest
        {
            get { return triggerStatusStrokeTest; }
            set
            {
                triggerStatusStrokeTest = value;
                OnPropertyChanged("TriggerStatusStrokeTest");
            }
        }
        //comman for all test 
        public string TriggerTestAddress_not_used
        {
            get { return triggerTestAddress; }
            set
            {
                triggerTestAddress = value;
                OnPropertyChanged("TriggerTestAddress");
            }
        }

        public string PressureLineA
        {
            get { return pressureLineA; }
            set
            {
                pressureLineA = value;
                OnPropertyChanged("PressureLineA");
            }
        }

        public string PressureLineB
        {
            get { return pressureLineB; }
            set
            {
                pressureLineB = value;
                OnPropertyChanged("PressureLineB");
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
        public string StrokeLengthValue
        {
            get { return strokeLengthValue; }
            set
            {
                strokeLengthValue = value;
                OnPropertyChanged("StrokeLengthValue");
            }

        }

        public double NoofCycles
        {
            get { return noOfCycles; }
            set
            {
                try
                {
                    noOfCycles = value;
                    OnPropertyChanged("NoofCycles");
                }
                catch (Exception ex)
                {

                }
            }
        }

        public string LineAPressureInput
        {
            get { return lineAPressureInput; }
            set
            {
                lineAPressureInput = value;
                OnPropertyChanged("LineAPressureInput");
            }
        }

        public string LineBPressureInput
        {
            get { return lineBPressureInput; }
            set
            {
                lineBPressureInput = value;
                OnPropertyChanged("LineBPressureInput");
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

        [Required(AllowEmptyStrings = false, ErrorMessage = "Required")]
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
                testName = TestType.StrokeTest;
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

        public double NoofCyclesCompleted
        {
            get { return noofCyclesCompleted; }
            set
            {
                noofCyclesCompleted = value;
                OnPropertyChanged("NoofCyclesCompleted");
            }
        }

        public bool IsTestStarted
        {
            get { return isTestStarted; }
            set { isTestStarted = value; OnPropertyChanged("IsTestStarted"); }
        }

        public string CylinderNumber
        {
            get { return cyliderNumber; }
            set { cyliderNumber = value; OnPropertyChanged("CylinderNumber"); }
        }

        public string Error
        {
            get { return null; }
        }

        public int ReportCount {
            get
            {
               return reportCount;
            }

            set
            {
                reportCount = value;
                
            }

        }

        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }
        #endregion

        #region Validation Methods
        /// <summary>
        /// Validate the properties and set error messages
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
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
                                validationMessage = "Valid format is,(YYYYMM<D 4>)";//, valid format is <yyyy><mm><x[10]>";
                                                                                     //else
                                                                                     //{

                            //    string valString = string.Format("{0}{1}", DateTime.Now.Year.ToString("0000"), DateTime.Now.Month.ToString("00"));
                            //    if(jobNumber.Substring(0,6)!=valString)
                            //        validationMessage = "Valid format is,(YYYYMM<D 10>)";
                            //}
                        }

                        break;
                    case "CustomerName":
                        if (string.IsNullOrWhiteSpace(customeName) || string.IsNullOrEmpty(customeName))
                        {
                            validationMessage = "Required";
                            customeName = "";
                        }
                        break;
                    //case "NoofCycles":
                    //    if (noOfCycles <= ushort.MinValue || noOfCycles >= ushort.MaxValue)
                    //        validationMessage = "Required";
                    //    break;
                    //case "LineAPressureInput":
                    //    try
                    //    {
                    //        if (string.IsNullOrEmpty(lineAPressureInput) || float.Parse(lineAPressureInput) <= float.MinValue || float.Parse(lineAPressureInput) >= float.MaxValue)
                    //            validationMessage = "Required";
                    //    }
                    //    catch (Exception)
                    //    {
                    //        validationMessage = "Invalid Format.";
                    //    }
                    //    break;
                    //case "LineBPressureInput":
                    //    try
                    //    {
                    //        if (string.IsNullOrEmpty(lineBPressureInput) || float.Parse(lineBPressureInput) <= float.MinValue || float.Parse(lineBPressureInput) >= float.MaxValue)
                    //            validationMessage = "Required";
                    //    }
                    //    catch (Exception)
                    //    {
                    //        validationMessage = "Invalid Format.";
                    //    }
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
                    default:
                        break;

                    case "CylinderNumber":
                        if (string.IsNullOrWhiteSpace(cyliderNumber) || string.IsNullOrEmpty(cyliderNumber))
                        {
                            validationMessage = "Required";
                            cyliderNumber = "";
                        }
                        else if(cyliderNumber.Contains('_') || cyliderNumber.Contains('-'))
                        {
                            validationMessage = "Invalid data.";
                            cyliderNumber = cyliderNumber.Replace('_',' ').Replace('-',' ');
                            cyliderNumber.Trim();
                        }
                        break;

                }
            }
            //isTestStarted = true;
            return validationMessage;
        }
        //public void OnPropertyChanged1(string Property)
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
        #endregion

    }
}
