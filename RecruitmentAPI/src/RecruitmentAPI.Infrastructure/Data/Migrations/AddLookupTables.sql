BEGIN TRANSACTION;
CREATE TABLE [Countries] (
    [CountryId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Iso3Code] nvarchar(3) NOT NULL,
    [Iso2Code] nvarchar(2) NULL,
    [PhoneCode] nvarchar(10) NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY ([CountryId])
);

CREATE TABLE [Currencies] (
    [CurrencyId] int NOT NULL IDENTITY,
    [Code] nvarchar(3) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Symbol] nvarchar(5) NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Currencies] PRIMARY KEY ([CurrencyId])
);

CREATE TABLE [MajorFieldsOfStudy] (
    [MajorFieldOfStudyId] int NOT NULL IDENTITY,
    [Name] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_MajorFieldsOfStudy] PRIMARY KEY ([MajorFieldOfStudyId])
);

CREATE TABLE [QualificationTypes] (
    [QualificationTypeId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_QualificationTypes] PRIMARY KEY ([QualificationTypeId])
);

CREATE TABLE [Institutions] (
    [InstitutionId] int NOT NULL IDENTITY,
    [Name] nvarchar(400) NOT NULL,
    [CountryId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Institutions] PRIMARY KEY ([InstitutionId]),
    CONSTRAINT [FK_Institutions_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId]) ON DELETE CASCADE
);

CREATE TABLE [Nationalities] (
    [NationalityId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [CountryId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Nationalities] PRIMARY KEY ([NationalityId]),
    CONSTRAINT [FK_Nationalities_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId]) ON DELETE CASCADE
);

CREATE TABLE [Regions] (
    [RegionId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [CountryId] int NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Regions] PRIMARY KEY ([RegionId]),
    CONSTRAINT [FK_Regions_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId])
);

CREATE TABLE [Certificates] (
    [CertificateId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [QualificationTypeId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Certificates] PRIMARY KEY ([CertificateId]),
    CONSTRAINT [FK_Certificates_QualificationTypes_QualificationTypeId] FOREIGN KEY ([QualificationTypeId]) REFERENCES [QualificationTypes] ([QualificationTypeId]) ON DELETE CASCADE
);

CREATE TABLE [Degrees] (
    [DegreeId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [QualificationTypeId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Degrees] PRIMARY KEY ([DegreeId]),
    CONSTRAINT [FK_Degrees_QualificationTypes_QualificationTypeId] FOREIGN KEY ([QualificationTypeId]) REFERENCES [QualificationTypes] ([QualificationTypeId]) ON DELETE CASCADE
);

CREATE TABLE [Cities] (
    [CityId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [CountryId] int NOT NULL,
    [RegionId] int NULL,
    [IsActive] bit NOT NULL,
    [IsCustom] bit NOT NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY ([CityId]),
    CONSTRAINT [FK_Cities_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Cities_Regions_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [Regions] ([RegionId])
);

CREATE TABLE [CandidateQualifications] (
    [CandidateQualificationId] bigint NOT NULL IDENTITY,
    [CandidateId] bigint NOT NULL,
    [CandidateEducationId] bigint NULL,
    [QualificationTypeId] int NOT NULL,
    [DegreeId] int NULL,
    [CertificateId] int NULL,
    [MajorFieldOfStudyId] int NULL,
    [InstitutionId] int NULL,
    [CountryId] int NULL,
    [GraduationYear] int NULL,
    [GradeOrGPA] nvarchar(50) NULL,
    [DataSource] tinyint NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [ModifiedDate] datetime2 NULL,
    [ModifiedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_CandidateQualifications] PRIMARY KEY ([CandidateQualificationId]),
    CONSTRAINT [FK_CandidateQualifications_CandidateEducations_CandidateEducationId] FOREIGN KEY ([CandidateEducationId]) REFERENCES [CandidateEducations] ([CandidateEducationId]),
    CONSTRAINT [FK_CandidateQualifications_Candidates_CandidateId] FOREIGN KEY ([CandidateId]) REFERENCES [Candidates] ([CandidateId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CandidateQualifications_Certificates_CertificateId] FOREIGN KEY ([CertificateId]) REFERENCES [Certificates] ([CertificateId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CandidateQualifications_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CandidateQualifications_Degrees_DegreeId] FOREIGN KEY ([DegreeId]) REFERENCES [Degrees] ([DegreeId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CandidateQualifications_Institutions_InstitutionId] FOREIGN KEY ([InstitutionId]) REFERENCES [Institutions] ([InstitutionId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CandidateQualifications_MajorFieldsOfStudy_MajorFieldOfStudyId] FOREIGN KEY ([MajorFieldOfStudyId]) REFERENCES [MajorFieldsOfStudy] ([MajorFieldOfStudyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CandidateQualifications_QualificationTypes_QualificationTypeId] FOREIGN KEY ([QualificationTypeId]) REFERENCES [QualificationTypes] ([QualificationTypeId]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryId', N'IsActive', N'IsCustom', N'Iso2Code', N'Iso3Code', N'Name', N'PhoneCode') AND [object_id] = OBJECT_ID(N'[Countries]'))
    SET IDENTITY_INSERT [Countries] ON;
INSERT INTO [Countries] ([CountryId], [IsActive], [IsCustom], [Iso2Code], [Iso3Code], [Name], [PhoneCode])
VALUES (1, CAST(1 AS bit), CAST(1 AS bit), N'OT', N'OTH', N'Other', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryId', N'IsActive', N'IsCustom', N'Iso2Code', N'Iso3Code', N'Name', N'PhoneCode') AND [object_id] = OBJECT_ID(N'[Countries]'))
    SET IDENTITY_INSERT [Countries] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CurrencyId', N'Code', N'IsActive', N'IsCustom', N'Name', N'Symbol') AND [object_id] = OBJECT_ID(N'[Currencies]'))
    SET IDENTITY_INSERT [Currencies] ON;
INSERT INTO [Currencies] ([CurrencyId], [Code], [IsActive], [IsCustom], [Name], [Symbol])
VALUES (1, N'SAR', CAST(1 AS bit), CAST(0 AS bit), N'Saudi Riyal', N'﷼'),
(2, N'USD', CAST(1 AS bit), CAST(0 AS bit), N'US Dollar', N'$'),
(3, N'EUR', CAST(1 AS bit), CAST(0 AS bit), N'Euro', N'€'),
(4, N'GBP', CAST(1 AS bit), CAST(0 AS bit), N'British Pound', N'£'),
(5, N'AED', CAST(1 AS bit), CAST(0 AS bit), N'UAE Dirham', N'د.إ'),
(6, N'KWD', CAST(1 AS bit), CAST(0 AS bit), N'Kuwaiti Dinar', N'د.ك'),
(7, N'QAR', CAST(1 AS bit), CAST(0 AS bit), N'Qatari Riyal', N'ر.ق'),
(8, N'BHD', CAST(1 AS bit), CAST(0 AS bit), N'Bahraini Dinar', N'د.ب'),
(9, N'OMR', CAST(1 AS bit), CAST(0 AS bit), N'Omani Rial', N'ر.ع'),
(10, N'EGP', CAST(1 AS bit), CAST(0 AS bit), N'Egyptian Pound', N'£'),
(11, N'INR', CAST(1 AS bit), CAST(0 AS bit), N'Indian Rupee', N'₹'),
(12, N'PKR', CAST(1 AS bit), CAST(0 AS bit), N'Pakistani Rupee', N'₨'),
(13, N'JPY', CAST(1 AS bit), CAST(0 AS bit), N'Japanese Yen', N'¥'),
(14, N'CNY', CAST(1 AS bit), CAST(0 AS bit), N'Chinese Yuan', N'¥'),
(15, N'CAD', CAST(1 AS bit), CAST(0 AS bit), N'Canadian Dollar', N'$'),
(16, N'AUD', CAST(1 AS bit), CAST(0 AS bit), N'Australian Dollar', N'$'),
(17, N'OTH', CAST(1 AS bit), CAST(1 AS bit), N'Other', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CurrencyId', N'Code', N'IsActive', N'IsCustom', N'Name', N'Symbol') AND [object_id] = OBJECT_ID(N'[Currencies]'))
    SET IDENTITY_INSERT [Currencies] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MajorFieldOfStudyId', N'IsActive', N'IsCustom', N'Name') AND [object_id] = OBJECT_ID(N'[MajorFieldsOfStudy]'))
    SET IDENTITY_INSERT [MajorFieldsOfStudy] ON;
INSERT INTO [MajorFieldsOfStudy] ([MajorFieldOfStudyId], [IsActive], [IsCustom], [Name])
VALUES (1, CAST(1 AS bit), CAST(1 AS bit), N'Other');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MajorFieldOfStudyId', N'IsActive', N'IsCustom', N'Name') AND [object_id] = OBJECT_ID(N'[MajorFieldsOfStudy]'))
    SET IDENTITY_INSERT [MajorFieldsOfStudy] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'QualificationTypeId', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[QualificationTypes]'))
    SET IDENTITY_INSERT [QualificationTypes] ON;
INSERT INTO [QualificationTypes] ([QualificationTypeId], [IsActive], [Name])
VALUES (1, CAST(1 AS bit), N'Degree'),
(2, CAST(1 AS bit), N'Certificate');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'QualificationTypeId', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[QualificationTypes]'))
    SET IDENTITY_INSERT [QualificationTypes] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CertificateId', N'IsActive', N'IsCustom', N'Name', N'QualificationTypeId') AND [object_id] = OBJECT_ID(N'[Certificates]'))
    SET IDENTITY_INSERT [Certificates] ON;
INSERT INTO [Certificates] ([CertificateId], [IsActive], [IsCustom], [Name], [QualificationTypeId])
VALUES (1, CAST(1 AS bit), CAST(1 AS bit), N'Other', 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CertificateId', N'IsActive', N'IsCustom', N'Name', N'QualificationTypeId') AND [object_id] = OBJECT_ID(N'[Certificates]'))
    SET IDENTITY_INSERT [Certificates] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DegreeId', N'IsActive', N'IsCustom', N'Name', N'QualificationTypeId') AND [object_id] = OBJECT_ID(N'[Degrees]'))
    SET IDENTITY_INSERT [Degrees] ON;
INSERT INTO [Degrees] ([DegreeId], [IsActive], [IsCustom], [Name], [QualificationTypeId])
VALUES (1, CAST(1 AS bit), CAST(1 AS bit), N'Other', 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DegreeId', N'IsActive', N'IsCustom', N'Name', N'QualificationTypeId') AND [object_id] = OBJECT_ID(N'[Degrees]'))
    SET IDENTITY_INSERT [Degrees] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'InstitutionId', N'CountryId', N'IsActive', N'IsCustom', N'Name') AND [object_id] = OBJECT_ID(N'[Institutions]'))
    SET IDENTITY_INSERT [Institutions] ON;
INSERT INTO [Institutions] ([InstitutionId], [CountryId], [IsActive], [IsCustom], [Name])
VALUES (1, 1, CAST(1 AS bit), CAST(1 AS bit), N'Other');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'InstitutionId', N'CountryId', N'IsActive', N'IsCustom', N'Name') AND [object_id] = OBJECT_ID(N'[Institutions]'))
    SET IDENTITY_INSERT [Institutions] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'NationalityId', N'CountryId', N'IsActive', N'IsCustom', N'Name') AND [object_id] = OBJECT_ID(N'[Nationalities]'))
    SET IDENTITY_INSERT [Nationalities] ON;
INSERT INTO [Nationalities] ([NationalityId], [CountryId], [IsActive], [IsCustom], [Name])
VALUES (1, 1, CAST(1 AS bit), CAST(1 AS bit), N'Other');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'NationalityId', N'CountryId', N'IsActive', N'IsCustom', N'Name') AND [object_id] = OBJECT_ID(N'[Nationalities]'))
    SET IDENTITY_INSERT [Nationalities] OFF;

CREATE INDEX [IX_CandidateQualifications_CandidateEducationId] ON [CandidateQualifications] ([CandidateEducationId]);

CREATE INDEX [IX_CandidateQualifications_CandidateId] ON [CandidateQualifications] ([CandidateId]);

CREATE INDEX [IX_CandidateQualifications_CertificateId] ON [CandidateQualifications] ([CertificateId]);

CREATE INDEX [IX_CandidateQualifications_CountryId] ON [CandidateQualifications] ([CountryId]);

CREATE INDEX [IX_CandidateQualifications_DegreeId] ON [CandidateQualifications] ([DegreeId]);

CREATE INDEX [IX_CandidateQualifications_InstitutionId] ON [CandidateQualifications] ([InstitutionId]);

CREATE INDEX [IX_CandidateQualifications_MajorFieldOfStudyId] ON [CandidateQualifications] ([MajorFieldOfStudyId]);

CREATE INDEX [IX_CandidateQualifications_QualificationTypeId] ON [CandidateQualifications] ([QualificationTypeId]);

CREATE UNIQUE INDEX [IX_Certificates_Name_QualificationTypeId] ON [Certificates] ([Name], [QualificationTypeId]);

CREATE INDEX [IX_Certificates_QualificationTypeId] ON [Certificates] ([QualificationTypeId]);

CREATE INDEX [IX_Cities_CountryId] ON [Cities] ([CountryId]);

CREATE UNIQUE INDEX [IX_Cities_Name_CountryId_RegionId] ON [Cities] ([Name], [CountryId], [RegionId]) WHERE [RegionId] IS NOT NULL;

CREATE INDEX [IX_Cities_RegionId] ON [Cities] ([RegionId]);

CREATE UNIQUE INDEX [IX_Countries_Iso2Code] ON [Countries] ([Iso2Code]) WHERE [Iso2Code] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Countries_Iso3Code] ON [Countries] ([Iso3Code]);

CREATE UNIQUE INDEX [IX_Countries_Name] ON [Countries] ([Name]);

CREATE UNIQUE INDEX [IX_Currencies_Code] ON [Currencies] ([Code]);

CREATE UNIQUE INDEX [IX_Currencies_Name] ON [Currencies] ([Name]);

CREATE UNIQUE INDEX [IX_Degrees_Name_QualificationTypeId] ON [Degrees] ([Name], [QualificationTypeId]);

CREATE INDEX [IX_Degrees_QualificationTypeId] ON [Degrees] ([QualificationTypeId]);

CREATE INDEX [IX_Institutions_CountryId] ON [Institutions] ([CountryId]);

CREATE UNIQUE INDEX [IX_Institutions_Name_CountryId] ON [Institutions] ([Name], [CountryId]);

CREATE UNIQUE INDEX [IX_MajorFieldsOfStudy_Name] ON [MajorFieldsOfStudy] ([Name]);

CREATE INDEX [IX_Nationalities_CountryId] ON [Nationalities] ([CountryId]);

CREATE UNIQUE INDEX [IX_Nationalities_Name] ON [Nationalities] ([Name]);

CREATE UNIQUE INDEX [IX_QualificationTypes_Name] ON [QualificationTypes] ([Name]);

CREATE INDEX [IX_Regions_CountryId] ON [Regions] ([CountryId]);

CREATE UNIQUE INDEX [IX_Regions_Name_CountryId] ON [Regions] ([Name], [CountryId]) WHERE [CountryId] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260406182630_AddLookupTablesAndCandidateQualifications', N'10.0.3');

COMMIT;
GO

