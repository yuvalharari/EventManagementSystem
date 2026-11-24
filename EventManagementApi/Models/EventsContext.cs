using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Models;

public partial class EventsContext : DbContext
{
    public EventsContext(DbContextOptions<EventsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventUser> EventUsers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<EventUser>(entity =>
        {
            entity.HasKey(e => new { e.EventRef, e.UserRef });

            entity.ToTable("EventUser");

            entity.Property(e => e.Creation)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.EventRefNavigation).WithMany(p => p.EventUsers)
                .HasForeignKey(d => d.EventRef)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventUser_Events");

            entity.HasOne(d => d.UserRefNavigation).WithMany(p => p.EventUsers)
                .HasForeignKey(d => d.UserRef)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventUser_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
