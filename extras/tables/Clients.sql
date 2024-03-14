-- autogerada pela EntityFramework do ASP e depois alterada para acrescentar o masking
CREATE TABLE [dbo].[Clients](
	[ClientID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[FullName] [nvarchar](max) NOT NULL,
	[PhoneNumber] [nvarchar](max) MASKED WITH (FUNCTION = 'partial(0, "xxxx", 3)') NOT NULL,
	[MedicalRecordNumber] [int] NOT NULL,
	[DiagnosisDetails] [nvarchar](max) MASKED WITH (FUNCTION = 'default()') NOT NULL,
	[TreatmentPlan] [nvarchar](max) MASKED WITH (FUNCTION = 'default()') NOT NULL,
	[AccessCode] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED 
(
	[ClientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Clients]  WITH CHECK ADD  CONSTRAINT [FK_Clients_Users_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Clients] CHECK CONSTRAINT [FK_Clients_Users_UserID]
GO