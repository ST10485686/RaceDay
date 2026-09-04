# **RaceDay**

RaceDay is a race/event management system that lets Organisers create and manage race events (e.g. marathons, trail runs, fun runs), and lets Participants discover events, enrol in specific entry categories, and view their results once a race has taken place.

This repository currently contains Part 1: System Planning and Database — the ERD, API endpoint plan, and SQL database script for the system, committed inside /docs before any application code is written.

## **Roles**
Organiser — represents a race organisation. An Organiser can create, update, and delete their own events; define entry categories per event; view and manage enrolments for their events; attach documents (route maps, rules) to their events; and capture/update race results.
Participant — represents a person taking part in races. A Participant can browse public events and categories, enrol themselves into a category (receiving a bib number), view their own enrolments, withdraw from a category, and view results for events they took part in.

Both roles are registered through the same Users table and are distinguished by a Role column; Organisers and Participants each extend Users in a one-to-one relationship, matching the ERD below.

###**Planning Documents (/docs)**

**File	Description**
RaceDay_ERD.png	Entity Relationship Diagram for the full RaceDay data model (8 entities).
API_Endpoint_Plan.md	Full table of every planned API endpoint: method, route, description, role required, request body, and expected response.
RaceDay_Schema.sql	SQL Server script that creates the schema (with PKs, FKs, and constraints) and seeds it with sample data.
validate-structure.yml	GitHub Actions workflow (place in .github/workflows/) that checks the /docs folder and required files exist on every push/PR.
**ERD Diagram**

The ERD is included as a PNG file in the repository. You can view it here.

Key relationships:

Users (1) to Organisers (1): one-to-one. An Organiser is a specific type of User.
Users (1) to Participants (1): one-to-one. A Participant is a specific type of User.
Organisers (1) to Events (M): one-to-many. One Organiser can create many Events.
Events (1) to Categories (M): one-to-many. An Event can have many entry Categories.
Categories (1) to EventEnrolments (M): one-to-many. A Category can have many Enrolments.
Participants (1) to EventEnrolments (M): one-to-many. A Participant can hold many Enrolments (across different events/categories).
EventEnrolments (1) to Results (1): one-to-one. Each Enrolment can have one final Result.
Events (1) to Documents (M): one-to-many. An Event can have many associated Documents.

**CI/CD Workflow**

A GitHub Actions workflow (validate-structure.yml) validates that the /docs folder exists and contains the required planning files on every push and pull request to main.

**CI screenshot**:


**YouTube (unlisted)**: 

**Repository Structure**
/
├── docs/
│   ├── RaceDay_ERD.png
│   ├── API_Endpoint_Plan.md
│   └── RaceDay_Schema.sql
├── .github/
│   └── workflows/
│       └── validate-structure.yml
└── README.md
