using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Claims.Test
{
    [TestClass]
    public class MitchellXmlFormatterTest
    {
        [TestMethod]
        public void TestParsing()
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                StringBuilder message = new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<cla:MitchellClaim xmlns:cla=""http://www.mitchell.com/examples/claim"">
  <cla:ClaimNumber>22c9c23bac142856018ce14a26b6c299</cla:ClaimNumber>
  <cla:ClaimantFirstName>George</cla:ClaimantFirstName>
  <cla:ClaimantLastName>Washington</cla:ClaimantLastName>
  <cla:Status>OPEN</cla:Status>
  <cla:LossDate>2014-07-09T17:19:13.631-07:00</cla:LossDate>
  <cla:LossInfo>
    <cla:CauseOfLoss>Collision</cla:CauseOfLoss>
    <cla:ReportedDate>2014-07-10T17:19:13.676-07:00</cla:ReportedDate>
    <cla:LossDescription>Crashed into an apple tree.</cla:LossDescription>
  </cla:LossInfo>
  <cla:AssignedAdjusterID>12345</cla:AssignedAdjusterID>
  <cla:Vehicles>
    <cla:VehicleDetails>
      <cla:Vin>1M8GDM9AXKP042788</cla:Vin>
      <cla:ModelYear>2015</cla:ModelYear>
      <cla:MakeDescription>Ford</cla:MakeDescription>
      <cla:ModelDescription>Mustang</cla:ModelDescription>
      <cla:EngineDescription>EcoBoost</cla:EngineDescription>
      <cla:ExteriorColor>Deep Impact Blue</cla:ExteriorColor>
      <cla:LicPlate>NO1PRES</cla:LicPlate>
      <cla:LicPlateState>VA</cla:LicPlateState>
      <cla:LicPlateExpDate>2015-03-10-07:00</cla:LicPlateExpDate>
      <cla:DamageDescription>Front end smashed in. Apple dents in roof.</cla:DamageDescription>
      <cla:Mileage>1776</cla:Mileage>
    </cla:VehicleDetails>
  </cla:Vehicles>
</cla:MitchellClaim>");
                System.Text.ASCIIEncoding encoding = new ASCIIEncoding();
                stream.Write(encoding.GetBytes(message.ToString()), 0, message.Length);
                stream.Position = 0;
                MitchellXmlFormatter formatter = new MitchellXmlFormatter();
                var formattedObject = formatter.ReadFromStream(typeof(MitchellClaim), stream, null, null);
                MitchellClaim claim = formattedObject as MitchellClaim;
                Assert.IsNotNull(claim);
                Assert.AreEqual(Guid.Parse("22c9c23bac142856018ce14a26b6c299"), claim.ClaimNumber);
                Assert.AreEqual("George", claim.ClaimantFirstName);
                Assert.AreEqual("Washington", claim.ClaimantLastName);
                Assert.AreEqual(Claims.Status.OPEN, claim.Status);
                Assert.AreEqual(DateTime.Parse("2014-07-09T17:19:13.631-07:00"), claim.LossDate);
                Assert.AreEqual(Claims.CauseOfLossCode.Collision, claim.CauseOfLoss);
                Assert.AreEqual(DateTime.Parse("2014-07-10T17:19:13.676-07:00"), claim.ReportedDate);
                Assert.AreEqual("Crashed into an apple tree.", claim.LossDescription);
                Assert.AreEqual(12345, claim.AssignedAdjusterID);
                Assert.IsNotNull(claim.VehicleDetails);
                Assert.AreEqual(1, claim.VehicleDetails.Count);
                VehicleDetail vehicle = claim.VehicleDetails.ElementAt(0);
                Assert.AreEqual("1M8GDM9AXKP042788", vehicle.Vin);
                Assert.AreEqual((short)2015, vehicle.ModelYear);
                Assert.AreEqual("Ford", vehicle.MakeDescription);
                Assert.AreEqual("Mustang", vehicle.ModelDescription);
                Assert.AreEqual("EcoBoost", vehicle.EngineDescription);
                Assert.AreEqual("Deep Impact Blue", vehicle.ExteriorColor);
                Assert.AreEqual("NO1PRES", vehicle.LicPlate);
                Assert.AreEqual("VA", vehicle.LicPlateState);
                Assert.AreEqual(DateTime.Parse("2015-03-10-07:00"), vehicle.LicPlateExpDate);
                Assert.AreEqual("Front end smashed in. Apple dents in roof.", vehicle.DamageDescription);
                Assert.AreEqual(1776, vehicle.Mileage);
            }
        }

        [TestMethod]
        public void TestRoundTrip()
        {
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            claim.ClaimantFirstName = "Alice";
            claim.ClaimantLastName = "Test";
            VehicleDetail vehicle = new VehicleDetail();
            vehicle.Vin = "12345";
            vehicle.LicPlate = "ABCDEF";
            vehicle.Mileage = 54321;
            claim.VehicleDetails = new[] { vehicle };

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                MitchellXmlFormatter formatter = new MitchellXmlFormatter();
                formatter.WriteToStream(typeof(MitchellClaim), claim, stream, null);
                stream.Position = 0;
                var formattedObject = formatter.ReadFromStream(typeof(MitchellClaim), stream, null, null);
                MitchellClaim deserializedClaim = formattedObject as MitchellClaim;
                Assert.IsNotNull(deserializedClaim);
                Assert.AreEqual(claim.ClaimNumber, deserializedClaim.ClaimNumber);
                Assert.AreEqual(claim.ClaimantFirstName, deserializedClaim.ClaimantFirstName);
                Assert.AreEqual(claim.ClaimantLastName, deserializedClaim.ClaimantLastName);
                Assert.IsNotNull(claim.VehicleDetails);
                Assert.AreEqual(1, claim.VehicleDetails.Count);
                VehicleDetail deserializedVehicle = claim.VehicleDetails.ElementAt(0);
                Assert.AreEqual(vehicle.Vin, deserializedVehicle.Vin);
                Assert.AreEqual(vehicle.LicPlate, deserializedVehicle.LicPlate);
                Assert.AreEqual(vehicle.Mileage, deserializedVehicle.Mileage);
            }
        }
    }
}
