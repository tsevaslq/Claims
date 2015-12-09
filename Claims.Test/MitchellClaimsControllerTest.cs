using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Claims.Controllers;
using Moq;
using System.Collections.Generic;
using System.Web.Http.Results;

namespace Claims.Test
{
    [TestClass]
    public class MitchellClaimsControllerTest
    {
        [TestMethod]
        public void TestAddExistingClaim()
        {
            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.ClaimExists(It.IsAny<Guid>())).Returns(true);

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            IHttpActionResult result = controller.PostMitchellClaim(claim);
            Assert.IsTrue(result is BadRequestErrorMessageResult);
        }
        [TestMethod]
        public void TestAddNewClaim()
        {
            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.ClaimExists(It.IsAny<Guid>())).Returns(false);

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            IHttpActionResult result = controller.PostMitchellClaim(claim);

            CreatedAtRouteNegotiatedContentResult<MitchellClaim> contentResult = result as CreatedAtRouteNegotiatedContentResult<MitchellClaim>;
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(claim.ClaimNumber, contentResult.Content.ClaimNumber);
        }
        [TestMethod]
        public void TestDeleteNonExistentClaim()
        {
            var mock = new Mock<IClaimManager>();

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            IHttpActionResult result = controller.DeleteMitchellClaim(Guid.NewGuid());
            Assert.IsTrue(result is NotFoundResult);
        }
        [TestMethod]
        public void TestDeleteClaim()
        {
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.GetClaim(claim.ClaimNumber)).Returns(claim);

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            IHttpActionResult result = controller.DeleteMitchellClaim(claim.ClaimNumber);
            OkNegotiatedContentResult<MitchellClaim> contentResult = result as OkNegotiatedContentResult<MitchellClaim>;
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(claim.ClaimNumber, contentResult.Content.ClaimNumber);
        }
        [TestMethod]
        public void TestGetClaim()
        {
            Guid claimNumber = Guid.NewGuid();
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = claimNumber;

            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.GetClaim(claimNumber)).Returns(claim);
            
            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            IHttpActionResult result = controller.GetMitchellClaim(claimNumber);

            OkNegotiatedContentResult<MitchellClaim> contentResult = result as OkNegotiatedContentResult<MitchellClaim>;
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(claimNumber, contentResult.Content.ClaimNumber);
        }
        [TestMethod]
        public void TestGetNonexistentClaim()
        {
            Guid claimNumber = Guid.NewGuid();

            var mock = new Mock<IClaimManager>();

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            IHttpActionResult result = controller.GetMitchellClaim(claimNumber);

            Assert.IsTrue(result is NotFoundResult);
        }
        [TestMethod]
        public void TestGetAllClaims()
        {
            List<MitchellClaim> lstClaims = new List<MitchellClaim>();
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            lstClaims.Add(claim);

            MitchellClaim claim2 = new MitchellClaim();
            claim2.ClaimNumber = Guid.NewGuid();
            lstClaims.Add(claim2);

            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.GetAllClaims()).Returns(lstClaims.AsQueryable());

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            IQueryable<MitchellClaim> result = controller.GetMitchellClaims();

            Assert.IsNotNull(result);
            Assert.AreEqual(lstClaims.Count(), result.Count());
            Assert.IsTrue(result.Any(x => x.ClaimNumber == claim.ClaimNumber));
            Assert.IsTrue(result.Any(x => x.ClaimNumber == claim2.ClaimNumber));
        }
        [TestMethod]
        public void TestGetAllClaimsWhenEmpty()
        {
            var mock = new Mock<IClaimManager>();

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            IQueryable<MitchellClaim> result = controller.GetMitchellClaims();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
        [TestMethod]
        public void TestGetClaimsByLossDate()
        {
            List<MitchellClaim> lstClaims = new List<MitchellClaim>();
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            claim.LossDate = new DateTime(2015, 1, 10);
            lstClaims.Add(claim);

            MitchellClaim claim2 = new MitchellClaim();
            claim2.ClaimNumber = Guid.NewGuid();
            claim2.LossDate = new DateTime(2015, 1, 15);
            lstClaims.Add(claim2);

            DateTime dtStart = new DateTime(2015, 1, 10);
            DateTime dtEnd = new DateTime(2015, 1, 20);

            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.GetClaimsByLossDate(dtStart, dtEnd.AddDays(1), false)).Returns(lstClaims.AsQueryable());

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            IQueryable<MitchellClaim> result = controller.GetMitchellClaimByLossDate(dtStart, dtEnd);

            Assert.IsNotNull(result);
            Assert.AreEqual(lstClaims.Count(), result.Count());
            Assert.IsTrue(result.Any(x => x.ClaimNumber == claim.ClaimNumber));
            Assert.IsTrue(result.Any(x => x.ClaimNumber == claim2.ClaimNumber));
        }
        [TestMethod]
        public void TestGetClaimSpecificVehicles()
        {
            Guid claimNumber = Guid.NewGuid();
            VehicleDetail vehicle = new VehicleDetail();
            const string VIN = "12345";
            vehicle.Vin = VIN;
            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.GetClaimVehicle(claimNumber, VIN)).Returns(vehicle);

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            IHttpActionResult result = controller.GetMitchellClaimVehicle(claimNumber, VIN);

            OkNegotiatedContentResult<VehicleDetail> contentResult = result as OkNegotiatedContentResult<VehicleDetail>;
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(VIN, contentResult.Content.Vin);
        }
        [TestMethod]
        public void TestReplaceClaim()
        {
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            var mock = new Mock<IClaimManager>();

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            IHttpActionResult result = controller.PutMitchellClaim(claim.ClaimNumber, claim);
            StatusCodeResult status = result as StatusCodeResult;
            Assert.IsNotNull(status);
            Assert.AreEqual(System.Net.HttpStatusCode.NoContent, status.StatusCode);
        }
        [TestMethod]
        public void TestReplaceClaimBadRequest()
        {
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            var mock = new Mock<IClaimManager>();

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            IHttpActionResult result = controller.PutMitchellClaim(Guid.NewGuid(), claim);
            BadRequestResult badRequest = result as BadRequestResult;
            Assert.IsNotNull(badRequest);
        }

        [TestMethod]
        public void TestUpdateClaim()
        {
            MitchellClaim claim = new MitchellClaim();
            claim.ClaimNumber = Guid.NewGuid();
            claim.ClaimantFirstName = "Alice";
            claim.ClaimantLastName = "Test";
            var mock = new Mock<IClaimManager>();
            mock.Setup(x => x.GetClaim(claim.ClaimNumber)).Returns(claim);

            MitchellClaim claim2 = new MitchellClaim();
            claim2.ClaimNumber = claim.ClaimNumber;
            claim2.ClaimantFirstName = "Bob";

            MitchellClaimsController controller = new MitchellClaimsController(mock.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            IHttpActionResult result = controller.PatchMitchellClaim(claim.ClaimNumber, claim2);
            StatusCodeResult status = result as StatusCodeResult;
            Assert.IsNotNull(status);
            Assert.AreEqual(System.Net.HttpStatusCode.NoContent, status.StatusCode);
        }
    }
}
