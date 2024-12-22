namespace Durable.Services.Interfaces
{
    public interface IEnvImpactReportService
    {
        Task<string> GetReportAsync(
            string name,
            string? region,
            int percentage,
            string reportName);
    }
}
