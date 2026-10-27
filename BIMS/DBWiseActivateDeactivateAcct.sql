USE BaranggayInformationSystem;

UPDATE dbo.users
SET is_active = 0
WHERE username = 'staff_username';

UPDATE dbo.users
SET is_active = 1
WHERE username = 'staff_username';