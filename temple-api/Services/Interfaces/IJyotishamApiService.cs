using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IJyotishamApiService
    {
        Task<JyotishamApiResponse<PanchangDataDto>> GetPanchangAsync(PanchangRequestDto request);
        Task<JyotishamApiResponse<HoroscopeDataDto>> GetDailyHoroscopeAsync(HoroscopeRequestDto request);
        Task<JyotishamApiResponse<WeeklyHoroscopeDataDto>> GetWeeklyHoroscopeAsync(HoroscopeRequestDto request);
        Task<JyotishamApiResponse<MonthlyHoroscopeDataDto>> GetMonthlyHoroscopeAsync(HoroscopeRequestDto request);
    }
}

