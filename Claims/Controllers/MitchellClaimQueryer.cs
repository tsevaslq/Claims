using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Claims.Controllers
{
    public class MitchellClaimQueryer
    {
        private IQueryable<MitchellClaim> m_iQueryable;
        public MitchellClaimQueryer(IQueryable<MitchellClaim> a_iQueryable)
        {
            m_iQueryable = a_iQueryable;
        }

        public IQueryable<MitchellClaim> GetAllClaims()
        {
            return m_iQueryable;
        }

        public MitchellClaim GetClaim(Guid claimNumber)
        {
            return m_iQueryable.FirstOrDefault(x => x.ClaimNumber == claimNumber);
        }

        /// <summary>
        /// Returns claims that are greater than the lossStartDate and less than lossEndDate
        /// </summary>
        /// <param name="lossStartDate">start of date range</param>
        /// <param name="lossEndDate">end of date range</param>
        /// <param name="a_bEndDateInclusive">Whether the date range includes the end date</param>
        /// <returns>claims within date range</returns>
        public IQueryable<MitchellClaim> GetClaimsByLossDate(DateTime lossStartDate, DateTime lossEndDate, bool a_bEndDateInclusive)
        {
            if (a_bEndDateInclusive)
                return m_iQueryable.Where(x => x.LossDate >= lossStartDate && x.LossDate <= lossEndDate);
            
            // excludes end date
            return m_iQueryable.Where(x => x.LossDate >= lossStartDate && x.LossDate < lossEndDate);
        }

        public VehicleDetail GetClaimVehicle(Guid id, string vin)
        {
            MitchellClaim mitchellClaim = m_iQueryable.FirstOrDefault(x => x.ClaimNumber == id);
            if (mitchellClaim == null || mitchellClaim.VehicleDetails == null)
            {
                return null;
            }

            return mitchellClaim.VehicleDetails.FirstOrDefault(x => x.Vin == vin);
        }

        public bool ClaimExists(Guid id)
        {
            return m_iQueryable.Count(e => e.ClaimNumber == id) > 0;
        }
    }
}