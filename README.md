*RaceDay - Race Event Management System*
 *Part 1 Planning*

*Brief Description of the System*

RaceDay is a role-based REST API platform for managing running events. The system allows Organisers to create and manage race events, define multiple categories per event, track enrolments, and publish official results. Participants can browse upcoming events, enrol in specific categories, manage their enrolments, and view their race results history. The system enforces strict role-based access control via JWT authentication.

*Roles Description*
1. *Organiser*
*Purpose*: Event creator and manager
*Capabilities*
Register/login, manage own profile, CRUD own Events, CRUD Categories for own Events, view all Enrolments for own Events, publish/edit/delete Results for own Events, view Participants list
*Restrictions*: Cannot enrol in events as a Participant, cannot edit/delete other Organisers' events, cannot enrol if event has passed or category is full.

2.* Participant*
*Purpose*: End-user who competes in races
*Capabilities*: Register/login, manage own profile, browse all Events and Categories (public), enrol in a Category, view own Enrolments, cancel own Enrolment before results are published, view Results for any Category/Event, view own results history
*Restrictions*: Cannot create events, cannot publish results, cannot view other participants' private data, cannot double-enrol in same category (UNIQUE constraint)

*Folder Structure*
/docs
  ├── erd.png               
  ├── erd_description.md     
  ├── endpoint_plan.md       
  └── schema.sql            
.github/workflows/
  └── validate.yml           
README.md    

ERD
<img width="1920" height="1280" alt="raceday_erd" src="https://github.com/user-attachments/assets/e5885c35-ee0a-413b-b494-16b812a9c498" />


*Entities*

Users (UserId PK, Email UNIQUE, Role CHECK), Events (EventId PK, OrganiserId FK), Categories (CategoryId PK, EventId FK, UNIQUE EventId+Name), Enrolments (EnrolmentId PK, ParticipantId FK, CategoryId FK, UNIQUE Participant+Category), Results (ResultId PK, EnrolmentId FK UNIQUE)

*Key Design Decisions:*

Single Users table with Role discriminator for simpler Auth - satisfies 2 Organisers + 2 Participants seed requirement
Categories separate from Events to allow one event to have multiple distances/fees
Enrolments as junction table with UNIQUE(ParticipantId, CategoryId) to prevent duplicate enrolments - maps to POST /api/categories/{id}/enrolments 409 Conflict
Results 1-to-1 with Enrolments (UNIQUE FK) - ensures one result per enrolment
All FKs ON DELETE CASCADE, CHECK constraints for data integrity (Distance >0, Fee >=0, FinishTime >0)


*Endpoint Plan*
See docs/endpoint_plan.md - 26 endpoints covering:

Authentication (register, login) - Role: None
User Profile (GET/PUT me) - Role: Any
Events (CRUD + list) - Organiser for write, None for read
Categories (CRUD per event) - Organiser for write, None for read
Event Enrolments (enrol, list, cancel) - Participant for enrol, Organiser for view
Results (publish, list, update) - Organiser for write, None for read
Each endpoint includes Method, Route (/api/...), Description, Role Required, Request Body, Expected Response with failure codes (400, 401, 403, 404, 409).

*SQL Script*
File: docs/schema.sql

Tested on clean SQL Server instance (SSMS 20+)
Creates database RaceDayDB
Creates 5 tables with PK, FK, UNIQUE, NOT NULL, CHECK, DEFAULT constraints
Seeds: 2 Organisers (Sarah, David), 3 Participants (Thabo, Emily, Lerato), 3 Events (Cape Town, Stellenbosch, Joburg), 7 Categories, 6 Enrolments, 2 Results
Final verification SELECT at end
To run: Open SSMS > Open File > Execute > Check Messages for count verification.

*CI/CD*
Workflow: .github/workflows/validate.yml

Triggers on push to main/master and PRs
Steps: Checkout, check /docs exists, check required files present, validate SQL file exists and non-empty
Generates green build badge

*CI/CD Screenshot*
Image unavailable. Please retry the request.
Insert screenshot of successful green build here after first push to GitHub. Workflow name: Validate Repository Structure


*Video Presentation*
Unlisted YouTube Link: [INSERT YOUR YOUTUBE LINK HERE]
Video covers:

ERD decisions and cardinality explanation
Endpoint plan choices and role matrix
SQL script design with constraints
Live run of schema.sql in SSMS
