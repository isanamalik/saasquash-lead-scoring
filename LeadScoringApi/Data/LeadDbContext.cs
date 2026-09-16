namespace LeadScoringApi.Data;

using System.Text.Json;
using System.Linq;
using LeadScoringApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public class LeadDbContext : DbContext
{
    public LeadDbContext(DbContextOptions<LeadDbContext> options) : base(options) { }

    public DbSet<Lead> Leads => Set<Lead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite has no native array/list column type, so TechStack is stored
        // as a JSON string and converted back to List<string> on read.
        var techStackComparer = new ValueComparer<List<string>>(
            (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
            v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
            v => v.ToList());

        modelBuilder.Entity<Lead>()
            .Property(l => l.TechStack)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
            .Metadata.SetValueComparer(techStackComparer);
    }
}