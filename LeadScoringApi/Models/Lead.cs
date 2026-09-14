namespace LeadScoringApi.Models;

public class Lead
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public string Location { get; set; } = string.Empty;
    public string FundingStage { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public List<string> TechStack { get; set; } = new();
    public string ContactEmail { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
}

public class ScoredLead : Lead
{
    public int Score { get; set; }
    public string Tier { get; set; } = string.Empty;
    public Dictionary<string, int> ScoreBreakdown { get; set; } = new();
}
