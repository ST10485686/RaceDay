USE RaceDay;
GO

-- Verification: counts per table (Fixes SQL80001 near RowCount)
SELECT 'User' AS TableName, COUNT(*) AS Total FROM dbo.[User] UNION ALL
SELECT 'Event', COUNT(*) FROM dbo.Event UNION ALL
SELECT 'Race', COUNT(*) FROM dbo.Race UNION ALL
SELECT 'Sponsor', COUNT(*) FROM dbo.Sponsor UNION ALL
SELECT 'EventSponsor', COUNT(*) FROM dbo.EventSponsor UNION ALL
SELECT 'Registration', COUNT(*) FROM dbo.Registration UNION ALL
SELECT 'Result', COUNT(*) FROM dbo.Result UNION ALL
SELECT 'Payment', COUNT(*) FROM dbo.Payment
ORDER BY TableName;
GO

-- Quick sanity checks
SELECT TOP 5 * FROM dbo.[User];
SELECT TOP 5 * FROM dbo.Event;
SELECT TOP 5 * FROM dbo.Registration;
GO
