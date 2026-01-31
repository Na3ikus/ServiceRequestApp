# ServiceRequestApp

<p align="left">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  <img alt="Blazor" src="https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white">
  <img alt="Entity Framework Core" src="https://img.shields.io/badge/EF%20Core-5C2D91?style=for-the-badge&logo=efcore&logoColor=white">
  <img alt="MySQL" src="https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white">
  <img alt="HTML5" src="https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white">
  <img alt="CSS3" src="https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white">
  <img alt="Tailwind CSS" src="https://img.shields.io/badge/Tailwind_CSS-06B6D4?style=for-the-badge&logo=tailwind-css&logoColor=white">
  <img alt="JavaScript" src="https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black">
</p>

## What it is
ServiceRequestApp is a comprehensive IT Service Desk and Incident Management system built on .NET (Blazor Server) with MySQL. It enables registering, tracking, and escalating incidents and service requests, supports SLA transparency, and facilitates team collaboration.

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
- **Data**: MySQL with Entity Framework Core (Pomelo provider); migrations supported.
- **UI**: HTML, CSS, Tailwind CSS; auxiliary JavaScript when needed.
- **Automation/scripting**: PowerShell.

## Target use cases
- IT Service Desk in internal IT departments.
- Incident Management for critical services.
- Service catalog and service request handling.
- Metric collection for reporting and SLA improvement.

## Project status
- Repository is under active development; capabilities may evolve.

## Roadmap (example directions)
- Advanced SLA policies and auto-escalations.
- Integrations with email/chat (SMTP, Teams/Slack webhooks).
- KPI dashboards (MTTR, MTBF, SLA compliance).
- Expanded RBAC, auditing, logging.
- Knowledge/FAQ modules for self-service.

## How to contribute
- Review open issues and discussions.
- Propose improvements or report problems.
- Follow project style and quality conventions.

## Contact
- Open an issue in the repository for bugs, requests, or ideas.
- Suggestions for integrations and new features are welcome.
