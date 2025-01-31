using Durable.Service.Models;

namespace Durable.Services.Interfaces
{
    public interface IEnvImpactReportService
    {
        Task<string> GetReportAsync(ReportBaseModel model);

        Task<HttpResponseMessage> Execute(string url, object body = null, string authToken = null);
    }
}
