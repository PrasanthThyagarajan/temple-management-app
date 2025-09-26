-- Add missing columns to Expenses table
ALTER TABLE Expenses ADD COLUMN RequestedBy INTEGER;
ALTER TABLE Expenses ADD COLUMN ApprovedBy INTEGER;
ALTER TABLE Expenses ADD COLUMN ApprovedOn TEXT;

-- Remove old column if it exists (this might fail if column doesn't exist, that's ok)
-- ALTER TABLE Expenses DROP COLUMN ApprovedByUserId;
-- ALTER TABLE Expenses DROP COLUMN ApprovedAt;
