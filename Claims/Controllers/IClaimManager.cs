using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Controllers
{
    public interface IClaimManager
    {
        IQueryable<MitchellClaim> GetAllClaims();

        MitchellClaim GetClaim(Guid claimNumber);

        IQueryable<MitchellClaim> GetClaimsByLossDate(DateTime lossStartDate, DateTime lossEndDate, bool a_bEndDateInclusive);

        bool ClaimExists(Guid id);

        VehicleDetail GetClaimVehicle(Guid id, string vin);

        void AddClaim(MitchellClaim mitchellClaim);

        void DeleteClaim(MitchellClaim mitchellClaim);

        void UpdateClaim(MitchellClaim mitchellClaim);
    }
}
