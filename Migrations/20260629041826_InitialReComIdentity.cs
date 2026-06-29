using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZunoksBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialReComIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END

IF OBJECT_ID(N'[ReComLeads]', N'U') IS NULL
BEGIN
    CREATE TABLE [ReComLeads] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Company] nvarchar(500) NOT NULL,
        [CompanySize] nvarchar(50) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(200) NULL,
        [Details] nvarchar(4000) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ReComLeads] PRIMARY KEY ([Id])
    );
END

IF OBJECT_ID(N'[ZunoksSubmissions]', N'U') IS NULL
BEGIN
    CREATE TABLE [ZunoksSubmissions] (
        [Id] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(500) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZunoksSubmissions] PRIMARY KEY ([Id])
    );
END

IF OBJECT_ID(N'[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[ReComLeadServices]', N'U') IS NULL
BEGIN
    CREATE TABLE [ReComLeadServices] (
        [Id] int NOT NULL IDENTITY,
        [ReComLeadId] int NOT NULL,
        [ServiceName] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_ReComLeadServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReComLeadServices_ReComLeads_ReComLeadId] FOREIGN KEY ([ReComLeadId]) REFERENCES [ReComLeads] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[SelectedModules]', N'U') IS NULL
BEGIN
    CREATE TABLE [SelectedModules] (
        [Id] int NOT NULL IDENTITY,
        [ZunoksSubmissionId] int NOT NULL,
        [ModuleName] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_SelectedModules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SelectedModules_ZunoksSubmissions_ZunoksSubmissionId] FOREIGN KEY ([ZunoksSubmissionId]) REFERENCES [ZunoksSubmissions] ([Id]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[ZunoksResponses]', N'U') IS NULL
BEGIN
    CREATE TABLE [ZunoksResponses] (
        [Id] int NOT NULL IDENTITY,
        [ZunoksSubmissionId] int NOT NULL,
        [Module] nvarchar(500) NOT NULL,
        [QuestionId] nvarchar(100) NOT NULL,
        [QuestionLabel] nvarchar(2000) NULL,
        [Answer] nvarchar(4000) NOT NULL,
        CONSTRAINT [PK_ZunoksResponses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ZunoksResponses_ZunoksSubmissions_ZunoksSubmissionId] FOREIGN KEY ([ZunoksSubmissionId]) REFERENCES [ZunoksSubmissions] ([Id]) ON DELETE CASCADE
    );
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID(N'[AspNetRoleClaims]'))
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'RoleNameIndex' AND object_id = OBJECT_ID(N'[AspNetRoles]'))
    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID(N'[AspNetUserClaims]'))
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID(N'[AspNetUserLogins]'))
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID(N'[AspNetUserRoles]'))
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'EmailIndex' AND object_id = OBJECT_ID(N'[AspNetUsers]'))
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UserNameIndex' AND object_id = OBJECT_ID(N'[AspNetUsers]'))
    CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReComLeadServices_ReComLeadId' AND object_id = OBJECT_ID(N'[ReComLeadServices]'))
    CREATE INDEX [IX_ReComLeadServices_ReComLeadId] ON [ReComLeadServices] ([ReComLeadId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SelectedModules_ZunoksSubmissionId' AND object_id = OBJECT_ID(N'[SelectedModules]'))
    CREATE INDEX [IX_SelectedModules_ZunoksSubmissionId] ON [SelectedModules] ([ZunoksSubmissionId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ZunoksResponses_ZunoksSubmissionId' AND object_id = OBJECT_ID(N'[ZunoksResponses]'))
    CREATE INDEX [IX_ZunoksResponses_ZunoksSubmissionId] ON [ZunoksResponses] ([ZunoksSubmissionId]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "ReComLeadServices");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "AspNetUsers");
            migrationBuilder.DropTable(name: "ReComLeads");
        }
    }
}
