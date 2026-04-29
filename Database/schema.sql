-- Currency Exchange Office - Database Schema
CREATE DATABASE ExchangeDb;
GO
USE ExchangeDb;
GO

CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(50) NOT NULL,
    BalancePLN DECIMAL(18,4) DEFAULT 0
);

CREATE TABLE UserCurrencies (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT,
    Currency NVARCHAR(10),
    Amount DECIMAL(18,4)
);

CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT,
    Currency NVARCHAR(10),
    TransactionType NVARCHAR(10), -- 'BUY' or 'SELL'
    Amount DECIMAL(18,4),
    Rate DECIMAL(18,4),
    TransactionDate DATETIME DEFAULT GETDATE()
);
