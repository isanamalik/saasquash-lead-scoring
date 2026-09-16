using LeadScoringApi.Data;
using LeadScoringApi.Services;
using LeadScoringApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<LeadScoringService>();

builder.Services.AddDbContext<LeadDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Create the SQLite file and seed it from MockLeadData on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeadDbContext>();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapGet("/api/leads", async (LeadDbContext db, LeadScoringService scoringService) =>
{
    var leads = await db.Leads.ToListAsync();
    var scoredLeads = leads.Select(lead => scoringService.ScoreLead(lead)).ToList();
    return Results.Ok(scoredLeads);
})
.WithName("GetScoredLeads");

app.MapGet("/api/leads/{id}", async (int id, LeadDbContext db, LeadScoringService scoringService) =>
{
    var lead = await db.Leads.FindAsync(id);
    if (lead == null) return Results.NotFound();

    var scoredLead = scoringService.ScoreLead(lead);
    return Results.Ok(scoredLead);
})
.WithName("GetScoredLeadById");

app.MapGet("/api/leads/tier/{tier}", async (string tier, LeadDbContext db, LeadScoringService scoringService) =>
{
    var leads = await db.Leads.ToListAsync();
    var scoredLeads = leads
        .Select(lead => scoringService.ScoreLead(lead))
        .Where(lead => lead.Tier.Equals(tier, StringComparison.OrdinalIgnoreCase))
        .ToList();
    return Results.Ok(scoredLeads);
})
.WithName("GetLeadsByTier");

app.Run();