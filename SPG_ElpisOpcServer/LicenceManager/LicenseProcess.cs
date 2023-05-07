using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SKCLNET;
using System.Diagnostics;
using System.Reflection;

namespace LicenseManager
{

    public class LicenseHandler
    {
        private bool IsDemo;

        private SecurityMode softwareMode;
        private int getComputerID;
        [Obfuscation(Feature = "renaming", Exclude = false, ApplyToMembers = true, StripAfterObfuscation = true)]
        public enum SecurityMode
        {
            /// <summary>
            /// Initial value, flag to show that the licensing is not yet initiated
            /// </summary>
            NotSet,

            /// <summary>
            /// Evaluation mode.
            /// </summary>
            Evaluation,

            /// <summary>
            /// Product is fully licensed and should be unrestricted
            /// </summary>
            Licensed,

            /// <summary>
            /// Expired, so nothing should work!
            /// </summary>
            Expired,

            /// <summary>
            /// Illegal. An attempt to circumvent security was detected.
            /// </summary>
            Illegal
        }

        const int CONST_HARDDRIVE = 2;//2;
        const int CONST_WINID = 128;
        const int CONST_AUTOCREATEFILE = 8;
        const int CONST_PRODUCTNAME = 1;
        string CONST_TCSEED = "400";
        string CONST_K2SEED = "123";
        string CONST_PASSWD = "pa55w0rd";
        const int partNumber = 11109999;

        public int GetComputerId(string filename)
        {
            ProtectionPlusWrapper(true, filename);
            return getComputerID;

        }
        public bool CheckLicense(bool isLicenseCheck, string filename)
        {
            bool isLicensed = false;
            //Check general license
            if (File.Exists(filename))
                isLicensed = VerifyLicense(filename);
            return isLicensed;
        }
        public bool SetLicenseCode(string filename, string licenseCode, string computerId)
        {
            bool isLicensed = false;
            string LicenceFile = System.Environment.CurrentDirectory;
            try
            {
                //create new instance of protection
                ProtectionPlusWrapper(false, filename, partNumber, licenseCode, computerId);
                // System.Windows.Forms.MessageBox.Show("licdd return " + softwareMode.ToString());
                if (softwareMode == SecurityMode.Licensed)
                {
                    isLicensed = true;
                    //System.Windows.Forms.MessageBox.Show("Licenced true");
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                isLicensed = false;
            }
            return isLicensed;
        }
        private bool VerifyLicense(string filename)
        {
            // Read the licence File and find out the part Number  number 
            string LicenceFile = System.Environment.CurrentDirectory;
            //create new instance of protection
            ProtectionPlusWrapper(true, filename);
            if (softwareMode == SecurityMode.Licensed)
            {
                IsDemo = false;
                //System.Windows.Forms.MessageBox.Show("full");
            }
            else
            {
                IsDemo = true;
                //System.Windows.Forms.MessageBox.Show("demo");
            }

            return IsDemo;
        }

        private void protectionPlus_StatusChanged(bool startup)
        {
            // System.Windows.Forms.MessageBox.Show("in staus");
            if (protectionPlus.IsExpired)
                softwareMode = SecurityMode.Expired;

            else if (protectionPlus.IsClockTurnedBack)
                softwareMode = SecurityMode.Expired;

            else if (protectionPlus.IsDemo)
            {
                // System.Windows.Forms.MessageBox.Show("Demo entered....");
                softwareMode = SecurityMode.Evaluation;
            }

            else if (protectionPlus.ExpireMode == "N" || protectionPlus.ExpireMode == "P")
                softwareMode = SecurityMode.Licensed;

            else
                softwareMode = SecurityMode.Illegal;

        }
        private void protectionPlus_Error()
        {
            //System.Windows.Forms.MessageBox.Show("in errror");
            softwareMode = SecurityMode.Illegal;
            System.Diagnostics.Debug.WriteLine(string.Format("ProtectionPlus Error '{0}'", protectionPlus.LastErrorString));
        }
        private LFile protectionPlus;
        private void ProtectionPlusWrapper(bool IsLicenseCheck, string fname = null, int partnumber = 0, string LicenseCode = null, string computerID = null)
        {
            // Check for License file(.dat file).
            // If license file not present then exit the application.
            if (File.Exists(fname))
            {
                //initialize and use protection plus
                try
                {
                    Trace.WriteLine("Initializing...");
                    protectionPlus = new LFile();

                    protectionPlus.Error += new SKCLNET.LFile.__Delegate_Error(protectionPlus_Error);
                    protectionPlus.StatusChanged += new SKCLNET.LFile.__Delegate_StatusChanged(protectionPlus_StatusChanged);
                    protectionPlus.CPAlgorithm = CONST_HARDDRIVE | CONST_WINID;
                    protectionPlus.CPAlgorithmDrive = "C";
                    protectionPlus.EZTrial = false;
                    protectionPlus.UseEZTrigger = true;
                    protectionPlus.TCSeed = Convert.ToInt16(CONST_TCSEED);
                    protectionPlus.TCRegKey2Seed = Convert.ToInt16(CONST_K2SEED);
                    protectionPlus.LFPassword = CONST_PASSWD;
                    protectionPlus.LFOpenFlags = SKLFOPENFLAGS.CREATE_NORMAL;
                    protectionPlus.LFName = fname;
                    int compuId = protectionPlus.LicensedComputers;
                    getComputerID = protectionPlus.CPCompNo;

                    int j = protectionPlus.CPCheck(0);


                    if (IsLicenseCheck)
                    {

                        int cnt = protectionPlus.CPCheck(0);
                        // System.Windows.Forms.MessageBox.Show("NUM"+cnt.ToString());
                        bool found = false;
                        for (int count = 1; count <= cnt; count++)
                        {
                            // System.Windows.Forms.MessageBox.Show (protectionPlus.GetLicensedComputer(1).ToString());
                            if (getComputerID == protectionPlus.GetLicensedComputer(count))
                            {
                                found = true;
                                // System.Windows.Forms.MessageBox.Show("found computer Entry....");
                                break;
                            }
                        }
                        //System.Windows.Forms.MessageBox.Show("after check " + softwareMode.ToString());

                        DateTime exp_date = Convert.ToDateTime(protectionPlus.ExpireDateHard);
                        DateTime now = DateTime.Now;

                        string exp_mocde = protectionPlus.ExpireMode;
                        //trigger code p(periodic) 1 month demo
                        if ((exp_date <= now) && (exp_mocde == "P"))
                            softwareMode = SecurityMode.Illegal;



                        if ((!found) && (softwareMode == SecurityMode.Licensed) && (exp_mocde != "P"))
                        {
                            // System.Windows.Forms.MessageBox.Show("Illigal");
                            softwareMode = SecurityMode.Illegal;
                        }

                        // System.Windows.Forms.MessageBox.Show(softwareMode.ToString());
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("in license set");
                        //System.Windows.Forms.MessageBox.Show(softwareMode.ToString());
                        //licence check technic in if statement
                        int result = 0;

                        int value = protectionPlus.TCode(Convert.ToInt32(LicenseCode), 0, 11109999, Convert.ToInt32(computerID), ref result);
                        //System.Windows.Forms.MessageBox.Show(value.ToString());

                        // if ((Convert.ToInt32(LicenseCode) - partnumber == protectionPlus.CPCompNo))
                        if (value == 31)//LFEdit, trigger code 31
                        {
                            int h = protectionPlus.FloatingUsersCount;
                            protectionPlus.ExpireMode = "N";
                            protectionPlus.SetLicensedComputer(0, protectionPlus.CPCompNo);

                            // protectionPlus .TCode(21062029, 0, 11109999, SerialNum, ref result);
                            // if (result > 0) // sucess
                            protectionPlus.SetUserString(2, DateTime.Now.ToString());
                            // protectionPlus.RegUser = cop copmpany name
                            protectionPlus.ExpireMode = "N";
                            softwareMode = SecurityMode.Licensed;
                            //System.Windows.Forms.MessageBox.Show("Licenced");

                        }
                        else if (value == 19)
                        {
                            protectionPlus.ExpireMode = "P";
                            softwareMode = SecurityMode.Licensed;
                        }

                        else
                        {
                            softwareMode = SecurityMode.Evaluation;
                        }
                    }
                    //the events will have fired by now, so dispose of protection plus
                    protectionPlus.Error -= new SKCLNET.LFile.__Delegate_Error(protectionPlus_Error);
                    protectionPlus.StatusChanged -= new SKCLNET.LFile.__Delegate_StatusChanged(protectionPlus_StatusChanged);
                    //protectionPlus.Dispose();
                    protectionPlus.LFClose();
                    protectionPlus = null;
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                    softwareMode = SecurityMode.NotSet;
                }
            }
        }
    }
}
