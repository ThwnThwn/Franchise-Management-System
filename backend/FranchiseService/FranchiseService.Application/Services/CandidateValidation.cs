namespace FranchiseService.Application.Services;

/// <summary>
/// Ket qua validate cho GetAvailableStaffs.
/// Tra ve truc tiep cho FE hien giao dien canh bao.
/// </summary>
public class CandidateValidation
{
    public bool IsConflict { get; set; }
    public bool IsRestTimeInvalid { get; set; }
    public double RestHours { get; set; }
    public bool IsOverLimit { get; set; }
    public double WeeklyHours { get; set; }
}
