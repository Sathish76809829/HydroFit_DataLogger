using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    /// <summary>
    /// Certificate Model 
    /// </summary>
    public interface ICeritificateInformation : INotifyPropertyChanged
    {
        #region properties

        string CustomerName { get; set; }
        string JobNumber { get; set; }
        TestType TestName { get; set; }
        string ReportNumber { get; set; }
        string TestDateTime { get; set; }

        //    private string customeName;
        //    private string jobNumber;
        //    private TestType testName;
        //    private string reportNumber;
        //    private string testDateTime;
        //   
        //    public event PropertyChangedEventHandler PropertyChanged;

        //    public string CustomerName
        //    {
        //        get { return customeName; }
        //        set
        //        {
        //            customeName = value;
        //            OnPropertyChanged("CustomerName");
        //        }
        //    }

        //    public string JobNumber
        //    {
        //        get { return jobNumber; }
        //        set
        //        {
        //            jobNumber = value;
        //            OnPropertyChanged("JobNumber");
        //        }
        //    }

        //    public TestType TestName
        //    {
        //        get { return testName; }
        //        set
        //        {
        //            testName = value;
        //            OnPropertyChanged("TestName");
        //        }
        //    }
        //    public string ReportNumber
        //    {
        //        get { return reportNumber; }
        //        set
        //        {
        //            reportNumber = value;
        //            OnPropertyChanged("ReportNumber");
        //        }
        //    }

        //    public string TestDateTime
        //    {
        //        get { return testDateTime; }
        //        set
        //        {
        //            testDateTime = value;
        //            OnPropertyChanged("TestDateTime");
        //        }
        //    }

        //    public int BoreSize
        //    {
        //        get { return boreSize; }
        //        set
        //        {
        //            boreSize = value;
        //            OnPropertyChanged("BoreSize");
        //        }
        //    }

        //    public int RodSize
        //    {
        //        get { return rodSize; }
        //        set
        //        {
        //            rodSize = value;
        //            OnPropertyChanged("RodSize");
        //        }
        //    }

        //    public int StrokeLength
        //    {
        //        get { return strokeLength; }
        //        set
        //        {
        //            strokeLength = value;
        //            OnPropertyChanged("StrokeLength");
        //        }
        //    }


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
        //    

        //}

        #endregion Properties
    }

}
