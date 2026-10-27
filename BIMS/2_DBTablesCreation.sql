USE [BaranggayInformationSystem];
GO

-- BIMS: Barangay Information Management System
-- SQL Server LocalDB / .NET 10

CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password_hash NVARCHAR(100) NOT NULL,
    password_salt NVARCHAR(50) NOT NULL,
    full_name NVARCHAR(100) NOT NULL,
    role NVARCHAR(10) NOT NULL DEFAULT 'Staff',
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT ck_users_role
        CHECK (role IN ('Admin', 'Staff'))
);
GO

CREATE TABLE households (
    id INT IDENTITY(1,1) PRIMARY KEY,
    household_no NVARCHAR(20) NOT NULL UNIQUE,
    purok NVARCHAR(50) NOT NULL,
    street NVARCHAR(100) NULL,
    head_resident_id INT NULL,
    remarks NVARCHAR(255) NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);
GO

CREATE TABLE residents (
    id INT IDENTITY(1,1) PRIMARY KEY,
    first_name NVARCHAR(50) NOT NULL,
    middle_name NVARCHAR(50) NULL,
    last_name NVARCHAR(50) NOT NULL,
    suffix NVARCHAR(10) NULL,
    birthdate DATE NOT NULL,
    sex NVARCHAR(10) NOT NULL,
    civil_status NVARCHAR(15) NOT NULL DEFAULT 'Single',
    citizenship NVARCHAR(50) NOT NULL DEFAULT 'Filipino',
    occupation NVARCHAR(100) NULL,
    contact_no NVARCHAR(20) NULL,
    household_id INT NULL,
    relationship_to_head NVARCHAR(30) NULL,
    is_voter BIT NOT NULL DEFAULT 0,

    -- Resident registration approval
    approval_status NVARCHAR(15) NOT NULL DEFAULT 'Pending',
    approved_by INT NULL,
    approved_at DATETIME2(0) NULL,
    rejection_reason NVARCHAR(255) NULL,

    -- Status after registration approval
    status NVARCHAR(15) NOT NULL DEFAULT 'Active',
    created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    updated_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT ck_res_sex
        CHECK (sex IN ('Male', 'Female')),
    CONSTRAINT ck_res_civil
        CHECK (civil_status IN
            ('Single', 'Married', 'Widowed', 'Separated', 'Annulled')),
    CONSTRAINT ck_res_status
        CHECK (status IN ('Active', 'Deceased', 'Transferred')),
    CONSTRAINT ck_res_approval
        CHECK (approval_status IN ('Pending', 'Approved', 'Rejected')),
    CONSTRAINT fk_res_household
        FOREIGN KEY (household_id) REFERENCES households(id),
    CONSTRAINT fk_res_approver
        FOREIGN KEY (approved_by) REFERENCES users(id)
);
GO

ALTER TABLE households
ADD CONSTRAINT fk_hh_head
FOREIGN KEY (head_resident_id) REFERENCES residents(id);
GO

CREATE INDEX idx_resident_name
ON residents(last_name, first_name);
GO

CREATE INDEX idx_res_approval
ON residents(approval_status);
GO

CREATE TABLE certificates (
    id INT IDENTITY(1,1) PRIMARY KEY,
    control_no NVARCHAR(40) NOT NULL UNIQUE,
    resident_id INT NOT NULL,
    cert_type NVARCHAR(30) NOT NULL,
    purpose NVARCHAR(255) NOT NULL,
    or_number NVARCHAR(30) NULL,
    fee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    date_issued DATE NOT NULL,
    issued_by INT NOT NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT ck_cert_type
        CHECK (cert_type IN (
            'Barangay Clearance',
            'Certificate of Residency',
            'Certificate of Indigency',
            'Business Clearance'
        )),
    CONSTRAINT fk_cert_resident
        FOREIGN KEY (resident_id) REFERENCES residents(id),
    CONSTRAINT fk_cert_user
        FOREIGN KEY (issued_by) REFERENCES users(id)
);
GO

CREATE INDEX idx_cert_date
ON certificates(date_issued);
GO

-- Certificate requests are separate from issued certificates.
CREATE TABLE certificate_requests (
    id INT IDENTITY(1,1) PRIMARY KEY,
    resident_id INT NOT NULL,
    cert_type NVARCHAR(30) NOT NULL,
    purpose NVARCHAR(255) NOT NULL,
    request_date DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    request_status NVARCHAR(15) NOT NULL DEFAULT 'Pending',
    reviewed_by INT NULL,
    reviewed_at DATETIME2(0) NULL,
    rejection_reason NVARCHAR(255) NULL,
    certificate_id INT NULL,

    CONSTRAINT ck_cert_request_status
        CHECK (request_status IN
            ('Pending', 'Approved', 'Rejected', 'Issued')),
    CONSTRAINT fk_cert_request_resident
        FOREIGN KEY (resident_id) REFERENCES residents(id),
    CONSTRAINT fk_cert_request_user
        FOREIGN KEY (reviewed_by) REFERENCES users(id),
    CONSTRAINT fk_cert_request_certificate
        FOREIGN KEY (certificate_id) REFERENCES certificates(id)
);
GO

CREATE INDEX idx_cert_request_status
ON certificate_requests(request_status);
GO

CREATE TABLE case_records (
    id INT IDENTITY(1,1) PRIMARY KEY,
    case_no NVARCHAR(40) NOT NULL UNIQUE,
    record_type NVARCHAR(10) NOT NULL,
    complainant_resident_id INT NULL,
    complainant_name NVARCHAR(120) NOT NULL,
    respondent_resident_id INT NULL,
    respondent_name NVARCHAR(120) NOT NULL,
    incident_date DATE NOT NULL,
    incident_place NVARCHAR(150) NULL,
    nature NVARCHAR(100) NOT NULL,
    narrative NVARCHAR(MAX) NOT NULL,
    status NVARCHAR(15) NOT NULL DEFAULT 'Filed',
    resolution NVARCHAR(MAX) NULL,
    filed_by INT NOT NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    updated_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT ck_case_type
        CHECK (record_type IN ('Complaint', 'Blotter')),
    CONSTRAINT ck_case_status
        CHECK (status IN
            ('Filed', 'Mediation', 'Settled', 'Escalated', 'Dismissed')),
    CONSTRAINT fk_case_complainant
        FOREIGN KEY (complainant_resident_id) REFERENCES residents(id),
    CONSTRAINT fk_case_respondent
        FOREIGN KEY (respondent_resident_id) REFERENCES residents(id),
    CONSTRAINT fk_case_user
        FOREIGN KEY (filed_by) REFERENCES users(id)
);
GO

CREATE INDEX idx_case_date
ON case_records(incident_date);
GO

CREATE INDEX idx_case_status
ON case_records(status);
GO

-- Automatically update modification timestamps.
CREATE TRIGGER trg_residents_updated
ON residents
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE r
    SET updated_at = SYSDATETIME()
    FROM residents r
    INNER JOIN inserted i ON r.id = i.id;
END;
GO

CREATE TRIGGER trg_case_records_updated
ON case_records
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE c
    SET updated_at = SYSDATETIME()
    FROM case_records c
    INNER JOIN inserted i ON c.id = i.id;
END;
GO
