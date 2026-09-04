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

-- Clean drop order (children first)
IF OBJECT_ID('dbo.EventSponsor', 'U') IS NOT NULL DROP TABLE dbo.EventSponsor;
IF OBJECT_ID('dbo.Payment', 'U') IS NOT NULL DROP TABLE dbo.Payment;
IF OBJECT_ID('dbo.Result', 'U') IS NOT NULL DROP TABLE dbo.Result;
IF OBJECT_ID('dbo.Registration', 'U') IS NOT NULL DROP TABLE dbo.Registration;
IF OBJECT_ID('dbo.Race', 'U') IS NOT NULL DROP TABLE dbo.Race;
IF OBJECT_ID('dbo.Event', 'U') IS NOT NULL DROP TABLE dbo.Event;
IF OBJECT_ID('dbo.Sponsor', 'U') IS NOT NULL DROP TABLE dbo.Sponsor;
IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL DROP TABLE dbo.[User];
GO

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
CONSTRAINT FK_Event_Organizer FOREIGN KEY (organizer_id)
REFERENCES dbo.[User](user_id) ON DELETE NO ACTION
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
CONSTRAINT FK_Race_Event FOREIGN KEY (event_id)
REFERENCES dbo.Event(event_id) ON DELETE CASCADE
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
CONSTRAINT FK_EventSponsor_Event FOREIGN KEY (event_id)
REFERENCES dbo.Event(event_id) ON DELETE CASCADE,
CONSTRAINT FK_EventSponsor_Sponsor FOREIGN KEY (sponsor_id)
REFERENCES dbo.Sponsor(sponsor_id) ON DELETE CASCADE
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
CONSTRAINT FK_Registration_User FOREIGN KEY (user_id)
REFERENCES dbo.[User](user_id) ON DELETE CASCADE,
CONSTRAINT FK_Registration_Race FOREIGN KEY (race_id)
REFERENCES dbo.Race(race_id) ON DELETE NO ACTION,
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
CONSTRAINT FK_Result_Registration FOREIGN KEY (registration_id)
REFERENCES dbo.Registration(registration_id) ON DELETE CASCADE,
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
CONSTRAINT FK_Payment_Registration FOREIGN KEY (registration_id)
REFERENCES dbo.Registration(registration_id) ON DELETE CASCADE,
CONSTRAINT UQ_Payment_Registration UNIQUE (registration_id)
);
GO
