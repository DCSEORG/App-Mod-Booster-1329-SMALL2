namespace ExpenseApp.Models;

public class Expense
{
    public int      ExpenseId      { get; set; }
    public int      UserId         { get; set; }
    public string   UserName       { get; set; } = string.Empty;
    public int      CategoryId     { get; set; }
    public string   CategoryName   { get; set; } = string.Empty;
    public int      StatusId       { get; set; }
    public string   StatusName     { get; set; } = string.Empty;
    public int      AmountMinor    { get; set; }   // pence / minor currency units
    public decimal  AmountGBP      { get; set; }
    public string   Currency       { get; set; } = "GBP";
    public DateTime ExpenseDate    { get; set; }
    public string?  Description    { get; set; }
    public string?  ReceiptFile    { get; set; }
    public DateTime? SubmittedAt   { get; set; }
    public int?     ReviewedBy     { get; set; }
    public string?  ReviewedByName { get; set; }
    public DateTime? ReviewedAt    { get; set; }
    public DateTime CreatedAt      { get; set; }
}

public class CreateExpenseRequest
{
    public int      UserId      { get; set; }
    public int      CategoryId  { get; set; }
    /// <summary>Amount in minor units (pence). E.g. £12.34 = 1234</summary>
    public int      AmountMinor { get; set; }
    public string   Currency    { get; set; } = "GBP";
    public DateTime ExpenseDate { get; set; }
    public string?  Description { get; set; }
    public string?  ReceiptFile { get; set; }
    /// <summary>When true the expense is immediately submitted for approval.</summary>
    public bool     Submit      { get; set; } = false;
}

public class UpdateExpenseRequest
{
    public int      CategoryId  { get; set; }
    public int      AmountMinor { get; set; }
    public string   Currency    { get; set; } = "GBP";
    public DateTime ExpenseDate { get; set; }
    public string?  Description { get; set; }
    public string?  ReceiptFile { get; set; }
}

public class ReviewExpenseRequest
{
    /// <summary>UserId of the manager performing the review.</summary>
    public int ReviewedBy { get; set; }
}
