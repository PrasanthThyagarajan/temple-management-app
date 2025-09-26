-- Fix Expenses table schema to match the entity
-- This script adds the missing columns and renames existing ones

-- First, let's add the missing columns
ALTER TABLE Expenses ADD COLUMN RequestedBy INTEGER;
ALTER TABLE Expenses ADD COLUMN ApprovedBy INTEGER;
ALTER TABLE Expenses ADD COLUMN ApprovedOn TEXT;

-- Copy data from old column to new column
UPDATE Expenses SET ApprovedBy = ApprovedByUserId WHERE ApprovedByUserId IS NOT NULL;

-- Set default RequestedBy to 1 for existing records
UPDATE Expenses SET RequestedBy = 1 WHERE RequestedBy IS NULL;

-- Verify the schema
.schema Expenses
