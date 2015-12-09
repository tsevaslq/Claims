using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Claims.Controllers;

namespace Claims.Test
{
    [TestClass]
    public class MitchellClaimQueryerTest
    {
        [TestMethod]
        public void TestGetAllClaims()
        {
            List<MitchellClaim> lstClaims = new List<MitchellClaim>();
            lstClaims.Add(new MitchellClaim());
            lstClaims.Add(new MitchellClaim());

            // test retrieving all claims
            MitchellClaimQueryer queryer = new MitchellClaimQueryer(lstClaims.AsQueryable());
            IQueryable<MitchellClaim> iquerAll = queryer.GetAllClaims();
            Assert.AreEqual(iquerAll.Count(), lstClaims.Count());
        }
        [TestMethod]
        public void TestGetClaim()
        {
            List<MitchellClaim> lstClaims = new List<MitchellClaim>();
            MitchellClaim claimTest = new MitchellClaim();
            Guid claimNumberTest = Guid.NewGuid();
            claimTest.ClaimNumber = claimNumberTest;
            lstClaims.Add(claimTest);

            // test retrieving specific claim
            MitchellClaimQueryer queryer = new MitchellClaimQueryer(lstClaims.AsQueryable());
            MitchellClaim getClaim = queryer.GetClaim(claimNumberTest);
            Assert.IsNotNull(getClaim);
            Assert.AreEqual(getClaim, claimTest);
        }
        [TestMethod]
        public void TestGetClaimsByLossDate()
        {
            List<MitchellClaim> lstClaims = new List<MitchellClaim>();
            MitchellClaim claimTestNoLossDate = new MitchellClaim();
            claimTestNoLossDate.ClaimNumber = Guid.NewGuid();
            lstClaims.Add(claimTestNoLossDate);

            MitchellClaim claimTestA = new MitchellClaim();
            claimTestA.ClaimNumber = Guid.NewGuid();
            claimTestA.LossDate = new DateTime(2015, 5, 10, 4, 30, 0);
            lstClaims.Add(claimTestA);

            MitchellClaim claimTestB = new MitchellClaim();
            claimTestB.LossDate = new DateTime(2015, 5, 13, 8, 10, 0);
            claimTestB.ClaimNumber = Guid.NewGuid();
            lstClaims.Add(claimTestB);

            MitchellClaim claimTestC = new MitchellClaim();
            claimTestC.LossDate = new DateTime(2015, 5, 13, 20, 40, 0);
            claimTestC.ClaimNumber = Guid.NewGuid();
            lstClaims.Add(claimTestC);

            MitchellClaim claimTestD = new MitchellClaim();
            claimTestD.LossDate = new DateTime(2015, 5, 15, 11, 6, 0);
            claimTestD.ClaimNumber = Guid.NewGuid();
            lstClaims.Add(claimTestD);

            // test retrieving by loss datetime range
            MitchellClaimQueryer queryer = new MitchellClaimQueryer(lstClaims.AsQueryable());
            DateTime dtStart = new DateTime(2015, 5, 1, 12, 0, 0);
            DateTime dtEnd = new DateTime(2015, 5, 13, 12, 0, 0);
            IQueryable<MitchellClaim> results = queryer.GetClaimsByLossDate(dtStart, dtEnd, true);
            Assert.AreEqual(results.Count(), 2);
            Assert.IsTrue(results.Contains(claimTestA));
            Assert.IsTrue(results.Contains(claimTestB));

            // invalid date range
            dtStart = new DateTime(2015, 10, 1);
            dtEnd = new DateTime(2015, 1, 1);
            results = queryer.GetClaimsByLossDate(dtStart, dtEnd, true);
            Assert.AreEqual(results.Count(), 0);

            // the search date range includes the start date
            dtStart = new DateTime(2015, 5, 10, 4, 30, 0);
            dtEnd = new DateTime(2015, 5, 10, 5, 0, 0);
            results = queryer.GetClaimsByLossDate(dtStart, dtEnd, true);
            Assert.AreEqual(results.Count(), 1);
            Assert.IsTrue(results.Contains(claimTestA));

            // the search date range includes the end date
            dtStart = new DateTime(2015, 5, 10, 1, 30, 0);
            dtEnd = new DateTime(2015, 5, 10, 4, 30, 0);
            results = queryer.GetClaimsByLossDate(dtStart, dtEnd, true);
            Assert.AreEqual(results.Count(), 1);
            Assert.IsTrue(results.Contains(claimTestA));

            // the search date range excludes the end date
            dtStart = new DateTime(2015, 5, 10, 1, 30, 0);
            dtEnd = new DateTime(2015, 5, 10, 4, 30, 0);
            results = queryer.GetClaimsByLossDate(dtStart, dtEnd, false);
            Assert.AreEqual(results.Count(), 0);
        }
        [TestMethod]
        public void TestGetClaimVehicle()
        {
            const string TEST_VIN = "ABCDEFG1234567890";
            const string BAD_VIN = "12345";
            List<MitchellClaim> lstClaims = new List<MitchellClaim>();
            MitchellClaim claimTest = new MitchellClaim();
            Guid claimNumberTest = Guid.NewGuid();
            claimTest.ClaimNumber = claimNumberTest;
            VehicleDetail vehicle = new VehicleDetail();

            vehicle.Vin = TEST_VIN;
            claimTest.VehicleDetails = new[] { vehicle };
            lstClaims.Add(claimTest);

            MitchellClaimQueryer queryer = new MitchellClaimQueryer(lstClaims.AsQueryable());
            // querying using claim number that doesn't match should return null
            VehicleDetail getVehicle = queryer.GetClaimVehicle(Guid.NewGuid(), TEST_VIN);
            Assert.IsNull(getVehicle);

            // querying for VIN that claim doesn't have should return null
            getVehicle = queryer.GetClaimVehicle(claimNumberTest, BAD_VIN);
            Assert.IsNull(getVehicle);

            // should return correct vehicle
            getVehicle = queryer.GetClaimVehicle(claimNumberTest, TEST_VIN);
            Assert.IsNotNull(getVehicle);
            Assert.AreEqual(getVehicle, vehicle);
        }

        [TestMethod]
        public void TestClaimExists()
        {
            List<MitchellClaim> lstClaims = new List<MitchellClaim>();
            MitchellClaim claimTest = new MitchellClaim();
            Guid claimNumberTest = Guid.NewGuid();
            claimTest.ClaimNumber = claimNumberTest;
            lstClaims.Add(claimTest);

            // test that checking non-existant claim number returns false
            MitchellClaimQueryer queryer = new MitchellClaimQueryer(lstClaims.AsQueryable());
            bool bExists = queryer.ClaimExists(Guid.NewGuid());
            Assert.IsFalse(bExists);

            // test that existing claim is correctly detected
            bExists = queryer.ClaimExists(claimNumberTest);
            Assert.IsTrue(bExists);
        }
    }
}
