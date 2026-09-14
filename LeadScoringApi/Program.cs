using LeadScoringApi.Data;
using LeadScoringApi.Services;
using LeadScoringApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<LeadScoringService>();
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapGet("/api/leads", (LeadScoringService scoringService) =>
{
    var leads = MockLeadData.GetLeads();
    var scoredLeads = leads.Select(lead => scoringService.ScoreLead(lead)).ToList();
    return Results.Ok(scoredLeads);
})
.WithName("GetScoredLeads");

app.MapGet("/api/leads/{id}", (int id, LeadScoringService scoringService) =>
{
    var lead = MockLeadData.GetLeads().FirstOrDefault(l => l.Id == id);
    if (lead == null) return Results.NotFound();
    
    var scoredLead = scoringService.ScoreLead(lead);
    return Results.Ok(scoredLead);
})
.WithName("GetScoredLeadById");

app.MapGet("/api/leads/tier/{tier}", (string tier, LeadScoringService scoringService) =>
{
    var leads = MockLeadData.GetLeads();
    var scoredLeads = leads
        .Select(lead => scoringService.ScoreLead(lead))
        .Where(lead => lead.Tier.Equals(tier, StringComparison.OrdinalIgnoreCase))
        .ToList();
    return Results.Ok(scoredLeads);
})
.WithName("GetLeadsByTier");

app.Run();
