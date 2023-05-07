using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElpisOpcServer.SunPowerGen
{
    class TestTypeFactory
    {
        public static ITestInformation CreateTestInformationObject( TestType testType)
        {
            switch (testType)
            {
                case TestType.StrokeTest:
                    return new StrokeTestInformation();
                case TestType.HoldMidPositionTest:
                    return new Hold_MidPositionLineATestInformation();
                case TestType.HoldMidPositionLineBTest:
                    return new Hold_MidPositionLineBTestInformation();
                case TestType.SlipStickTest:
                    return new Slip_StickTestInformation();
                default:
                    return null;
            }
        }


        public static ITestInformation GetTestInformationObject(TestType testType, object testObject)
        {
            switch (testType)
            {
                case TestType.StrokeTest:
                    return testObject as StrokeTestInformation;
                case TestType.HoldMidPositionTest:
                    return testObject as Hold_MidPositionLineATestInformation;
                case TestType.HoldMidPositionLineBTest:
                    return testObject as Hold_MidPositionLineBTestInformation;
                case TestType.SlipStickTest:
                    return testObject as Slip_StickTestInformation;
                default:
                    return null;
            }
        }
    }
}
