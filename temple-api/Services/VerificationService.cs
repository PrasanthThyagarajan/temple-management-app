using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly TempleDbContext _context;
        private readonly ISaleService _saleService;

        public VerificationService(TempleDbContext context, ISaleService saleService)
        {
            _context = context;
            _saleService = saleService;
        }

        public async Task<bool> VerifyUserAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationCode == code && !u.IsVerified);
            if (user == null) return false;
            user.IsVerified = true;
            user.IsActive = true;
            user.VerificationCode = string.Empty;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> VerifySaleBookingAsync(string bookingToken) => Task.FromResult(false);
    }
}


