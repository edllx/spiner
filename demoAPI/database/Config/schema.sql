-- Create DB if not exists (outside of DO block)
SELECT 'CREATE DATABASE weddy'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'weddy')\gexec

-- Connect to the database
\c weddy

-- Users table
CREATE TABLE IF NOT EXISTS weathers (
    temperature SMALLINT NOT NULL,
    type VARCHAR(20),
    CONSTRAINT ck_weather_type CHECK (type IN ('Fahrenheit', 'Celsius'))
);
