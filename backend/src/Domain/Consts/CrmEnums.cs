namespace Crm.Domain.Consts;

public enum EnumLeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Unqualified = 3,
    Converted = 4
}

public enum EnumOpportunityStage
{
    Qualification = 0,
    Proposal = 1,
    Negotiation = 2,
    ClosedWon = 3,
    ClosedLost = 4
}

public enum EnumActivityType
{
    Call = 0,
    Meeting = 1,
    Email = 2,
    Task = 3,
    Note = 4
}
