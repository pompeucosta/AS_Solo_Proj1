CREATE PROCEDURE [dbo].[IsAllowedToUnmask]
    @requesterID INT,
    @clientID INT,
    @accessCode NVARCHAR(MAX) = NULL,
	@isAllowed BIT OUTPUT
AS
BEGIN
    DECLARE @hashedCode VARBINARY(64);
	DECLARE @clientUserID INT;

	SELECT @clientUserID = u.UserID
	FROM [dbo].[Users] u
	JOIN [dbo].[Clients] c ON u.UserID = c.UserID
	WHERE c.ClientID = @clientID;

	IF (@clientUserID IS NULL)
	BEGIN
		SET @isAllowed = 0;
		RETURN;
	END;

    IF (@requesterID = @clientUserID)
    BEGIN
        SET @isAllowed = 1;
		RETURN;
    END;
    ELSE
    BEGIN
        IF (@accessCode IS NULL)
        BEGIN
            SET @isAllowed = 0;
			RETURN;
        END;
        ELSE
        BEGIN
            SET @hashedCode = HASHBYTES('SHA2_256', @accessCode);
            DECLARE @clientCode NVARCHAR(MAX);

            SELECT @clientCode = AccessCode
            FROM Clients
            WHERE Clients.ClientID = @clientID;

            IF (@clientCode = @hashedCode)
            BEGIN
                SET @isAllowed = 1;
				RETURN;
            END;
            ELSE
            BEGIN
                SET @isAllowed = 0;
				RETURN;
            END;
        END;
    END;
END;