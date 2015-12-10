using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http;
using System.Net.Http;
using Claims.Controllers;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Claims.Test
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void AddClaim()
        {
            var config = new HttpConfiguration();
            WebApiConfig.Setup(config);

            using (HttpServer server = new HttpServer(config))
            {
                var valuesUri = new Uri(new Uri("http://localhost"), "api/MitchellClaims");
                using (var client = new HttpClient(server))
                {
                    client.BaseAddress = new Uri("http://localhost/");
                    Guid newClaim = Guid.NewGuid();
                    // request to create a new claim and store it
                    HttpRequestMessage requestAddClaim = new HttpRequestMessage(HttpMethod.Post, "api/MitchellClaims");
                    const string VIN = "ABCDE";
                    StringContent newClaimContent = new StringContent(String.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<cla:MitchellClaim xmlns:cla=""http://www.mitchell.com/examples/claim"">
  <cla:ClaimNumber>{0}</cla:ClaimNumber>
  <cla:ClaimantFirstName>Alice</cla:ClaimantFirstName>
  <cla:ClaimantLastName>Test</cla:ClaimantLastName>
  <cla:AssignedAdjusterID>123</cla:AssignedAdjusterID>
  <cla:Vehicles>
    <cla:VehicleDetails>
      <cla:Vin>{1}</cla:Vin>
      <cla:MakeDescription>Toyota</cla:MakeDescription>
      <cla:ModelDescription>Corolla</cla:ModelDescription>
    </cla:VehicleDetails>
  </cla:Vehicles>
</cla:MitchellClaim>", newClaim.ToString(), VIN),
                                    Encoding.UTF8,
                                    "text/xml");
                    requestAddClaim.Content = newClaimContent;

                    HttpResponseMessage httpResponseMessage = client.SendAsync(requestAddClaim).Result;
                    Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);

                    // retrieve the claim with the matching ClaimNumber
                    HttpRequestMessage requestGetClaim = new HttpRequestMessage(HttpMethod.Get, String.Format("api/MitchellClaims/{0}", newClaim.ToString()));
                    httpResponseMessage = client.SendAsync(requestGetClaim).Result;
                    string sResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;

                    // confirm that the xml returns expected values
                    XDocument xdoc = XDocument.Parse(sResponse);

                    XNamespace cla = "http://www.mitchell.com/examples/claim";
                    XElement xeClaim = xdoc.Element(cla + "MitchellClaim");
                    Assert.IsNotNull(xeClaim);

                    XElement xeClaimNumber = xeClaim.Element(cla + "ClaimNumber");
                    Assert.IsNotNull(xeClaimNumber);
                    Assert.AreEqual(newClaim, Guid.Parse(xeClaimNumber.Value));

                    XElement xeClaimantFirstName = xeClaim.Element(cla + "ClaimantFirstName");
                    Assert.IsNotNull(xeClaimantFirstName);
                    Assert.AreEqual("Alice", xeClaimantFirstName.Value);

                    XElement xeClaimantLastName = xeClaim.Element(cla + "ClaimantLastName");
                    Assert.IsNotNull(xeClaimantLastName);
                    Assert.AreEqual("Test", xeClaimantLastName.Value);

                    XElement xeAssignedAdjusterID = xeClaim.Element(cla + "AssignedAdjusterID");
                    Assert.IsNotNull(xeAssignedAdjusterID);
                    Assert.AreEqual("123", xeAssignedAdjusterID.Value);

                    XElement xeVehicles = xeClaim.Element(cla + "Vehicles");
                    Assert.IsNotNull(xeVehicles);

                    XElement xeVehicleDetails = xeVehicles.Element(cla + "VehicleDetails");
                    Assert.IsNotNull(xeVehicleDetails);

                    XElement xeVin = xeVehicleDetails.Element(cla + "Vin");
                    Assert.IsNotNull(xeVin);
                    Assert.AreEqual(VIN, xeVin.Value);

                    XElement xeMakeDescription = xeVehicleDetails.Element(cla + "MakeDescription");
                    Assert.IsNotNull(xeMakeDescription);
                    Assert.AreEqual("Toyota", xeMakeDescription.Value);

                    XElement xeModelDescription = xeVehicleDetails.Element(cla + "ModelDescription");
                    Assert.IsNotNull(xeModelDescription);
                    Assert.AreEqual("Corolla", xeModelDescription.Value);

                    // trying to add a duplicate claim should return an error
                    HttpRequestMessage requestAddDupClaim = new HttpRequestMessage(HttpMethod.Post, "api/MitchellClaims");
                    requestAddDupClaim.Content = newClaimContent;
                    httpResponseMessage = client.SendAsync(requestAddDupClaim).Result;
                    Assert.IsFalse(httpResponseMessage.IsSuccessStatusCode);

                    // retrieve just the vehicle
                    HttpRequestMessage requestGetVehicle = new HttpRequestMessage(HttpMethod.Get, String.Format("api/MitchellClaims/{0}/Vehicles/{1}", newClaim.ToString(), VIN));
                    httpResponseMessage = client.SendAsync(requestGetVehicle).Result;
                    sResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;

                    // confirm that the returned xml contains the vehicle data
                    xdoc = XDocument.Parse(sResponse);
                    xeVehicleDetails = xdoc.Element(cla + "VehicleDetails");
                    Assert.IsNotNull(xeVehicleDetails);

                    xeVin = xeVehicleDetails.Element(cla + "Vin");
                    Assert.IsNotNull(xeVin);
                    Assert.AreEqual(VIN, xeVin.Value);

                    xeMakeDescription = xeVehicleDetails.Element(cla + "MakeDescription");
                    Assert.IsNotNull(xeMakeDescription);
                    Assert.AreEqual("Toyota", xeMakeDescription.Value);

                    xeModelDescription = xeVehicleDetails.Element(cla + "ModelDescription");
                    Assert.IsNotNull(xeModelDescription);
                    Assert.AreEqual("Corolla", xeModelDescription.Value);

                    // partially update the claim
                    HttpRequestMessage requestUpdateClaim = new HttpRequestMessage(new HttpMethod("PATCH"), String.Format("api/MitchellClaims/{0}", newClaim.ToString()));
                    StringContent updateClaimContent = new StringContent(String.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<cla:MitchellClaim xmlns:cla=""http://www.mitchell.com/examples/claim"">
  <cla:ClaimNumber>{0}</cla:ClaimNumber>
  <cla:Vehicles>
    <cla:VehicleDetails>
      <cla:Vin>{1}</cla:Vin>
      <cla:ModelDescription>Camry</cla:ModelDescription>
    </cla:VehicleDetails>
  </cla:Vehicles>
</cla:MitchellClaim>", newClaim.ToString(), VIN), Encoding.UTF8, "text/xml");
                    requestUpdateClaim.Content = updateClaimContent;

                    httpResponseMessage = client.SendAsync(requestUpdateClaim).Result;
                    Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);

                    // send another GET to check that the claim was updated
                    requestGetClaim = new HttpRequestMessage(HttpMethod.Get, String.Format("api/MitchellClaims/{0}", newClaim.ToString()));
                    httpResponseMessage = client.SendAsync(requestGetClaim).Result;
                    sResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;

                    xdoc = XDocument.Parse(sResponse);

                    xeClaim = xdoc.Element(cla + "MitchellClaim");
                    Assert.IsNotNull(xeClaim);

                    xeVehicles = xeClaim.Element(cla + "Vehicles");
                    Assert.IsNotNull(xeVehicles);

                    xeVehicleDetails = xeVehicles.Element(cla + "VehicleDetails");
                    Assert.IsNotNull(xeVehicleDetails);

                    // check that the Model was updated
                    xeModelDescription = xeVehicleDetails.Element(cla + "ModelDescription");
                    Assert.IsNotNull(xeModelDescription);
                    Assert.AreEqual("Camry", xeModelDescription.Value);

                    // check some other fields to make sure they were not updated
                    xeClaimantFirstName = xeClaim.Element(cla + "ClaimantFirstName");
                    Assert.IsNotNull(xeClaimantFirstName);
                    Assert.AreEqual("Alice", xeClaimantFirstName.Value);

                    xeClaimantLastName = xeClaim.Element(cla + "ClaimantLastName");
                    Assert.IsNotNull(xeClaimantLastName);
                    Assert.AreEqual("Test", xeClaimantLastName.Value);

                    xeMakeDescription = xeVehicleDetails.Element(cla + "MakeDescription");
                    Assert.IsNotNull(xeMakeDescription);
                    Assert.AreEqual("Toyota", xeMakeDescription.Value);

                    // delete the claim
                    HttpRequestMessage requestDeleteClaim = new HttpRequestMessage(HttpMethod.Delete, String.Format("api/MitchellClaims/{0}", newClaim.ToString()));
                    httpResponseMessage = client.SendAsync(requestDeleteClaim).Result;
                    Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);

                    // trying to get the claim again should return an error
                    requestGetClaim = new HttpRequestMessage(HttpMethod.Get, String.Format("api/MitchellClaims/{0}", newClaim.ToString()));
                    httpResponseMessage = client.SendAsync(requestGetClaim).Result;
                    Assert.IsFalse(httpResponseMessage.IsSuccessStatusCode);
                }
            }
        }

        /// <summary>
        /// Find a list of claims in the backing store by date range of the LossDate.
        /// </summary>
        [TestMethod]
        public void SearchByLossDate()
        {
            var config = new HttpConfiguration();
            WebApiConfig.Setup(config);

            const string CLAIM_FORMAT = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<cla:MitchellClaim xmlns:cla=""http://www.mitchell.com/examples/claim"">
  <cla:ClaimNumber>{0}</cla:ClaimNumber>
  <cla:LossDate>{1}</cla:LossDate>
</cla:MitchellClaim>";

            using (HttpServer server = new HttpServer(config))
            {
                var valuesUri = new Uri(new Uri("http://localhost"), "api/MitchellClaims");
                using (var client = new HttpClient(server))
                {
                    client.BaseAddress = new Uri("http://localhost/");
                    // add a bunch of claims to test with
                    // Claim A - loss date 1/10/3015
                    Guid claimNumberA = Guid.NewGuid();
                    HttpRequestMessage requestAddClaimA = new HttpRequestMessage(HttpMethod.Post, "api/MitchellClaims");
                    StringContent claimA = new StringContent(String.Format(CLAIM_FORMAT, claimNumberA.ToString(), new DateTime(2015, 1, 10).ToString("o")),
                                    Encoding.UTF8,
                                    "text/xml");
                    requestAddClaimA.Content = claimA;
                    HttpResponseMessage httpResponseMessage = client.SendAsync(requestAddClaimA).Result;
                    Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);


                    // Claim B - loss date 1/13/3015
                    Guid claimNumberB = Guid.NewGuid();
                    HttpRequestMessage requestAddClaimB = new HttpRequestMessage(HttpMethod.Post, "api/MitchellClaims");
                    StringContent claimB = new StringContent(String.Format(CLAIM_FORMAT, claimNumberB.ToString(), new DateTime(2015, 1, 13).ToString("o")),
                                    Encoding.UTF8,
                                    "text/xml");
                    requestAddClaimB.Content = claimB;
                    httpResponseMessage = client.SendAsync(requestAddClaimB).Result;
                    Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);


                    // Claim C - loss date 1/15/3015
                    Guid claimNumberC = Guid.NewGuid();
                    HttpRequestMessage requestAddClaimC = new HttpRequestMessage(HttpMethod.Post, "api/MitchellClaims");
                    StringContent claimC = new StringContent(String.Format(CLAIM_FORMAT, claimNumberC.ToString(), new DateTime(2015, 1, 15).ToString("o")),
                                    Encoding.UTF8,
                                    "text/xml");
                    requestAddClaimC.Content = claimC;
                    httpResponseMessage = client.SendAsync(requestAddClaimC).Result;
                    Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);


                    // Claim D - no loss date
                    Guid claimNumberD = Guid.NewGuid();
                    HttpRequestMessage requestAddClaimD = new HttpRequestMessage(HttpMethod.Post, "api/MitchellClaims");
                    StringContent claimD = new StringContent(String.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<cla:MitchellClaim xmlns:cla=""http://www.mitchell.com/examples/claim"">
  <cla:ClaimNumber>{0}</cla:ClaimNumber>
</cla:MitchellClaim>", claimNumberD.ToString()),
                                    Encoding.UTF8,
                                    "text/xml");
                    requestAddClaimD.Content = claimD;
                    httpResponseMessage = client.SendAsync(requestAddClaimD).Result;
                    Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);

                    // search by loss date range
                    DateTime dtStart = new DateTime(2015, 1, 13);
                    DateTime dtEnd = new DateTime(2015, 1, 15);
                    HttpRequestMessage requestSearchClaims = new HttpRequestMessage(HttpMethod.Get, String.Format("api/MitchellClaims/?lossStartDate={0}&lossEndDate={1}", dtStart.ToShortDateString(), dtEnd.ToShortDateString()));
                    httpResponseMessage = client.SendAsync(requestSearchClaims).Result;
                    string sResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;

                    // confirm that the xml returns expected values
                    XDocument xdoc = XDocument.Parse(sResponse);
                    
                    XNamespace cla = "http://www.mitchell.com/examples/claim";
                    // should return Claim B and Claim C
                    int nNumClaims = xdoc.Descendants(cla + "MitchellClaim").Count();
                    Assert.AreEqual(2, nNumClaims);

                    XElement xeClaimB = xdoc.Descendants(cla + "ClaimNumber").FirstOrDefault(x=> x.Value == claimNumberB.ToString());
                    Assert.IsNotNull(xeClaimB);

                    XElement xeClaimC = xdoc.Descendants(cla + "ClaimNumber").FirstOrDefault(x => x.Value == claimNumberC.ToString());
                    Assert.IsNotNull(xeClaimC);
                }
            }
        }
    }
}
