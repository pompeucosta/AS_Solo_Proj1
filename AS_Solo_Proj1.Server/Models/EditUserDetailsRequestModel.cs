﻿namespace AS_Solo_Proj1.Server.Models
{
    public class EditUserDetailsRequestModel
    {
        public int ClientID { get; set; }
        public string? AccessCode { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string DiagnosisDetails { get; set; }
        public string TreatmentPlan { get; set; }
        public int MedicalRecordNumber { get; set; }
    }
}
