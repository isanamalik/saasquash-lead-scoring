namespace LeadScoringApi.Services;

using LeadScoringApi.Models;

public class LeadScoringService
{
    // Practical small-business operating tools — a business using these has
    // real, functioning operations worth acquiring (not a developer-stack signal).
    private readonly List<string> _operationalTools = new()
    {
        "QuickBooks", "ServiceTitan", "Salesforce", "Xero", "Shopify",
        "Square", "Excel", "Google Sheets", "HubSpot"
    };

    private readonly List<string> _targetLocations = new()
    {
        "United States", "Canada", "United Kingdom", "Germany"
    };

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

        var breakdown = new Dictionary<string, int>
        {
            ["CompanySize"] = ScoreCompanySize(lead.EmployeeCount),
            ["OwnershipStructure"] = ScoreFundingStage(lead.FundingStage),
            ["Revenue"] = ScoreRevenue(lead.Revenue),
            ["OperationalMaturity"] = ScoreOperationalTooling(lead.TechStack),
            ["Location"] = ScoreLocation(lead.Location)
        };

        scoredLead.Score = breakdown.Values.Sum();
        scoredLead.ScoreBreakdown = breakdown;
        scoredLead.Tier = DetermineTier(scoredLead.Score);

        return scoredLead;
    }

    // Search funds target small, owner-operated businesses — NOT enterprise
    // scale. Smaller headcount = higher score. Max 25.
    private int ScoreCompanySize(int employeeCount)
    {
        return employeeCount switch
        {
            >= 10 and <= 150 => 25,   // sweet spot: real business, still acquirable
            > 150 and <= 300 => 12,   // larger, harder to acquire/integrate
            < 10 => 15,               // very small — viable but thin
            _ => 3                    // 300+ — enterprise scale, not a search-fund target
        };
    }

    // Bootstrapped/self-funded/family-owned businesses are actually for sale.
    // Heavy VC funding (Series B+) usually means the company isn't acquirable
    // this way and is optimizing for growth, not a sale. Max 25.
    private int ScoreFundingStage(string stage)
    {
        return stage.ToLower() switch
        {
            "bootstrapped" or "family-owned" or "self-funded" => 25,
            "seed" => 15,
            "series a" => 8,
            "series b" => 3,
            _ => 0 // series c/d/e and beyond — not a realistic target
        };
    }

    // Classic lower-middle-market range: big enough to be a real, profitable
    // business; small enough to be within reach of a search-fund acquisition. Max 20.
    private int ScoreRevenue(decimal revenue)
    {
        return revenue switch
        {
            >= 1_000_000 and <= 20_000_000 => 20,
            > 20_000_000 and <= 50_000_000 => 10,
            >= 300_000 and < 1_000_000 => 8,
            > 50_000_000 => 3,
            _ => 0
        };
    }

    private int ScoreOperationalTooling(List<string> techStack)
    {
        var matchCount = techStack.Count(tool =>
            _operationalTools.Contains(tool, StringComparer.OrdinalIgnoreCase));
        return Math.Min(matchCount * 5, 15);
    }

    // Target geographies where search-fund/ETA activity and legal frameworks
    // are well-established. Max 15.
    private int ScoreLocation(string location)
    {
        return _targetLocations.Any(loc =>
            location.Contains(loc, StringComparison.OrdinalIgnoreCase)) ? 15 : 0;
    }

    private string DetermineTier(int score)
    {
        return score switch
        {
            >= 80 => "A",
            >= 60 => "B",
            >= 40 => "C",
            _ => "D"
        };
    }
}