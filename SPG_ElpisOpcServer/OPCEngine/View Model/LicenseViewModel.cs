#region Namespaces

//using LicenseManager;
using System.Windows.Threading;

#endregion Namespaces

#region OPCEngine namespace
namespace Elpis.Windows.OPC.Server
{
    public class LicenseViewModel
    {

        #region Variable Memebers and Properties
        //public LicenseHandler licenceManeger = new LicenseHandler();
        //public MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

        public DispatcherTimer twoHourDemo { get; set; }
        public DispatcherTimer timer { get; set; }

        public string LicenseStatus { get; set; }
        public string LiecenceKey { get; set; }
        public int ComputerId { get; set; }
        #endregion Variable Memebers and Properties

        public void GetLicenceInitial()
        {
            //ComputerId = licenceManeger.GetComputerId("LicenseStatus.dat");
        }

        public void Activate()
        {
            //if (System.IO.File.Exists("LicenseStatus.dat"))
            //{
            //    if (licenceManeger.SetLicenseCode("LicenseStatus.dat", LiecenceKey.ToString(), ComputerId.ToString()) == true)
            //    {
            //        MessageBox.Show("Aproved");
            //        LicenseStatus = "This product is fully licensed to this computer";
            //        if (twoHourDemo == null)
            //            twoHourDemo = TimerHelper.TwohrDemo;
            //        twoHourDemo.Stop();

            //        if (timer == null)
            //            timer = TimerHelper.Timer;
            //        timer.Stop();

            //        mainWindowViewModel.TitleTxt = "Elpis OPC Server";
            //        System.Windows.MessageBox.Show(mainWindowViewModel.TitleTxt);

            //    }
            //    else
            //    {
            //        MessageBox.Show("Invalid License Key");
            //        //lblLicence.Content = "Invalid License Key";
            //        LicenseStatus = "Invalid License Key";

            //    }
            //}



        }

    }
}
#endregion OPCEngine namespace