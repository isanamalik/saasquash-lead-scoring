namespace LeadScoringApi.Services;

using LeadScoringApi.Models;

public class LeadScoringService
{
    private readonly List<string> _targetTechStack = new() { "React", ".NET", "Azure", "AWS", "Node.js", "Python" };
    private readonly List<string> _targetLocations = new() { "United States", "Canada", "United Kingdom", "Germany" };

    public ScoredLead ScoreLead(Lead lead)
    {
        var scoredLead = new ScoredLead
        {
            Id = lead.Id,
            CompanyName = lead.CompanyName,
            Industry = lead.Industry,
            EmployeeCount = lead.EmployeeCount,
            Location = lead.Location,
            FundingStage = lead.FundingStage,
            Revenue = lead.Revenue,
            TechStack = lead.TechStack,
            ContactEmail = lead.ContactEmail,
            Website = lead.Website
        };

        var breakdown = new Dictionary<string, int>();

        breakdown["CompanySize"] = ScoreCompanySize(lead.EmployeeCount);
        breakdown["FundingStage"] = ScoreFundingStage(lead.FundingStage);
        breakdown["Revenue"] = ScoreRevenue(lead.Revenue);
        breakdown["TechStackMatch"] = ScoreTechStack(lead.TechStack);
        breakdown["Location"] = ScoreLocation(lead.Location);

        scoredLead.Score = breakdown.Values.Sum();
        scoredLead.ScoreBreakdown = breakdown;
        scoredLead.Tier = DetermineTier(scoredLead.Score);

        return scoredLead;
    }

    private int ScoreCompanySize(int employeeCount)
    {
        return employeeCount switch
        {
            >= 500 => 25,
            >= 100 => 20,
            >= 50 => 15,
            >= 10 => 10,
            _ => 5
        };
    }

    private int ScoreFundingStage(string stage)
    {
        return stage.ToLower() switch
        {
            "series c" or "series d" or "series e" => 25,
            "series b" => 20,
            "series a" => 15,
            "seed" => 10,
            _ => 5
        };
    }

    private int ScoreRevenue(decimal revenue)
    {
        return revenue switch
        {
            >= 50_000_000 => 20,
            >= 10_000_000 => 15,
            >= 1_000_000 => 10,
            >= 100_000 => 5,
            _ => 0
        };
    }

    private int ScoreTechStack(List<string> techStack)
    {
        var matchCount = techStack.Count(tech => _targetTechStack.Contains(tech, StringComparer.OrdinalIgnoreCase));
        return matchCount * 5;
    }

    private int ScoreLocation(string location)
    {
        return _targetLocations.Any(loc => location.Contains(loc, StringComparison.OrdinalIgnoreCase)) ? 10 : 0;
    }

    private string DetermineTier(int score)
    {
        return score switch
        {
            >= 70 => "A",
            >= 50 => "B",
            >= 30 => "C",
            _ => "D"
        };
    }
}
