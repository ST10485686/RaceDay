USE RaceDay;
GO

INSERT INTO dbo.[User] (email, password_hash, full_name, date_of_birth, gender, phone, emergency_contact)
VALUES
('sarah.morgan.org@raceday.co.za', 'hash_123456', 'Sarah Johnson', '1985-03-15', 'F', '+1-555-0101', 'Mike Johnson'),
('m.chen.events@raceday.co.za', 'hash_789012', 'Michael Chen', '1978-11-22', 'M', '+1-555-0201', 'Lisa Chen'),
('emma.wilson.runner@outlook.com', 'hash_345678', 'Emma Wilson', '1992-07-08', 'F', '+1-555-0301', 'David Wilson'),
('j.rodriguez.run@gmail.com', 'hash_901234', 'James Rodriguez', '1988-09-23', 'M', '+1-555-0401', 'Anna Rodriguez');
GO

INSERT INTO dbo.Event (organizer_id, event_name, event_date, location, city, country, description) VALUES
(1, 'City Marathon 2026', '2026-11-15', 'Downtown Convention Center', 'New York', 'USA', 'Annual city marathon'),
(1, 'Spring 5K Fun Run', '2026-05-10', 'Central Park', 'Boston', 'USA', 'Family-friendly 5K'),
(2, 'Trail Challenge Series', '2026-08-22', 'Mountain View Park', 'Denver', 'USA', 'Trail runs');
GO

INSERT INTO dbo.Sponsor (sponsor_name, website, contact_email, phone) VALUES
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
(1, '10K Run', 10.00, '2026-11-15 09:30:00', '2026-11-15 11:30:00', '16+', 1500),
(2, '5K Competitive', 5.00, '2026-05-10 09:00:00', '2026-05-10 11:00:00', '18+', 500),
(2, '5K Family Walk', 5.00, '2026-05-10 10:30:00', '2026-05-10 12:30:00', 'All Ages', 300),
(2, 'Kids Fun Run', 1.00, '2026-05-10 11:00:00', '2026-05-10 12:00:00', '5-12', 100),
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
