# RaceDay Platform - Full Stack Application

A complete, end-to-end web platform for managing sailing regattas and catamaran racing events. This repository contains the RESTful API, the MVC Web Application, and the automated testing suite.

## 📋Table of Contents
1. [System Overview](#-system-overview)
2. [Architecture & Tech Stack](#-architecture--tech-stack)
3. [User Roles](#-user-roles)
4. [Setup & Running Locally](#-setup--running-locally)
5. [Docker Containerization](#-docker-containerization)
6. [Azure Blob Storage](#-azure-blob-storage)
7. [CI/CD Pipeline](#-cicd-pipeline)
8. [YouTube Walkthrough](https://youtu.be/4gpE-81fQNU)

---

##  System Overview
The RaceDay platform allows sailing clubs to organize events and participants to enroll in races. 
*   **Part 1:** System planning (ERD, API Endpoint Plan, SQL Script) — located in the `/docs` folder.
*   **Part 2:** RESTful API built with ASP.NET Core 8 and Entity Framework Core.
*   **Part 3:** ASP.NET Core MVC front-end consuming the API, featuring session management, role-based UI, and Azure Blob Storage integration for event banners.

---

##  Architecture & Tech Stack

The solution is split into three projects:
1.  **`RaceDayAPI`**: The RESTful backend. Uses SQL Server, EF Core (Code First), and JWT Authentication. It handles all business logic and Azure Blob Storage uploads.
2.  **`RaceDay.MVC`**: The front-end. It contains **no direct database access**. It communicates with the API using `HttpClient` and maintains user state using Session Cookies.
3.  **`RaceDay.Tests`**: xUnit test suite for validating API authentication and role logic.

**Technologies Used:**
*   C# 12 / .NET 8
*   ASP.NET Core Web API & MVC
*   Entity Framework Core (SQL Server)
*   JWT & Session Authentication
*   Azure.Storage.Blobs (SDK)
*   Docker
*   xUnit
*   GitHub Actions

---

## User Roles

The MVC application features dynamic navigation based on the user's role:
*   **Organiser:** Can create, edit, and delete events; manage categories; view all enrolments; capture results; and upload event banner images to Azure.
*   **Participant:** Can view upcoming events, enroll in categories, view their personal enrollment history, and view their published results.
*   **Unauthenticated Users:** Can only view public event listings and are blocked from protected pages.

---

##  Setup & Running Locally

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or LocalDB)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containerization)

### 1. Database Setup
1. Open `RaceDayAPI/appsettings.json` and verify your SQL Server connection string.
2. Open the Package Manager Console (Default project: `RaceDayAPI`) and run:
   ```powershell
   Update-Database
