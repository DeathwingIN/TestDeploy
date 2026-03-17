using System.Threading.Tasks;

namespace MicroCloud.Licensing.Function.Services
{
    public interface ITokenValidatorService
    {
        Task<bool> ValidateAppSourceTokenAsync(string token);
    }
}