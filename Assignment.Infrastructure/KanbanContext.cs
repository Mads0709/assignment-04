using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Assignment.Infrastructure;

public class KanbanContext : DbContext
{   
    public KanbanContext(DbContextOptions<KanbanContext> options) : base(options) { }

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) //this confirgure the properties in the classes
    {
        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired(); //makes a max length of 100

            entity.HasOne(e => e.AssignedTo);

            entity.Property(e => e.Description);

            entity.Property(e => e.State).HasConversion<string>().IsRequired();

            entity.Property(e => e.Created);

            entity.Property(e => e.StateUpdated);
        });


        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired(); //makes a max length of 100

            entity.Property(e => e.Email).HasMaxLength(100).IsRequired(); //makes a max length of 100

            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50); //makes a max length of 50

            entity.HasIndex(e => e.Name).IsUnique(); //makes it Unique
        });
    }
}