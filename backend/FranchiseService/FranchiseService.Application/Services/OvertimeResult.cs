namespace FranchiseService.Application.Services;

/// <summary>
/// Ket qua tra ve sau khi tinh toan OT cho mot ca lam viec.
/// </summary>
public class OvertimeResult
{
    /// <summary>
    /// So phut lam viec thuong (trong nguong cho phep).
    /// </summary>
    public int RegularMinutes { get; set; }

    /// <summary>
    /// So phut tang ca trong ngay (vuot nguong DailyOtThresholdHours).
    /// </summary>
    public int DailyOtMinutes { get; set; }

    /// <summary>
    /// So phut tang ca trong tuan (vuot nguong WeeklyOtThresholdHours).
    /// </summary>
    public int WeeklyOtMinutes { get; set; }
}
