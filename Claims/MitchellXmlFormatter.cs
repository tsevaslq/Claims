using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Claims
{
    // Because the MitchellClaim class was auto-generated based on the database table,
    // I didn't want to modify the class to use DataContract/XmlSerializer, 
    // but I needed to customize how the object gets serialized.
    public class MitchellXmlFormatter : BufferedMediaTypeFormatter
    {
        private XNamespace m_cla = "http://www.mitchell.com/examples/claim";

        public MitchellXmlFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
        }

        public override bool CanWriteType(Type type)
        {
            if (type == typeof(MitchellClaim) || type == typeof(VehicleDetail))
            {
                return true;
            }
            else
            {
                Type enumClaim = typeof(IEnumerable<MitchellClaim>);
                Type enumVehicle = typeof(IEnumerable<VehicleDetail>);
                return enumClaim.IsAssignableFrom(type) || enumVehicle.IsAssignableFrom(type);
            }
        }

        public override bool CanReadType(Type type)
        {
            // only single MitchellClaim
            return type == typeof(MitchellClaim);
        }

        public override object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            using (StreamReader reader = new StreamReader(readStream))
            {
                XDocument xdoc = XDocument.Load(readStream);
                XElement xeClaim = xdoc.Element(m_cla + "MitchellClaim");
                if (xeClaim == null)
                    return null;

                XElement xeClaimNumber = xeClaim.Element(m_cla + "ClaimNumber");

                MitchellClaim claim = new MitchellClaim();
                Guid claimNumber;
                if (xeClaimNumber == null)
                    return null; // claim number is a required field
                if (!Guid.TryParse(xeClaimNumber.Value, out claimNumber))
                    return null; // claim number is invalid guid

                claim.ClaimNumber = claimNumber;

                XElement xeClaimantFirstName = xeClaim.Element(m_cla + "ClaimantFirstName");
                if (xeClaimantFirstName != null)
                    claim.ClaimantFirstName = xeClaimantFirstName.Value;

                XElement xeClaimantLastName = xeClaim.Element(m_cla + "ClaimantLastName");
                if (xeClaimantLastName != null)
                    claim.ClaimantLastName = xeClaimantLastName.Value;

                XElement xeStatus = xeClaim.Element(m_cla + "Status");
                Claims.Status eStatus;
                // leave Status blank if it was an invalid enumeration
                // since it is an optional field.
                if (xeStatus != null && Enum.TryParse<Claims.Status>(xeStatus.Value, out eStatus))
                {
                    claim.Status = eStatus;
                }

                XElement xeLossDate = xeClaim.Element(m_cla + "LossDate");
                DateTime dtLossDate;
                if (xeLossDate != null && DateTime.TryParse(xeLossDate.Value, out dtLossDate))
                    claim.LossDate = dtLossDate;

                XElement xeLossInfo = xeClaim.Element(m_cla + "LossInfo");
                if (xeLossInfo != null)
                {
                    XElement xeCauseOfLoss = xeLossInfo.Element(m_cla + "CauseOfLoss");
                    if (xeCauseOfLoss != null)
                    {
                        Claims.CauseOfLossCode eCauseOfLossCode;
                        if (Enum.TryParse<Claims.CauseOfLossCode>(xeCauseOfLoss.Value, out eCauseOfLossCode))
                            claim.CauseOfLoss = eCauseOfLossCode;

                        // need to handle Mechanical Breakdown separately because of the space between words
                        if (String.Equals(xeCauseOfLoss.Value, CauseOfLossToString(Claims.CauseOfLossCode.MechanicalBreakdown)))
                            claim.CauseOfLoss = Claims.CauseOfLossCode.MechanicalBreakdown;
                    }

                    XElement xeReportedDate = xeLossInfo.Element(m_cla + "ReportedDate");
                    DateTime dtReportedDate;
                    if (xeReportedDate != null && DateTime.TryParse(xeReportedDate.Value, out dtReportedDate))
                        claim.ReportedDate = dtReportedDate;

                    XElement xeLossDescription = xeLossInfo.Element(m_cla + "LossDescription");
                    if (xeLossDescription != null)
                        claim.LossDescription = xeLossDescription.Value;
                }

                XElement xeAssignedAdjusterID = xeClaim.Element(m_cla + "AssignedAdjusterID");
                long nAssignedAdjusterID;
                if (xeAssignedAdjusterID != null && Int64.TryParse(xeAssignedAdjusterID.Value, out nAssignedAdjusterID))
                    claim.AssignedAdjusterID = nAssignedAdjusterID;

                XElement xeVehicles = xeClaim.Element(m_cla + "Vehicles");
                if (xeVehicles != null)
                {
                    List<VehicleDetail> lstVehicleDetails = new List<VehicleDetail>();
                    foreach (var xeVehicleDetails in xeVehicles.Elements(m_cla + "VehicleDetails"))
                    {
                        XElement xeVin = xeVehicleDetails.Element(m_cla + "Vin");
                        if (xeVin == null || String.IsNullOrEmpty(xeVin.Value))
                            return null; // VIN is required

                        VehicleDetail vehicle = new VehicleDetail();
                        vehicle.Vin = xeVin.Value;

                        XElement xeModelYear = xeVehicleDetails.Element(m_cla + "ModelYear");
                        short nModelYear;
                        if (xeModelYear != null && Int16.TryParse(xeModelYear.Value, out nModelYear))
                            vehicle.ModelYear = nModelYear;

                        XElement xeMakeDescription = xeVehicleDetails.Element(m_cla + "MakeDescription");
                        if (xeMakeDescription != null)
                            vehicle.MakeDescription = xeMakeDescription.Value;

                        XElement xeModelDescription = xeVehicleDetails.Element(m_cla + "ModelDescription");
                        if (xeModelDescription != null)
                            vehicle.ModelDescription = xeModelDescription.Value;

                        XElement xeEngineDescription = xeVehicleDetails.Element(m_cla + "EngineDescription");
                        if (xeEngineDescription != null)
                            vehicle.EngineDescription = xeEngineDescription.Value;

                        XElement xeExteriorColor = xeVehicleDetails.Element(m_cla + "ExteriorColor");
                        if (xeExteriorColor != null)
                            vehicle.ExteriorColor = xeExteriorColor.Value;

                        XElement xeLicPlate = xeVehicleDetails.Element(m_cla + "LicPlate");
                        if (xeLicPlate != null)
                            vehicle.LicPlate = xeLicPlate.Value;

                        XElement xeLicPlateState = xeVehicleDetails.Element(m_cla + "LicPlateState");
                        if (xeLicPlateState != null)
                            vehicle.LicPlateState = xeLicPlateState.Value;

                        XElement xeLicPlateExpDate = xeVehicleDetails.Element(m_cla + "LicPlateExpDate");
                        DateTime dtLicPlateExpDate;
                        if (xeLicPlateExpDate != null && DateTime.TryParse(xeLicPlateExpDate.Value, out dtLicPlateExpDate))
                            vehicle.LicPlateExpDate = dtLicPlateExpDate;

                        XElement xeDamageDescription = xeVehicleDetails.Element(m_cla + "DamageDescription");
                        if (xeDamageDescription != null)
                            vehicle.DamageDescription = xeDamageDescription.Value;

                        XElement xeMileage = xeVehicleDetails.Element(m_cla + "Mileage");
                        int nMileage;
                        if (xeMileage != null && Int32.TryParse(xeMileage.Value, out nMileage))
                            vehicle.Mileage = nMileage;

                        lstVehicleDetails.Add(vehicle);
                    }

                    if (lstVehicleDetails.Count > 0)
                        claim.VehicleDetails = lstVehicleDetails;
                }
                return claim;
            }
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            XDocument doc = new XDocument();
            var claims = value as IEnumerable<MitchellClaim>;
            if (claims != null)
            {
                // if we're serializing an IEnumerable,
                // add a MitchellClaimsList element to serve as the root
                // (even if there are one or zero elements)
                var list = new XElement("MitchellClaimsList");
                doc.Add(list);
                foreach (var claim in claims)
                {
                    list.Add(Serialize(claim));
                }
            }
            else if (value is MitchellClaim)
            {
                var claim = value as MitchellClaim;
                doc.Add(Serialize(claim));
            }
            else if (value is IEnumerable<VehicleDetail>)
            {
                var vehicles = value as IEnumerable<VehicleDetail>;
                doc.Add(Serialize(vehicles));
            }
            else if (value is VehicleDetail)
            {
                var vehicle = value as VehicleDetail;
                doc.Add(Serialize(vehicle));
            }
            else
            {
                throw new InvalidOperationException("Cannot serialize type");
            }

            doc.Save(writeStream);
        }

        private XElement Serialize(MitchellClaim claim)
        {
            var xeClaim = new XElement(m_cla + "MitchellClaim",
                new XAttribute(XNamespace.Xmlns + "cla", "http://www.mitchell.com/examples/claim"),
                new XElement(m_cla + "ClaimNumber", claim.ClaimNumber.ToString())); // the only required field
            AddOptionalElement(xeClaim, "ClaimantFirstName", claim.ClaimantFirstName);
            AddOptionalElement(xeClaim, "ClaimantLastName", claim.ClaimantLastName);

            if (claim.Status.HasValue)
                xeClaim.Add(new XElement(m_cla + "Status", claim.Status.Value.ToString()));

            if (claim.LossDate.HasValue)
                xeClaim.Add(new XElement(m_cla + "LossDate", claim.LossDate.Value.ToString("o")));

            if (claim.CauseOfLoss.HasValue || claim.ReportedDate.HasValue || !String.IsNullOrEmpty(claim.LossDescription))
            {
                var xeLossInfo = new XElement(m_cla + "LossInfo");
                xeClaim.Add(xeLossInfo);

                if (claim.CauseOfLoss.HasValue)
                    xeLossInfo.Add(new XElement(m_cla + "CauseOfLoss", CauseOfLossToString(claim.CauseOfLoss.Value)));

                if (claim.ReportedDate.HasValue)
                    xeLossInfo.Add(new XElement(m_cla + "ReportedDate", claim.ReportedDate.Value.ToString("o")));

                AddOptionalElement(xeLossInfo, "LossDescription", claim.LossDescription);
            }

            if (claim.AssignedAdjusterID.HasValue)
                xeClaim.Add(new XElement(m_cla + "AssignedAdjusterID", claim.AssignedAdjusterID.Value.ToString()));

            if (claim.VehicleDetails != null && claim.VehicleDetails.Count() > 0)
            {
                XElement xeVehicles = Serialize(claim.VehicleDetails);
                xeClaim.Add(xeVehicles);
            }

            return xeClaim;
        }

        private XElement Serialize(IEnumerable<VehicleDetail> vehicleDetails)
        {
            XElement xeVehicles = new XElement(m_cla + "Vehicles");
            foreach (var vehicle in vehicleDetails)
            {
                XElement xeVehicleDetails = Serialize(vehicle);
                xeVehicles.Add(xeVehicleDetails);
            }
            return xeVehicles;
        }

        private XElement Serialize(VehicleDetail vehicle)
        {
            XElement xeVehicleDetails = new XElement(m_cla + "VehicleDetails");

            xeVehicleDetails.Add(new XElement(m_cla + "Vin", vehicle.Vin)); // VIN is required field

            if (vehicle.ModelYear.HasValue)
                xeVehicleDetails.Add(new XElement(m_cla + "ModelYear", vehicle.ModelYear.Value.ToString()));

            AddOptionalElement(xeVehicleDetails, "MakeDescription", vehicle.MakeDescription);

            AddOptionalElement(xeVehicleDetails, "ModelDescription", vehicle.ModelDescription);

            AddOptionalElement(xeVehicleDetails, "EngineDescription", vehicle.EngineDescription);

            AddOptionalElement(xeVehicleDetails, "ExteriorColor", vehicle.ExteriorColor);

            AddOptionalElement(xeVehicleDetails, "LicPlate", vehicle.LicPlate);

            AddOptionalElement(xeVehicleDetails, "LicPlateState", vehicle.LicPlateState);

            if (vehicle.LicPlateExpDate.HasValue)
                xeVehicleDetails.Add(new XElement(m_cla + "LicPlateExpDate", vehicle.LicPlateExpDate.Value.ToString("o")));

            AddOptionalElement(xeVehicleDetails, "DamageDescription", vehicle.DamageDescription);

            if (vehicle.Mileage.HasValue)
                xeVehicleDetails.Add(new XElement(m_cla + "Mileage", vehicle.Mileage.Value.ToString()));

            return xeVehicleDetails;
        }

        private void AddOptionalElement(XElement a_xeRoot, string a_sElementName, string a_sElementValue)
        {
            if (!String.IsNullOrEmpty(a_sElementValue))
                a_xeRoot.Add(new XElement(m_cla + a_sElementName, a_sElementValue));
        }

        private string CauseOfLossToString(CauseOfLossCode a_eCode)
        {
            // adding a space between words
            if (a_eCode == CauseOfLossCode.MechanicalBreakdown)
                return "Mechanical Breakdown";
            return a_eCode.ToString();
        }
    }
}
