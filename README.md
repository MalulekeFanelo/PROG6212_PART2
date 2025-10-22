
# CMCS_MVC_Prototype – Part 2

 YOUTUBE LINK: https://youtu.be/q3sShRne3Xg
## Contract Monthly Claim System

A comprehensive MVC web application for managing monthly claims submission, approval, and payment processing for contract lecturers.

---

## 🚀 Project Overview

The **Contract Monthly Claim System (CMCS)** is a full-stack web application built with ASP.NET Core MVC that streamlines the monthly claim process for academic institutions. The system provides separate interfaces for Lecturers, Coordinators, and Managers to handle the complete claim lifecycle.

---

## ✨ Features

### 👨‍🏫 For Lecturers
- **Submit Claims**: Intuitive form with auto-calculation of totals
- **Auto-generated Lecturer IDs**: System automatically generates unique lecturer IDs from names
- **File Upload**: Support for PDF, Word, and image documents
- **Real-time Status Tracking**: Monitor claim progress through approval stages
- **Search Functionality**: Filter claims by lecturer ID

### 👨‍💼 For Coordinators
- **Pending Claims Review**: View and verify submitted claims
- **Approve/Reject Actions**: Single-click approval or rejection
- **Document Verification**: Access uploaded supporting documents
- **Clean Empty State**: Professional display when no pending claims exist

### 👔 For Managers
- **Payment Processing**: Final approval and payment authorization
- **Coordinator-Approved Claims**: Review claims that passed initial verification
- **Audit Trail**: Track approval history and actions
- **Empty State Handling**: Clean interface when all claims are processed

---

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 9.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery, Razor Pages
- **Authentication**: Windows Authentication (ready for extension)
- **File Handling**: IFormFile with secure uploads to wwwroot/Documents
- **Testing**: xUnit, Moq, FluentAssertions

---

## 📋 Prerequisites

- .NET 9.0 
- SQL Server Express 2019 or later
- Visual Studio 2022 or VS Code
- SQL Server Management Studio (SSMS)

---

## 🚦 Installation & Setup

### 1. Database Setup
```bash
# Apply migrations
dotnet ef database update

# Or create database manually in SSMS
CREATE DATABASE CMCS_Database;
```

### 2. Configuration
Update `appsettings.json` with your SQL Server connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server\\SQLEXPRESS;Database=CMCS_Database;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 3. Run the Application
```bash
dotnet run
# or
dotnet watch run
```

---

## 🗂️ Project Structure

```
CMCS_MVC_Prototype/
├── Controllers/
│   ├── ClaimsController.cs
│   ├── CoordinatorController.cs
│   ├── ManagerController.cs
│   └── HomeController.cs
├── Models/
│   └── Claim.cs
├── Views/
│   ├── Claims/
│   │   ├── Index.cshtml      # My Claims view
│   │   └── Create.cshtml     # Submit claim form
│   ├── Coordinator/
│   │   └── Index.cshtml      # Coordinator dashboard
│   ├── Manager/
│   │   └── Index.cshtml      # Manager dashboard
│   └── Shared/
│       └── _Layout.cshtml    # Main layout
├── Services/
│   └── ClaimService.cs       # Business logic layer
├── Data/
│   └── ApplicationDbContext.cs
└── wwwroot/
    └── Documents/            # File upload directory
```

---

## 🔄 System Workflow

1. **Lecturer Submission** → Claim created with "Pending" status
2. **Coordinator Review** → Approve/Reject with status updates
3. **Manager Processing** → Final approval and payment marking
4. **Status Tracking** → Real-time updates across all roles

### Claim Status Flow:
```
Pending → Approved by Coordinator → Approved by Manager (Paid)
                    ↓
            Rejected by Coordinator
                    ↓
            Rejected by Manager
```

---

## 🧪 Testing

### Running Tests
```bash
# Navigate to test project
cd CMCS_MVC.Tests

# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage
- ✅ Controller actions and routing
- ✅ Model validation and business logic
- ✅ File upload functionality
- ✅ Database operations (using in-memory DB)
- ✅ Error handling and edge cases

---

## 📁 File Upload System

### Supported File Types
- PDF documents (.pdf)
- Word documents (.doc, .docx)
- Images (.jpg, .jpeg, .png)

### Storage Location
- Uploaded files are saved to `wwwroot/Documents/`
- Files are renamed with GUID prefixes for security
- Maximum file size: 10MB

### Security Features
- File type validation
- Size limits enforcement
- Secure filename handling
- Unique file naming to prevent conflicts

---

## 🎨 UI/UX Features

- **Responsive Design**: Works on desktop, tablet, and mobile
- **Professional Styling**: Bootstrap 5 with custom CSS
- **Emoji Icons**: No external dependencies for icons
- **Auto-calculation**: Real-time total amount calculation
- **Form Validation**: Client and server-side validation
- **User Feedback**: Success/error messages with TempData
- **Empty States**: Professional handling of no-data scenarios

---

## 🔧 Key Code Features

### Auto Lecturer ID Generation
```csharp
// Generates IDs like: SMIJO489 from "John Smith"
public void GenerateLecturerId()
{
    // Format: First 3 of last name + first 2 of first name + random 3 digits
}
```

### Real-time Total Calculation
```javascript
// Automatic total calculation in submit form
$('#hoursWorked, #hourlyRate').on('input', calculateTotal);
```

### Status Badge System
```html
<span class="badge bg-warning text-dark">Pending</span>
<span class="badge bg-success">Approved</span>
<span class="badge bg-danger">Rejected</span>
```

---

## 🗃️ Database Schema

### Claims Table
| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary key, auto-increment |
| LecturerName | NVARCHAR(100) | Required |
| LecturerId | NVARCHAR(20) | Auto-generated |
| Month | NVARCHAR(7) | Format: YYYY-MM |
| HoursWorked | DECIMAL(10,2) | 0.1-1000 range |
| HourlyRate | DECIMAL(10,2) | 1-500 range |
| Total | DECIMAL(10,2) | Computed (Hours × Rate) |
| Status | NVARCHAR(50) | Default: "Pending" |
| DocumentPath | NVARCHAR(255) | File storage path |
| Submitted | DATETIME2 | Auto-set to current time |

---

## 🔒 Security Features

- **Anti-forgery Tokens**: All forms include validation
- **Parameter Binding**: Model binding with validation
- **SQL Injection Protection**: Entity Framework parameterization
- **File Upload Security**: Type and size validation
- **Status-based Authorization**: Role-appropriate actions

---

