// Data/RaceDayDbContext.cs
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Organiser> Organisers { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<EventEnrolment> EventEnrolments { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<Document> Documents { get; set; }

    // ⬇️ THIS IS THE METHOD YOU REPLACE ⬇️
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User - Organiser (1:1)
        modelBuilder.Entity<Organiser>()
            .HasOne(o => o.User)
            .WithOne(u => u.OrganiserProfile)
            .HasForeignKey<Organiser>(o => o.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // User - Participant (1:1)
        modelBuilder.Entity<Participant>()
            .HasOne(p => p.User)
            .WithOne(u => u.ParticipantProfile)
            .HasForeignKey<Participant>(p => p.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // Organiser - Events (1:M)
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserID);

        // Event - Categories (1:M)
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        // Participant - Enrolments (1:M)  <-- FIX: prevent cascade
        modelBuilder.Entity<EventEnrolment>()
            .HasOne(en => en.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(en => en.ParticipantID)
            .OnDelete(DeleteBehavior.NoAction);

        // Category - Enrolments (1:M)  <-- FIX: prevent cascade
        modelBuilder.Entity<EventEnrolment>()
            .HasOne(en => en.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(en => en.CategoryID)
            .OnDelete(DeleteBehavior.NoAction);

        // Enrolment - Result (1:1)
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(en => en.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on enrolment
        modelBuilder.Entity<EventEnrolment>()
            .HasIndex(en => new { en.ParticipantID, en.CategoryID })
            .IsUnique();

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}