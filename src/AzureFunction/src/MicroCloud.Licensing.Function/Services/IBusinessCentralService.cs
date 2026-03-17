using System.Threading.Tasks;

namespace MicroCloud.Licensing.Function.Services
{
    public interface IBusinessCentralService
    {
        Task<bool> ForwardWebhookAsync(string payload);
        Task<bool> CheckLicenseAsync(string requestTenantId);
    }
}