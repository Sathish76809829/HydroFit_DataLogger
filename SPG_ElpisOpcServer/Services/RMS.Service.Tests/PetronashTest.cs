using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RMS.Service.Abstractions.Database;
using RMS.Service.Abstractions.Parser;

namespace RMS.Service.Tests
{
    [TestClass]
    public class PetronashTest
    {
        [TestMethod]
        public void CalculationTest()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer("Data Source=mi3-wsq2.my-hosting-panel.com;Initial Catalog=elpisits_HR;Persist Security Info=True;User ID=elpisits_admin2;Password=Testing123$").Options;

            //using var context = new RMSDbContext(options, new Entity[] {
            //    new Entity(typeof(Petronash.Models.PetronashSignals)) { BuildAction = BuildPetronashSignals },
            //    new Entity(typeof(Petronash.Models.DeviceInputs)) { BuildAction = BuildDeviceSignals }
            //});
            //var service = new Petronash.PetronashDeviceProvider(context);
            //var data = @"{""device"":""20>2020-10-30 06:06:42<3"",""SignalDataList"":[""33>1.8<3""]}";
            //var obj = JsonObject.Parse(data);
            //if (Models.DeviceDataModel.TryParse(obj, out var deviceData))
            //{
            //    var res =  service.ProcessDataAsync(deviceData).Result;
            //}
        }

        private void BuildPetronashSignals(EntityTypeBuilder obj)
        {
            obj.Property("Id").IsRequired();
            obj.HasKey("Id");
            obj.Property("SignalId");
            obj.Property("DeviceId");
            obj.Property("Script")
                .HasMaxLength(1024);
        }

        private void BuildDeviceSignals(EntityTypeBuilder obj)
        {
            obj.Property("InputId").IsRequired();
            obj.HasKey("InputId");
            obj.Property("Id");
            obj.Property("DeviceId");
            obj.Property("DataType");
            obj.Property("Value")
                .HasMaxLength(512);
        }
    }
}
