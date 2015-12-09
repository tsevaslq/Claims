using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Claims;

namespace Claims.Controllers
{
    // I used the template code for Web API Controller
    /// <summary>
    /// Manages Claims
    /// </summary>
    public class MitchellClaimsController : ApiController
    {
        private IClaimManager m_manager;

        public MitchellClaimsController()
        {
            m_manager = new MitchellClaimsManager();
        }

        public MitchellClaimsController(IClaimManager a_manager)
        {
            m_manager = a_manager;
        }

        // GET: api/MitchellClaims
        /// <summary>
        /// Gets all Claims
        /// </summary>
        /// <returns>All Claims</returns>
        public IQueryable<MitchellClaim> GetMitchellClaims()
        {
            return m_manager.GetAllClaims();
        }

        // GET: api/MitchellClaims/{GUID}
        /// <summary>
        /// Gets claim with the given ClaimNumber
        /// </summary>
        /// <param name="id">ClaimNumber</param>
        /// <returns>Claim with matching ClaimNumber</returns>
        [ResponseType(typeof(MitchellClaim))]
        public IHttpActionResult GetMitchellClaim(Guid id)
        {
            MitchellClaim mitchellClaim = m_manager.GetClaim(id);
            if (mitchellClaim == null)
            {
                return NotFound();
            }

            return Ok(mitchellClaim);
        }
        
        /// <summary>
        /// Retrieves claims with a LossDate within the date range. The time portion is ignored.
        /// </summary>
        /// <param name="lossStartDate">start of search range</param>
        /// <param name="lossEndDate">end of search range</param>
        /// <returns>Claims with a LossDate within the date range</returns>
        public IQueryable<MitchellClaim> GetMitchellClaimByLossDate(DateTime lossStartDate, DateTime lossEndDate)
        {
            // I decided to not allow the time portion to be specified
            // so I can set the date range to start at the beginning of the start date
            // and the end of the end date (beginning of the next day).
            DateTime dtStart = lossStartDate.Date; // zero out the time portion
            DateTime dtEnd = lossEndDate.Date;
            dtEnd = lossEndDate.Date.AddDays(1);
            return m_manager.GetClaimsByLossDate(dtStart, dtEnd, false);
        }

        /// <summary>
        /// Gets vehicles for claim with given ClaimNumber
        /// </summary>
        /// <param name="id">ClaimNumber</param>
        /// <returns>List of Vehicles</returns>
        [Route("api/MitchellClaims/{id:guid}/Vehicles")]
        [HttpGet]
        public IHttpActionResult GetMitchellClaimVehicles(Guid id)
        {
            MitchellClaim mitchellClaim = m_manager.GetClaim(id);
            if (mitchellClaim == null)
            {
                return NotFound();
            }

            return Ok(mitchellClaim.VehicleDetails);
        }

        /// <summary>
        /// Gets Vehicle with matching VIN for claim with matching ClaimNumber
        /// </summary>
        /// <param name="id">ClaimNumber</param>
        /// <param name="vin">VIN</param>
        /// <returns>Vehicle</returns>
        [Route("api/MitchellClaims/{id:guid}/Vehicles/{vin}")]
        [HttpGet]
        public IHttpActionResult GetMitchellClaimVehicle(Guid id, string vin)
        {
            VehicleDetail vehicle = m_manager.GetClaimVehicle(id, vin);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        // PUT: api/MitchellClaims/{guid}
        // PUT will completely replace the claim with matching guid.
        // Will return 404 if claim with matching guid does not exist.
        /// <summary>
        /// Replaces claim in DB with given claim
        /// </summary>
        /// <param name="id">ClaimNumber</param>
        /// <param name="mitchellClaim">new Claim</param>
        /// <returns>NoContent</returns>
        [ResponseType(typeof(void))]
        public IHttpActionResult PutMitchellClaim(Guid id, MitchellClaim mitchellClaim)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // if null, the parsing may have failed
            if (mitchellClaim == null)
                return BadRequest("Input is invalid");

            if (id != mitchellClaim.ClaimNumber)
            {
                return BadRequest();
            }

            try
            {
                m_manager.UpdateClaim(mitchellClaim);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!m_manager.ClaimExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PATCH: api/MitchellClaims/{guid}
        // PATCH will partial update existing claim with matching guid.
        // Will return 404 if claim with matching guid does not exist.
        /// <summary>
        /// Will update claim in DB with provided fields
        /// </summary>
        /// <param name="id">ClaimNumber</param>
        /// <param name="mitchellClaim">Claim with fields to update</param>
        /// <returns>NoContent</returns>
        [ResponseType(typeof(void))]
        public IHttpActionResult PatchMitchellClaim(Guid id, MitchellClaim mitchellClaim)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // if null, the parsing may have failed
            if (mitchellClaim == null)
                return BadRequest("Input is invalid");

            if (id != mitchellClaim.ClaimNumber)
            {
                return BadRequest();
            }

            // get claim from DB
            MitchellClaim dbClaim = m_manager.GetClaim(id);

            if (dbClaim == null)
                return NotFound(); // can't update non-existent claim

            // update the claim from DB with the values provided in the input claim
            if (mitchellClaim.ClaimantFirstName != null)
                dbClaim.ClaimantFirstName = mitchellClaim.ClaimantFirstName;
            if (mitchellClaim.ClaimantLastName != null)
                dbClaim.ClaimantLastName = mitchellClaim.ClaimantLastName;
            if (mitchellClaim.Status.HasValue)
                dbClaim.Status = mitchellClaim.Status;
            if (mitchellClaim.LossDate.HasValue)
                dbClaim.LossDate = mitchellClaim.LossDate;
            if (mitchellClaim.CauseOfLoss.HasValue)
                dbClaim.CauseOfLoss = mitchellClaim.CauseOfLoss;
            if (mitchellClaim.ReportedDate.HasValue)
                dbClaim.ReportedDate = mitchellClaim.ReportedDate;
            if (mitchellClaim.LossDescription != null)
                dbClaim.LossDescription = mitchellClaim.LossDescription;
            if (mitchellClaim.AssignedAdjusterID.HasValue)
                dbClaim.AssignedAdjusterID = mitchellClaim.AssignedAdjusterID;

            if (mitchellClaim.VehicleDetails != null && mitchellClaim.VehicleDetails.Count() > 0)
            {
                foreach (var vehicle in mitchellClaim.VehicleDetails)
                {
                    VehicleDetail dbVehicle = dbClaim.VehicleDetails.FirstOrDefault(x => x.Vin == vehicle.Vin);
                    if (dbVehicle == null)
                    {
                        // PATCH should only update existing items (including existing Vehicles)
                        return BadRequest();
                    }
                    if (vehicle.ModelYear.HasValue)
                        dbVehicle.ModelYear = vehicle.ModelYear;
                    if (vehicle.MakeDescription != null)
                        dbVehicle.MakeDescription = vehicle.MakeDescription;
                    if (vehicle.ModelDescription != null)
                        dbVehicle.ModelDescription = vehicle.ModelDescription;
                    if (vehicle.EngineDescription != null)
                        dbVehicle.EngineDescription = vehicle.EngineDescription;
                    if (vehicle.ExteriorColor != null)
                        dbVehicle.ExteriorColor = vehicle.ExteriorColor;
                    if (vehicle.LicPlate != null)
                        dbVehicle.LicPlate = vehicle.LicPlate;
                    if (vehicle.LicPlateState != null)
                        dbVehicle.LicPlateState = vehicle.LicPlateState;
                    if (vehicle.LicPlateExpDate.HasValue)
                        dbVehicle.LicPlateExpDate = vehicle.LicPlateExpDate;
                    if (vehicle.DamageDescription != null)
                        dbVehicle.DamageDescription = vehicle.DamageDescription;
                    if (vehicle.Mileage.HasValue)
                        dbVehicle.Mileage = vehicle.Mileage;
                }
            }

            try
            {
                m_manager.UpdateClaim(mitchellClaim);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!m_manager.ClaimExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/MitchellClaims
        // New claim creations should use POST.
        // If the ClaimNumber matches an existing claim, it will return an error
        /// <summary>
        /// Adds new claim to DB
        /// </summary>
        /// <param name="mitchellClaim">New claim to add</param>
        /// <returns></returns>
        [ResponseType(typeof(MitchellClaim))]
        public IHttpActionResult PostMitchellClaim(MitchellClaim mitchellClaim)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // if null, the parsing may have failed
            if (mitchellClaim == null)
                return BadRequest("Input is invalid"); // ideally would send a more descriptive error message

            if (m_manager.ClaimExists(mitchellClaim.ClaimNumber))
                return BadRequest("Claim with duplicate ClaimNumber already exists.");
            m_manager.AddClaim(mitchellClaim);

            return CreatedAtRoute("DefaultApi", new { id = mitchellClaim.ClaimNumber }, mitchellClaim);
        }

        // DELETE: api/MitchellClaims/{guid}
        /// <summary>
        /// Deletes claim from DB
        /// </summary>
        /// <param name="id">ClaimNumber</param>
        /// <returns>deleted Claim</returns>
        [ResponseType(typeof(MitchellClaim))]
        public IHttpActionResult DeleteMitchellClaim(Guid id)
        {
            MitchellClaim mitchellClaim = m_manager.GetClaim(id);
            if (mitchellClaim == null)
            {
                return NotFound();
            }

            m_manager.DeleteClaim(mitchellClaim);

            return Ok(mitchellClaim);
        }
    }
}