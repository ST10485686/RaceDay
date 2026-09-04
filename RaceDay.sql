-- FIX: Auto-delete if RaceDay already exists from your last run
IF DB_ID('RaceDay') IS NOT NULL
BEGIN
USE master;
ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE RaceDay;
END
GO

CREATE DATABASE RaceDay;
GO
USE RaceDay;
GO

-- ============================================================
-- SECTION 1: DROP EXISTING TABLES 
-- ============================================================
IF OBJECT_ID('dbo.EventSponsor', 'U') IS NOT NULL DROP TABLE dbo.EventSponsor;
IF OBJECT_ID('dbo.Payment', 'U') IS NOT NULL DROP TABLE dbo.Payment;
IF OBJECT_ID('dbo.Result', 'U') IS NOT NULL DROP TABLE dbo.Result;
IF OBJECT_ID('dbo.Registration', 'U') IS NOT NULL DROP TABLE dbo.Registration;
IF OBJECT_ID('dbo.Race', 'U') IS NOT NULL DROP TABLE dbo.Race;
IF OBJECT_ID('dbo.Event', 'U') IS NOT NULL DROP TABLE dbo.Event;
IF OBJECT_ID('dbo.Sponsor', 'U') IS NOT NULL DROP TABLE dbo.Sponsor;
IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL DROP TABLE dbo.[User];
GO

-- ============================================================
-- SECTION 2: CREATE TABLES
-- ============================================================
CREATE TABLE dbo.[User] (
user_id INT IDENTITY(1,1) PRIMARY KEY,
email VARCHAR(255) NOT NULL UNIQUE,
password_hash VARCHAR(255) NOT NULL,
full_name VARCHAR(100) NOT NULL,
date_of_birth DATE NULL,
gender CHAR(1) NULL CHECK (gender IN ('M', 'F', 'O')),
phone VARCHAR(20) NULL,
emergency_contact VARCHAR(100) NULL,
created_at DATETIME DEFAULT GETDATE() NOT NULL
);
GO

CREATE TABLE dbo.Event (
event_id INT IDENTITY(1,1) PRIMARY KEY,
organizer_id INT NOT NULL,
event_name VARCHAR(100) NOT NULL,
event_date DATE NOT NULL,
location VARCHAR(200) NULL,
city VARCHAR(50) NULL,
country VARCHAR(50) NULL,
description NVARCHAR(MAX) NULL,
created_at DATETIME DEFAULT GETDATE() NOT NULL,
CONSTRAINT FK_Event_Organizer FOREIGN KEY (organizer_id) REFERENCES dbo.[User](user_id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Race (
race_id INT IDENTITY(1,1) PRIMARY KEY,
event_id INT NOT NULL,
race_name VARCHAR(100) NOT NULL,
distance_km DECIMAL(5,2) NOT NULL CHECK (distance_km > 0),
start_time DATETIME NOT NULL,
end_time DATETIME NULL,
age_group VARCHAR(50) NULL,
max_participants INT NULL CHECK (max_participants > 0),
created_at DATETIME DEFAULT GETDATE() NOT NULL,
CONSTRAINT FK_Race_Event FOREIGN KEY (event_id) REFERENCES dbo.Event(event_id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Sponsor (
sponsor_id INT IDENTITY(1,1) PRIMARY KEY,
sponsor_name VARCHAR(100) NOT NULL,
website VARCHAR(200) NULL,
contact_email VARCHAR(100) NULL,
phone VARCHAR(20) NULL,
created_at DATETIME DEFAULT GETDATE() NOT NULL
);
GO

CREATE TABLE dbo.EventSponsor (
event_id INT NOT NULL,
sponsor_id INT NOT NULL,
sponsorship_level VARCHAR(50) NULL,
created_at DATETIME DEFAULT GETDATE() NOT NULL,
CONSTRAINT PK_EventSponsor PRIMARY KEY (event_id, sponsor_id),
CONSTRAINT FK_EventSponsor_Event FOREIGN KEY (event_id) REFERENCES dbo.Event(event_id) ON DELETE CASCADE,
CONSTRAINT FK_EventSponsor_Sponsor FOREIGN KEY (sponsor_id) REFERENCES dbo.Sponsor(sponsor_id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Registration (
registration_id INT IDENTITY(1,1) PRIMARY KEY,
user_id INT NOT NULL,
race_id INT NOT NULL,
registration_date DATETIME DEFAULT GETDATE() NOT NULL,
status VARCHAR(20) DEFAULT 'pending' NOT NULL CHECK (status IN ('pending', 'confirmed', 'cancelled')),
bib_number VARCHAR(20) UNIQUE NULL,
created_at DATETIME DEFAULT GETDATE() NOT NULL,
CONSTRAINT FK_Registration_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id) ON DELETE CASCADE,
CONSTRAINT FK_Registration_Race FOREIGN KEY (race_id) REFERENCES dbo.Race(race_id) ON DELETE CASCADE,
CONSTRAINT UQ_Registration_UserRace UNIQUE (user_id, race_id)
);
GO

CREATE TABLE dbo.Result (
result_id INT IDENTITY(1,1) PRIMARY KEY,
registration_id INT NOT NULL,
finish_time TIME NULL,
overall_rank INT NULL,
age_group_rank INT NULL,
pace_per_km DECIMAL(4,2) NULL,
status VARCHAR(20) NOT NULL DEFAULT 'DNS' CHECK (status IN ('DNS', 'DNF', 'finished')),
created_at DATETIME DEFAULT GETDATE() NOT NULL,
CONSTRAINT FK_Result_Registration FOREIGN KEY (registration_id) REFERENCES dbo.Registration(registration_id) ON DELETE CASCADE,
CONSTRAINT UQ_Result_Registration UNIQUE (registration_id)
);
GO

CREATE TABLE dbo.Payment (
payment_id INT IDENTITY(1,1) PRIMARY KEY,
registration_id INT NOT NULL,
amount DECIMAL(10,2) NOT NULL CHECK (amount >= 0),
payment_date DATETIME DEFAULT GETDATE() NOT NULL,
payment_method VARCHAR(20) NULL CHECK (payment_method IN ('credit_card', 'paypal', 'bank_transfer')),
transaction_id VARCHAR(100) UNIQUE NULL,
created_at DATETIME DEFAULT GETDATE() NOT NULL,
CONSTRAINT FK_Payment_Registration FOREIGN KEY (registration_id) REFERENCES dbo.Registration(registration_id) ON DELETE CASCADE,
CONSTRAINT UQ_Payment_Registration UNIQUE (registration_id)
);
GO

-- ============================================================
-- SECTION 3: INDEXES
-- ============================================================
CREATE INDEX IX_User_Email ON dbo.[User](email);
CREATE INDEX IX_User_FullName ON dbo.[User](full_name);
CREATE INDEX IX_Event_Organizer ON dbo.Event(organizer_id);
CREATE INDEX IX_Event_Date ON dbo.Event(event_date);
CREATE INDEX IX_Race_Event ON dbo.Race(event_id);
CREATE INDEX IX_Race_StartTime ON dbo.Race(start_time);
CREATE INDEX IX_Registration_User ON dbo.Registration(user_id);
CREATE INDEX IX_Registration_Race ON dbo.Registration(race_id);
CREATE INDEX IX_Registration_Status ON dbo.Registration(status);
CREATE INDEX IX_Result_Registration ON dbo.Result(registration_id);
CREATE INDEX IX_Result_OverallRank ON dbo.Result(overall_rank);
CREATE INDEX IX_Payment_Registration ON dbo.Payment(registration_id);
CREATE INDEX IX_Payment_Transaction ON dbo.Payment(transaction_id);
CREATE INDEX IX_EventSponsor_Sponsor ON dbo.EventSponsor(sponsor_id);
GO

-- ============================================================
-- SECTION 4: INSERT SAMPLE DATA - NEW EMAILS
-- ============================================================
INSERT INTO dbo.[User] (email, password_hash, full_name, date_of_birth, gender, phone, emergency_contact)
VALUES
('sarah.morgan.org@raceday.co.za', 'hash_123456', 'Sarah Johnson', '1985-03-15', 'F', '+1-555-0101', 'Mike Johnson - +1-555-0102'),
('m.chen.events@raceday.co.za', 'hash_789012', 'Michael Chen', '1978-11-22', 'M', '+1-555-0201', 'Lisa Chen - +1-555-0202');
GO

INSERT INTO dbo.[User] (email, password_hash, full_name, date_of_birth, gender, phone, emergency_contact)
VALUES
('emma.wilson.runner@outlook.com', 'hash_345678', 'Emma Wilson', '1992-07-08', 'F', '+1-555-0301', 'David Wilson - +1-555-0302'),
('j.rodriguez.run@gmail.com', 'hash_901234', 'James Rodriguez', '1988-09-23', 'M', '+1-555-0401', 'Anna Rodriguez - +1-555-0402');
GO

INSERT INTO dbo.Event (organizer_id, event_name, event_date, location, city, country, description)
VALUES
(1, 'City Marathon 2026', '2026-11-15', 'Downtown Convention Center', 'New York', 'USA', 'Annual city marathon featuring full and half marathon distances'),
(1, 'Spring 5K Fun Run', '2026-05-10', 'Central Park', 'Boston', 'USA', 'Family-friendly 5K run with proceeds going to local charities'),
(2, 'Trail Challenge Series', '2026-08-22', 'Mountain View Park', 'Denver', 'USA', 'Challenging trail runs through the Rocky Mountain foothills');
GO

INSERT INTO dbo.Sponsor (sponsor_name, website, contact_email, phone)
VALUES
('Nike Running', 'www.nike.com/running', 'sponsors@nike.com', '+1-800-555-1000'),
('Gatorade', 'www.gatorade.com', 'events@gatorade.com', '+1-800-555-2000'),
('Apple Sports', 'www.apple.com/watch', 'sports@apple.com', '+1-800-555-3000'),
('Brooks Running', 'www.brooksrunning.com', 'sponsorship@brooks.com', '+1-800-555-4000');
GO

INSERT INTO dbo.EventSponsor (event_id, sponsor_id, sponsorship_level) VALUES
(1, 1, 'Platinum'), (1, 2, 'Gold'), (2, 1, 'Silver'), (2, 3, 'Gold'), (3, 4, 'Platinum'), (3, 2, 'Silver');
GO

INSERT INTO dbo.Race (event_id, race_name, distance_km, start_time, end_time, age_group, max_participants) VALUES
(1, 'Full Marathon', 42.20, '2026-11-15 07:00:00', '2026-11-15 13:00:00', NULL, 2000),
(1, 'Half Marathon', 21.10, '2026-11-15 08:30:00', '2026-11-15 12:00:00', NULL, 3000),
(1, '10K Run', 10.00, '2026-11-15 09:30:00', '2026-11-15 11:30:00', '16+', 1500);
GO

INSERT INTO dbo.Race (event_id, race_name, distance_km, start_time, end_time, age_group, max_participants) VALUES
(2, '5K Competitive', 5.00, '2026-05-10 09:00:00', '2026-05-10 11:00:00', '18+', 500),
(2, '5K Family Walk', 5.00, '2026-05-10 10:30:00', '2026-05-10 12:30:00', 'All Ages', 300),
(2, 'Kids Fun Run', 1.00, '2026-05-10 11:00:00', '2026-05-10 12:00:00', '5-12', 100);
GO

INSERT INTO dbo.Race (event_id, race_name, distance_km, start_time, end_time, age_group, max_participants) VALUES
(3, '10K Trail Run', 10.00, '2026-08-22 07:30:00', '2026-08-22 11:00:00', '18+', 400),
(3, '20K Trail Run', 20.00, '2026-08-22 06:00:00', '2026-08-22 11:30:00', '21+', 200);
GO

INSERT INTO dbo.Registration (user_id, race_id, registration_date, status, bib_number) VALUES
(3, 1, '2026-09-15 10:30:00', 'confirmed', 'M1001'),
(3, 3, '2026-09-20 14:15:00', 'confirmed', 'M3002'),
(3, 5, '2026-10-01 09:00:00', 'pending', NULL),
(4, 2, '2026-09-18 11:45:00', 'confirmed', 'H2003'),
(4, 4, '2026-09-25 16:20:00', 'confirmed', 'F4004'),
(4, 7, '2026-10-05 08:30:00', 'confirmed', 'T7005'),
(3, 6, '2026-09-28 13:10:00', 'cancelled', NULL),
(4, 8, '2026-10-02 10:00:00', 'confirmed', 'T8006');
GO

-- FIXED: registration_id 9 changed to 2 (was invalid)
INSERT INTO dbo.Result (registration_id, finish_time, overall_rank, age_group_rank, pace_per_km, status) VALUES
(1, '03:45:22', 245, 28, 5.30, 'finished'),
(4, '02:08:45', 78, 12, 6.10, 'finished'),
(3, '00:48:30', 134, 18, 4.85, 'finished'),
(5, '00:22:15', 45, 8, 4.45, 'finished'),
(8, '00:19:50', 12, 3, 3.98, 'finished'),
(6, '01:15:30', 67, 15, 7.55, 'finished'),
(2, '02:45:20', 89, 22, 8.27, 'finished');
GO

INSERT INTO dbo.Payment (registration_id, amount, payment_date, payment_method, transaction_id) VALUES
(1, 75.00, '2026-09-15 10:35:00', 'credit_card', 'TXN-1001-ABC'),
(2, 45.00, '2026-09-20 14:20:00', 'paypal', 'TXN-3002-DEF'),
(3, 25.00, '2026-10-01 09:05:00', 'bank_transfer', 'TXN-5003-GHI'),
(4, 75.00, '2026-09-18 11:50:00', 'credit_card', 'TXN-2004-JKL'),
(5, 25.00, '2026-09-25 16:25:00', 'paypal', 'TXN-4005-MNO'),
(6, 50.00, '2026-10-05 08:35:00', 'credit_card', 'TXN-7006-PQR'),
(8, 25.00, '2026-10-02 10:05:00', 'bank_transfer', 'TXN-8007-STU');
GO

-- ============================================================
-- SECTION 8: FINAL VERIFICATION - 
-- ============================================================
SELECT DB_NAME() AS CurrentDatabase;
GO

SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

SELECT * FROM (
SELECT 'User' AS TableName, COUNT(*) AS TotalRows FROM dbo.[User] UNION ALL
SELECT 'Event', COUNT(*) FROM dbo.Event UNION ALL
SELECT 'Race', COUNT(*) FROM dbo.Race UNION ALL
SELECT 'Sponsor', COUNT(*) FROM dbo.Sponsor UNION ALL
SELECT 'EventSponsor', COUNT(*) FROM dbo.EventSponsor UNION ALL
SELECT 'Registration', COUNT(*) FROM dbo.Registration UNION ALL
SELECT 'Result', COUNT(*) FROM dbo.Result UNION ALL
SELECT 'Payment', COUNT(*) FROM dbo.Payment
) AS AllCounts
ORDER BY TableName;
GO