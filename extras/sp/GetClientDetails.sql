CREATE PROCEDURE [dbo].[GetClientDetails]
    @requesterID INT,
    @clientID INT,
    @accessCode NVARCHAR(MAX) = NULL
AS
BEGIN
    DECLARE @isAllowed BIT;
	SET @isAllowed = -1;
	EXEC IsAllowedToUnmask @requesterID,@clientID,@accessCode,@isAllowed OUTPUT;

	IF(@isAllowed = 1)
		EXECUTE AS USER = 'Client';
	ELSE
		EXECUTE AS USER = 'HelpDesk';

    SELECT TOP 1
		[ClientID],
        [FullName],
        [PhoneNumber],
        [MedicalRecordNumber],
        [DiagnosisDetails],
        [TreatmentPlan]
    FROM Clients
    WHERE Clients.ClientID = @clientID;

    REVERT;
END;