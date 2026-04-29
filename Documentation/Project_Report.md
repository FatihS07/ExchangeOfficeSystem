# System Architecture and Functionality Report

## 1. System Overview
The Currency Exchange Office system is a distributed network application developed using the .NET platform. It follows a Client-Server architecture.

## 2. Architecture Components
* **Backend (Web Service):** Built with WCF. Handles logic, database, and NBP API integration.
* **Frontend (Client App):** Built with WPF. Provides the user interface for transactions.
* **Database:** SQL Server (LocalDB). Stores Users, Balances, and Transaction Logs.

## 3. Key Functionalities
* Live and historical rate fetching from NBP API.
* User authentication and wallet management.
* Currency trading (Buy/Sell) with automated transaction logging.
