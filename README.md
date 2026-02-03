#  ServiceDesk - IT Service Management System

<p align="center">
  <img src="screenshots/login-uk.png" alt="ServiceDesk Login" width="600"/>
</p>

<p align="center">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white">
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  <img alt="Blazor" src="https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white">
  <img alt="MySQL" src="https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white">
  <img alt="Tailwind CSS" src="https://img.shields.io/badge/Tailwind_CSS-06B6D4?style=for-the-badge&logo=tailwind-css&logoColor=white">
</p>

<p align="center">
  <strong>A modern, comprehensive IT Service Desk and Incident Management system</strong>
</p>

---

##  Overview

**ServiceDesk** is a powerful IT Service Management (ITSM) platform built with **.NET 10** and **Blazor Server**, designed to streamline incident tracking, service requests, and team collaboration. With multilingual support (English & Ukrainian), intuitive UI, and robust backend, it's perfect for IT departments of any size.

###  Key Highlights

-  **Real-time incident & request tracking**
-  **Multilingual interface** (English/Ukrainian)
-  **Interactive dashboards & analytics**
-  **Role-based access control (RBAC)**
-  **SLA management & escalations**
-  **Modern UI with Tailwind CSS**
-  **Built with .NET 10 & Blazor Server**

## What it’s for
- Centralize incident and request tracking.
- Enforce transparent SLAs and execution control.
- Improve communication between requesters and assignees.
- Provide analytics and dashboards to manage workload and service quality.

## Key capabilities
- Creation and routing of incidents/requests.
- Categorization, priorities, states, and change history.
- SLA policies and escalations.
- Notifications for assignees and requesters.
- Roles and access control.
- Search, filters, dashboards, and reports.

## Architectural notes
- **Interface**: Blazor Server (interactive UI without a separate SPA client).
- **Server**: ASP.NET Core, C#.
- **Data**: MySQL; migrations supported (typically EF Core).
- **UI**: HTML, CSS; auxiliary JavaScript when needed.
- **Automation/scripting**: PowerShell.

## Target use cases
- IT Service Desk in internal IT departments.
- Incident Management for critical services.
- Service catalog and service request handling.
- Metric collection for reporting and SLA improvement.

## 📸 Screenshots

<details>
<summary><b> Authentication & Login</b></summary>

### Login Page (English)
<img src="screenshots/login-en.png" alt="Login Page English" width="700"/>

### Login Page (Ukrainian)
<img src="screenshots/login-uk.png" alt="Login Page Ukrainian" width="700"/>

</details>

<details open>
<summary><b> Dashboard & Tickets</b></summary>

### Main Dashboard
<img src="screenshots/dashboard.png" alt="Dashboard" width="700"/>

### Ticket Details
<img src="screenshots/ticket-info.png" alt="Ticket Info" width="700"/>

</details>

<details>
<summary><b> Admin Panel</b></summary>

### Admin Panel
<img src="screenshots/admin-panel.png" alt="Admin Panel" width="700"/>

### Product Management
<img src="screenshots/admin-panel-product.png" alt="Product Management" width="700"/>

### Tech Stack Management
<img src="screenshots/admin-panel-techstack.png" alt="Tech Stack" width="700"/>

### Developer Panel
<img src="screenshots/panel-dev.png" alt="Developer Panel" width="700"/>

</details>

---

##  Project Status

>  **Active Development**: This repository is under active development. Features and capabilities may evolve.

### Current Version
- **Phase**: Beta
- **Framework**: .NET 10
- **Database**: MySQL 8.0+

---

##  Roadmap

###  Planned Features

- [ ] **Advanced SLA Policies**: Automated escalations and SLA breach notifications
- [ ] **Email Integration**: SMTP support for notifications
- [ ] **Chat Integration**: Teams/Slack webhook integration
- [ ] **KPI Dashboards**: MTTR, MTBF, SLA compliance metrics
- [ ] **Enhanced RBAC**: Expanded role management and permissions
- [ ] **Audit Logging**: Comprehensive system activity tracking
- [ ] **Knowledge Base**: FAQ and self-service modules
- [ ] **Mobile App**: Native mobile applications for iOS/Android
- [ ] **API**: RESTful API for third-party integrations
- [ ] **Reporting**: Advanced reporting and analytics engine

---

##  Contributing

We welcome contributions! Here's how you can help:

1.  **Report Bugs**: Open an issue with detailed reproduction steps
2.  **Suggest Features**: Share your ideas for new functionality
3.  **Submit Pull Requests**: Follow our coding conventions
4.  **Improve Documentation**: Help us make the docs better
5.  **Star the Project**: Show your support!

### Development Guidelines
- Follow C# coding conventions
- Write unit tests for new features
- Update documentation for any changes
- Use descriptive commit messages

---

##  Contact & Support

-  **Bug Reports**: [Open an Issue](https://github.com/Na3ikus/ServiceRequestApp/issues)
-  **Discussions**: [GitHub Discussions](https://github.com/Na3ikus/ServiceRequestApp/discussions)
-  **Feature Requests**: Share your ideas via Issues
-  **Email**: For private inquiries

---

##  License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  <a href="#top"> Back to Top</a>
</p>
