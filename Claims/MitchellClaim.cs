//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Claims
{
    using System;
    using System.Collections.Generic;
    
    public partial class MitchellClaim
    {
        public MitchellClaim()
        {
            this.VehicleDetails = new HashSet<VehicleDetail>();
        }
    
        public System.Guid ClaimNumber { get; set; }
        public string ClaimantFirstName { get; set; }
        public string ClaimantLastName { get; set; }
        public Nullable<Status> Status { get; set; }
        public Nullable<System.DateTime> LossDate { get; set; }
        public Nullable<CauseOfLossCode> CauseOfLoss { get; set; }
        public Nullable<System.DateTime> ReportedDate { get; set; }
        public string LossDescription { get; set; }
        public Nullable<long> AssignedAdjusterID { get; set; }
    
        public virtual ICollection<VehicleDetail> VehicleDetails { get; set; }
    }
}
