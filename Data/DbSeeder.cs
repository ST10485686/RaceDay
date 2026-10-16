using RaceDay.API.Helpers;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public static class DbSeeder
{
    public static void Seed(RaceDayDbContext context)
    {
        // Only seed if there are no users yet
        if (context.Users.Any())
            return;

        // ---------- 1. USERS ----------
        var organiser1User = new User
        {
            Email = "organiser1@raceday.com",
            PasswordHash = PasswordHasher.HashPassword("Password123!"),
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = new DateTime(1980, 5, 15),
            Role = "Organiser",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var organiser2User = new User
        {
            Email = "organiser2@raceday.com",
            PasswordHash = PasswordHasher.HashPassword("Password123!"),
            FirstName = "Sarah",
            LastName = "Johnson",
            DateOfBirth = new DateTime(1975, 8, 22),
            Role = "Organiser",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var participant1User = new User
        {
            Email = "sailor1@raceday.com",
            PasswordHash = PasswordHasher.HashPassword("Password123!"),
            FirstName = "Michael",
            LastName = "Brown",
            DateOfBirth = new DateTime(1992, 3, 10),
            Role = "Participant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var participant2User = new User
        {
            Email = "sailor2@raceday.com",
            PasswordHash = PasswordHasher.HashPassword("Password123!"),
            FirstName = "Emma",
            LastName = "Wilson",
            DateOfBirth = new DateTime(1995, 11, 1),
            Role = "Participant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var participant3User = new User
        {
            Email = "sailor3@raceday.com",
            PasswordHash = PasswordHasher.HashPassword("Password123!"),
            FirstName = "David",
            LastName = "Taylor",
            DateOfBirth = new DateTime(1988, 7, 19),
            Role = "Participant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(
            organiser1User, organiser2User,
            participant1User, participant2User, participant3User);
        context.SaveChanges();

        // ---------- 2. ORGANISERS ----------
        var organiser1 = new Organiser
        {
            UserID = organiser1User.UserID,
            OrganisationName = "Cape Town Sailing Club",
            ContactPhone = "+27 82 123 4567",
            IsVerified = true
        };

        var organiser2 = new Organiser
        {
            UserID = organiser2User.UserID,
            OrganisationName = "Durban Yacht Club",
            ContactPhone = "+27 83 987 6543",
            IsVerified = true
        };

        context.Organisers.AddRange(organiser1, organiser2);
        context.SaveChanges();

        // ---------- 3. PARTICIPANTS ----------
        var participant1 = new Participant
        {
            UserID = participant1User.UserID,
            SailNumber = "SA-1234",
            BoatName = "Sea Wolf",
            BoatType = "Nacra 17",
            ClubName = "Cape Town Sailing Club"
        };

        var participant2 = new Participant
        {
            UserID = participant2User.UserID,
            SailNumber = "SA-5678",
            BoatName = "Storm Rider",
            BoatType = "Hobie 16",
            ClubName = "Cape Town Sailing Club"
        };

        var participant3 = new Participant
        {
            UserID = participant3User.UserID,
            SailNumber = "SA-9012",
            BoatName = "Wind Dancer",
            BoatType = "Nacra 15",
            ClubName = "Durban Yacht Club"
        };

        context.Participants.AddRange(participant1, participant2, participant3);
        context.SaveChanges();

        // ---------- 4. EVENTS ----------
        var event1 = new Event
        {
            OrganiserID = organiser1.OrganiserID,
            Title = "Cape Town Regatta 2026",
            Description = "Annual catamaran racing event in Table Bay",
            Location = "Table Bay, Cape Town",
            DateStart = new DateTime(2026, 12, 10),
            DateEnd = new DateTime(2026, 12, 12),
            Status = "Open",
            MaxParticipants = 50,
            EventType = "Regatta",
            CreatedAt = DateTime.UtcNow
        };

        var event2 = new Event
        {
            OrganiserID = organiser1.OrganiserID,
            Title = "False Bay Spring Classic",
            Description = "Spring catamaran race around False Bay",
            Location = "False Bay, Cape Town",
            DateStart = new DateTime(2026, 9, 25),
            DateEnd = new DateTime(2026, 9, 26),
            Status = "Draft",
            MaxParticipants = 30,
            EventType = "Race",
            CreatedAt = DateTime.UtcNow
        };

        var event3 = new Event
        {
            OrganiserID = organiser2.OrganiserID,
            Title = "Durban Summer Series",
            Description = "Summer racing series in Durban Harbor",
            Location = "Durban Harbor, Durban",
            DateStart = new DateTime(2027, 1, 15),
            DateEnd = new DateTime(2027, 1, 17),
            Status = "Open",
            MaxParticipants = 40,
            EventType = "Series",
            CreatedAt = DateTime.UtcNow
        };

        context.Events.AddRange(event1, event2, event3);
        context.SaveChanges();

        // ---------- 5. CATEGORIES ----------
        var cat1 = new Category { EventID = event1.EventID, CategoryName = "Open Class", Description = "Open to all catamarans", MinBoatCount = 1, MaxBoatCount = 20 };
        var cat2 = new Category { EventID = event1.EventID, CategoryName = "Women's Class", Description = "For female skippers", MinBoatCount = 1, MaxBoatCount = 15 };
        var cat3 = new Category { EventID = event1.EventID, CategoryName = "Youth Class", Description = "For sailors under 18", MinBoatCount = 1, MaxBoatCount = 10 };
        var cat4 = new Category { EventID = event2.EventID, CategoryName = "Open Class", Description = "Open to all catamarans", MinBoatCount = 1, MaxBoatCount = 15 };
        var cat5 = new Category { EventID = event2.EventID, CategoryName = "Veteran Class", Description = "For sailors over 55", MinBoatCount = 1, MaxBoatCount = 10 };
        var cat6 = new Category { EventID = event3.EventID, CategoryName = "Open Class", Description = "Open to all catamarans", MinBoatCount = 1, MaxBoatCount = 25 };
        var cat7 = new Category { EventID = event3.EventID, CategoryName = "Cruiser Class", Description = "For cruising catamarans", MinBoatCount = 1, MaxBoatCount = 10 };

        context.Categories.AddRange(cat1, cat2, cat3, cat4, cat5, cat6, cat7);
        context.SaveChanges();

        // ---------- 6. ENROLMENTS ----------
        var enrol1 = new EventEnrolment
        {
            ParticipantID = participant1.ParticipantID,
            CategoryID = cat1.CategoryID,
            Status = "Confirmed",
            PaymentStatus = "Paid",
            Notes = "Confirmed entry",
            EnrolmentDate = DateTime.UtcNow
        };

        var enrol2 = new EventEnrolment
        {
            ParticipantID = participant2.ParticipantID,
            CategoryID = cat2.CategoryID,
            Status = "Confirmed",
            PaymentStatus = "Paid",
            Notes = "Looking forward to racing!",
            EnrolmentDate = DateTime.UtcNow
        };

        var enrol3 = new EventEnrolment
        {
            ParticipantID = participant3.ParticipantID,
            CategoryID = cat6.CategoryID,
            Status = "Pending",
            PaymentStatus = "Unpaid",
            Notes = "Awaiting approval",
            EnrolmentDate = DateTime.UtcNow
        };

        context.EventEnrolments.AddRange(enrol1, enrol2, enrol3);
        context.SaveChanges();

        // ---------- 7. RESULTS ----------
        var result1 = new Result
        {
            EnrolmentID = enrol1.EnrolmentID,
            Position = 1,
            FinalTime = new TimeSpan(1, 23, 45),
            Points = 10,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var result2 = new Result
        {
            EnrolmentID = enrol2.EnrolmentID,
            Position = 2,
            FinalTime = new TimeSpan(1, 25, 30),
            Points = 8,
            Penalties = "None",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Results.AddRange(result1, result2);
        context.SaveChanges();
    }
}