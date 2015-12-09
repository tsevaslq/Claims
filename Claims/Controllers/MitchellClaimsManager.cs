using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Claims.Controllers
{
    public class MitchellClaimsManager : IDisposable, IClaimManager
    {
        private MitchellClaimsEntities db = new MitchellClaimsEntities();
        private MitchellClaimQueryer m_queryer;

        public MitchellClaimsManager()
        {
            m_queryer = new MitchellClaimQueryer(db.MitchellClaims.Include(x => x.VehicleDetails));
        }

        public IQueryable<MitchellClaim> GetAllClaims()
        {
            return m_queryer.GetAllClaims();
        }

        public MitchellClaim GetClaim(Guid claimNumber)
        {
            return m_queryer.GetClaim(claimNumber);
        }

        public IQueryable<MitchellClaim> GetClaimsByLossDate(DateTime lossStartDate, DateTime lossEndDate, bool a_bEndDateInclusive)
        {
            return m_queryer.GetClaimsByLossDate(lossStartDate, lossEndDate, a_bEndDateInclusive);
        }

        public bool ClaimExists(Guid id)
        {
            return m_queryer.ClaimExists(id);
        }

        public VehicleDetail GetClaimVehicle(Guid id, string vin)
        {
            return m_queryer.GetClaimVehicle(id, vin);
        }

        public void AddClaim(MitchellClaim mitchellClaim)
        {
            db.MitchellClaims.Add(mitchellClaim);
            db.SaveChanges();
        }

        public void DeleteClaim(MitchellClaim mitchellClaim)
        {
            db.MitchellClaims.Remove(mitchellClaim);
            db.SaveChanges();
        }

        public void UpdateClaim(MitchellClaim mitchellClaim)
        {
            db.Entry(mitchellClaim).State = EntityState.Modified;
            db.SaveChanges();
        }

        void IDisposable.Dispose()
        {
            if (db != null)
                db.Dispose();
        }
    }
}