namespace LeadScoringApi.Data;

using System.Linq;

public static class DbSeeder
{
    public static void Seed(LeadDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Leads.Any())
        {
            context.Leads.AddRange(MockLeadData.GetLeads());
            context.SaveChanges();
        }
    }
}