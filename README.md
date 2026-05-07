# Network Application Development Project
## 🏦 Currency Exchange Office System

### 📘 Course Information
* **Course Name:** Network Application Development

### 📝 Project Title
**Distributed Currency Exchange System using WCF, WPF, and SQL Server**

### 👤 Author
* **Name:** Fatih Sanlı
* **Student ID:** 64294

---

### 🚀 Project Description
This project is a comprehensive network-based application simulating a real-world currency exchange office. It features a distributed architecture where a WCF Service handles business logic and external API communication, while a WPF client provides a modern user interface.

**Key Features:**
* **Live Data:** Fetches real-time Bid/Ask rates from the **National Bank of Poland (NBP) API**.
* **Historical Analysis:** Displays 7-day historical exchange rate trends.
* **User Management:** Secure Registration and Login system.
* **Virtual Wallet:** Ability to top-up PLN balance and manage multiple foreign currencies.
* **Transaction Logging:** Every buy/sell operation is recorded in the SQL database for auditing.

---

### 📂 Repository Structure
* **`/WCF-Service`**: Source code for the backend WCF Web Service.
* **`/Client-Application`**: Source code for the WPF client application.
* **`/Database`**: Contains `schema.sql` for database initialization.
* **`/Documentation`**: Detailed system architecture and functionality report.

---

### 🛠 How to Run the Project

1.  **Database Setup:**
    * The application uses **SQL Server LocalDB** `(localdb)\MSSQLLocalDB`.
    * The database `ExchangeDb` and required tables are automatically created by the service on the first run.
    * *Optional:* You can manually run the script found in `/Database/schema.sql`.

2.  **Start the Web Service:**
    * Open `MyWcfService.sln` in Visual Studio.
    * Right-click on the `MyWcfService` project and select **View in Browser** or press **F5** to start the service.
    * Ensure the service is running at the designated local address.

3.  **Start the Client Application:**
    * Open the `WpfApp1` project.
    * Ensure the **Service Reference** is updated to point to your running WCF service.
    * Run the application.

4.  **Using the App:**
    * Register a new user.
    * Login and use the "Add PLN" button to top-up your balance.
    * Enter a currency code (e.g., USD, EUR) to see live rates and perform transactions.

---

### 🛠 Technologies Used
* **Backend:** WCF (Windows Communication Foundation), .NET
* **Frontend:** WPF (Windows Presentation Foundation), XAML
* **Database:** Microsoft SQL Server (LocalDB), ADO.NET
* **External API:** NBP API (JSON)


<img width="1915" height="1080" alt="image" src="https://github.com/user-attachments/assets/c6573c67-c986-48e6-89c1-562389cea76c" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/8e145851-3577-437f-b814-c4bc11a94953" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/35382214-e33a-4668-bf9b-1136e40b5145" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/f0e06a9d-6ce2-4b81-aeb7-a1a664328c28" />

<img width="1920" height="1035" alt="image" src="https://github.com/user-attachments/assets/7e10d7b2-3577-48f4-9ca9-fb153c1ab99e" />

<img width="1920" height="1041" alt="image" src="https://github.com/user-attachments/assets/f7d3fa8d-e632-4ded-8cfe-630af3c10117" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/33a2b5d3-1e69-4d9b-ac9d-33c4f815036d" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/4e4cb458-0c76-42b1-ac7d-6e0f7d5e061b" />

<img width="1920" height="1045" alt="image" src="https://github.com/user-attachments/assets/50bfa2f9-79ba-4dbd-a2a8-fdc8c7059f2e" />

<img width="1920" height="1034" alt="image" src="https://github.com/user-attachments/assets/10d2b4f3-07f2-47d6-86e9-64f63c3e49ee" />

<img width="1920" height="1040" alt="image" src="https://github.com/user-attachments/assets/a78a9901-a861-4a8c-941d-c7c432687725" />







