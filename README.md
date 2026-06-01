# 🛠️ ServiceDesk - IT Service Management System

<p align="center">
  <img src="screenshots/dashboard.png" alt="ServiceDesk Dashboard" width="700"/>
</p>

<p align="center">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white">
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET%2010-512BD4?style=flat-square&logo=dotnet&logoColor=white">
  <img alt="Blazor" src="https://img.shields.io/badge/Blazor-512BD4?style=flat-square&logo=blazor&logoColor=white">
  <img alt="MySQL" src="https://img.shields.io/badge/MySQL-4479A1?style=flat-square&logo=mysql&logoColor=white">
  <img alt="Tailwind CSS" src="https://img.shields.io/badge/Tailwind_CSS-06B6D4?style=flat-square&logo=tailwind-css&logoColor=white">
</p>

---

## 📌 Overview

**ServiceDesk** is a modern, responsive IT Service Management (ITSM) system built with **.NET 10** and **Blazor Server**. It simplifies incident tracking, SLA management, and team collaboration with native multilingual support (English & Ukrainian) and role-based access control (RBAC).

> 💻 **Setup Note**: Uses a local MySQL database. Connection settings are in `appsettings.Development.json`. For Docker deployment, use environment variables.

---

## 📸 System Screenshots

### 🔑 Authentication & Login
| English Version | Ukrainian Version |
| :---: | :---: |
| <img src="screenshots/login-en.png" alt="Login English" width="350"/> | <img src="screenshots/login-uk.png" alt="Login Ukrainian" width="350"/> |

### 📊 Dashboard & Tickets
| Main Dashboard | Ticket Details |
| :---: | :---: |
| <img src="screenshots/dashboard.png" alt="Dashboard" width="350"/> | <img src="screenshots/ticket-info.png" alt="Ticket Details" width="350"/> |

### ⚙️ Management & Panels
| Admin Panel | Product Management |
| :---: | :---: |
| <img src="screenshots/admin-panel.png" alt="Admin Panel" width="350"/> | <img src="screenshots/admin-panel-product.png" alt="Product Management" width="350"/> |

| Tech Stack Management | Developer Panel |
| :---: | :---: |
| <img src="screenshots/admin-panel-techstack.png" alt="Tech Stack" width="350"/> | <img src="screenshots/panel-dev.png" alt="Developer Panel" width="350"/> |

---

## 🗺️ Roadmap & Features

- [x] **Multilingual UI** (English/Ukrainian)
- [x] **Interactive Dashboard** (MTTR, MTBF, SLA compliance metrics)
- [x] **Enhanced RBAC** (Role-Based Access Control & Permissions)
- [x] **Email Integration** (SMTP notifications)
- [x] **Mobile Adaptation** (Native mobile web-app for phones)
- [x] **RESTful API** for third-party integrations
- [ ] **Advanced SLA Policies** (Automated escalations & breach notifications)
- [ ] **Chat Integration** (Teams & Slack webhooks) ???
- [x] **Audit Logging** (Comprehensive system activity tracking)
- [ ] **Knowledge Base** (FAQ & self-service modules)
- [ ] **Advanced Reporting** (Analytics and export engine)

---

## ⚙️ Tech Stack

- **Frontend:** Blazor Server + Tailwind CSS (dynamic, interactive UI)
- **Backend:** ASP.NET Core (.NET 10)
- **Database:** MySQL 8.0+ (Entity Framework Core)

---

## 📄 License & Info

Licensed under the [MIT License](LICENSE). Contributions, issues, and feature requests are welcome!
