# RaceDay

RaceDay is a race/event management system. Any registered **User** can create and manage their own race events (e.g. marathons, trail runs, fun runs) made up of one or more **Races** (e.g. a 10km and a 21km on the same day), and any registered User can **register** for a race, get a bib number, and view their **Result** and **Payment** once registered. Events can also carry one or more **Sponsors**.

This repository currently contains **Part 1: System Planning and Database** — the ERD, API endpoint plan, and SQL database script for the system, committed inside `/docs` before any application code is written.

## Roles

RaceDay uses a single `User` table rather than separate role-specific tables — a person's role is defined by *how they use the system*, not by which table they sit in:

- **Organiser** — a User who creates events. Referenced as `organizer_id` on `Event`. An Organiser can create, update, and delete their own events; define the races within an event; attach sponsors to their events; view registrations for their races; and capture/update race results.
- **Participant** — a User who registers for races. A Participant can browse public events and races, register for a race (receiving a bib number), make a payment for their registration, view their own registrations, and view their result once a race has taken place.

The same person can act as both — nothing stops a User who organises one event from registering for another. Role is enforced at the API layer (checking whether the logged-in `user_id` matches `organizer_id`/`user_id` on the record being modified), not by a `Role` column on the table.

## Planning Documents (`/docs`)

| File | Description |
|---|---|
| `RaceDay_ERD.png` | Entity Relationship Diagram for the full RaceDay data model (8 entities). |
| `API_Endpoint_Plan.md` | Full table of every planned API endpoint: method, route, description, role required, request body, and expected response. |
| `RaceDay_Schema.sql` | SQL Server script that creates the schema (with PKs, FKs, and constraints) and seeds it with sample data. |
| `validate-structure.yml` | GitHub Actions workflow (place in `.github/workflows/`) that checks the `/docs` folder and required files exist on every push/PR. |

### ERD Diagram

The ERD is included as a PNG file in the repository. You can view it <img width="2312" height="3179" alt="erd-draft-v5-final" src="https://github.com/user-attachments/assets/27001f7b-233c-4222-acd4-b408df054332" />


**Key relationships:**

- `User` (1) to `Event` (M): one-to-many. A User can organise many Events (`Event.organizer_id`).
- `Event` (1) to `Race` (M): one-to-many. An Event can have many Races (e.g. different distances).
- `User` (1) to `Registration` (M): one-to-many. A User can hold many Registrations, across different races.
- `Race` (1) to `Registration` (M): one-to-many. A Race can have many Registrations, but a User can only register once per Race (`UQ_Registration_UserRace`).
- `Registration` (1) to `Result` (1): one-to-one. Each Registration can have one final Result.
- `Registration` (1) to `Payment` (1): one-to-one. Each Registration can have one Payment.
- `Event` (M) to `Sponsor` (M): many-to-many, resolved through the `EventSponsor` junction table (composite primary key of `event_id` + `sponsor_id`).

## CI/CD

A GitHub Actions workflow (`validate-structure.yml`) validates that the `/docs` folder exists and contains the required planning files on every push and pull request to `main`.

**CI screenshot:** 

## Video Walkthrough

**YouTube (unlisted):** _

## Repository Structure

```
/
├── docs/
│   ├── RaceDay_ERD.png
│   ├── API_Endpoint_Plan.md
│   └── RaceDay_Schema.sql
├── .github/
│   └── workflows/
│       └── validate-structure.yml
└── README.md
```

