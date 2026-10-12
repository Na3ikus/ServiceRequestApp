# 🛠️ ServiceDesk — IT Service Management System

<p align="center">
  <img src="screenshots/dashboard.png" alt="ServiceDesk Dashboard" width="700"/>
</p>

<p align="center">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white">
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET%2010-512BD4?style=flat-square&logo=dotnet&logoColor=white">
  <img alt="Blazor" src="https://img.shields.io/badge/Blazor-512BD4?style=flat-square&logo=blazor&logoColor=white">
  <img alt="MySQL" src="https://img.shields.io/badge/MySQL-4479A1?style=flat-square&logo=mysql&logoColor=white">
  <img alt="Tailwind CSS" src="https://img.shields.io/badge/Tailwind_CSS-06B6D4?style=flat-square&logo=tailwind-css&logoColor=white">
  <img alt="Docker" src="https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white">
</p>

---

## 📌 Overview

**ServiceDesk** is a modern, responsive IT Service Management (ITSM) system built with **.NET 10** and **Blazor Server**. It simplifies incident tracking, SLA management, and team collaboration with native multilingual support (🇬🇧 English / 🇺🇦 Ukrainian / 🇩🇪 German / 🇪🇸 Spanish / 🇫🇷 French) and role-based access control (RBAC).

Since **v2.0** the UI follows an "ops terminal" design: Geist and Geist Mono, design tokens instead of ad-hoc colours, `~/path` headings, single-hue charts, one accent colour (lime, amber, phosphor or ice) in a light or dark theme, and a `Ctrl+K` command palette that works like a small shell (`cd ~/tickets`, `theme light`, `accent amber`, `lang uk`) with live preview.

> 💻 **Setup Note:** Uses a local MySQL database. Connection settings are in `appsettings.Development.json`. For Docker deployment, use environment variables.
>
> 🎨 **Styles:** Requires **Node.js 22+**. `dotnet build` compiles Tailwind; use `-p:SkipTailwindBuild=true` to skip.

---

## 🏗️ Architecture

The project is built on **Clean Architecture** principles, separating concerns into four independent layers. The **Domain** layer defines the core entities (Ticket, User, Product, etc.) and interfaces with no external dependencies. The **Application** layer orchestrates business logic and use cases on top of those interfaces. The **Infrastructure** layer provides concrete implementations — EF Core persistence, MySQL, SMTP email, and migrations. Finally, two presentation entry points sit at the top: a **Blazor Server** app for the interactive UI (over SignalR) and a **REST API** for third-party integrations.

```mermaid
graph TD
    Browser["🌐 Browser / Mobile"]

    subgraph Presentation
        Blazor["Blazor Server\n(UI · SignalR)"]
        API["REST API\n(ASP.NET Core)"]
    end

    subgraph Application
        AppSvc["Services & Use Cases"]
    end

    subgraph Domain
        Entities["Entities & Interfaces\nTicket · User · Product · AuditLog…"]
    end

    subgraph Infrastructure
        EF["EF Core · MySQL 8.0+"]
        SMTP["📧 SMTP / Email"]
    end

    Browser -->|HTTP / WS| Blazor
    Browser -->|HTTP| API
    Blazor --> AppSvc
    API --> AppSvc
    AppSvc --> Entities
    AppSvc --> EF
    AppSvc --> SMTP
```

---

## 📸 Screenshots

### 🔑 Authentication

| Dark Mode | Light Mode |
| :---: | :---: |
| <img src="screenshots/login-dark.png" alt="Login Dark" width="350"/> | <img src="screenshots/login-light.png" alt="Login Light" width="350"/> |

### 📊 Home & Analytics

| Home | Statistics |
| :---: | :---: |
| <img src="screenshots/dashboard.png" alt="Home" width="350"/> | <img src="screenshots/statistics.png" alt="Statistics" width="350"/> |

### 🎫 Ticket Management

| Kanban Board | Developer Queue |
| :---: | :---: |
| <img src="screenshots/tickets-kanban.png" alt="Kanban Board" width="350"/> | <img src="screenshots/developer.png" alt="Developer Queue" width="350"/> |

| Calendar | Profile & Activity |
| :---: | :---: |
| <img src="screenshots/calendar.png" alt="Calendar" width="350"/> | <img src="screenshots/profile.png" alt="Profile" width="350"/> |

### ⚙️ Admin Panel & Audit Logs

| Administration | Audit Log Stream |
| :---: | :---: |
| <img src="screenshots/admin-products.png" alt="Administration" width="350"/> | <img src="screenshots/audit-logs.png" alt="Audit Logs" width="350"/> |

---

## ✅ Features

| Feature | Status |
|---|:---:|
| Multilingual UI (EN / UA / DE / ES / FR) | ✅ |
| Terminal-style design system (light / dark, 4 accents) | ✅ |
| Command palette with live theme & accent preview | ✅ |
| Statistics from real status history (period, time zone aware) | ✅ |
| Interactive Dashboard (MTTR, MTBF, SLA) | ✅ |
| Role-Based Access Control (RBAC) | ✅ |
| Email Notifications (SMTP) | ✅ |
| Mobile-Responsive Web App | ✅ |
| RESTful API | ✅ |
| Audit Logging | ✅ |
| Advanced SLA Policies & Escalations | ✅ |
| Knowledge Base & Self-Service | 🔜 |
| Advanced Reporting & Export | 🔜 |

---

## ⚙️ Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor Server + Tailwind CSS |
| Backend | ASP.NET Core (.NET 10) |
| Database | MySQL 8.0+ via Entity Framework Core |
| Containerization | Docker |

---

## 📄 License

Licensed under the [MIT License](LICENSE). Contributions, issues, and feature requests are welcome!
