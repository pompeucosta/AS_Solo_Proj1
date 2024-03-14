CREATE PROCEDURE [dbo].[UpdateUserDetails]
	@requesterID int,
	@clientID int,
	@accessCode nvarchar(max),
	@fullName nvarchar(max),
	@phoneNumber nvarchar(max),
	@medicalRecordNumber int,
	@diagnosisDetails nvarchar(max),
	@treatmentPlan nvarchar(max)
AS
begin
	declare @isAllowed bit;
	exec IsAllowedToUnmask @requesterID,@clientID,@accessCode,@isAllowed OUTPUT;

	if(@isAllowed = 1)
	begin
		Update Clients
		set FullName = @fullName,PhoneNumber = @phoneNumber,MedicalRecordNumber = @medicalRecordNumber,DiagnosisDetails = @diagnosisDetails,TreatmentPlan = @treatmentPlan
		where ClientID = @clientID;
		return;
	end;
	else
	begin
		Update Clients
		set FullName = @fullName,MedicalRecordNumber = @medicalRecordNumber
		where ClientID = @clientID;
		return;
	end;
end;