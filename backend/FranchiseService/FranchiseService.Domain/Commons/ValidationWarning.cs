namespace FranchiseService.Domain.Commons
{
    public class ValidationWarning
    {
        public string Code { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Severity { get; set; } = null!; // "BLOCKING" | "WARNING"
    }
}
