using System.Threading.Tasks;

namespace TempleApi.Services.Interfaces
{
    public interface IVerificationService
    {
        Task<bool> VerifyUserAsync(string code);
        Task<bool> VerifySaleBookingAsync(string bookingToken);
    }
}


