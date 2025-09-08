namespace TempleApi.Models.DTOs
{
    // Request DTOs
    public class PanchangRequestDto
    {
        public DateTime Date { get; set; }
        public string? Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Timezone { get; set; } = "Asia/Kolkata";
    }

    public class HoroscopeRequestDto
    {
        public DateTime Date { get; set; }
        public string? Time { get; set; }
        public string ZodiacSign { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
    }

    // Response DTOs
    public class JyotishamApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class PanchangDataDto
    {
        public DateTime Date { get; set; }
        public string Tithi { get; set; } = string.Empty;
        public string Nakshatra { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public string Sunrise { get; set; } = string.Empty;
        public string Sunset { get; set; } = string.Empty;
        public string Moonrise { get; set; } = string.Empty;
        public string Moonset { get; set; } = string.Empty;
        public List<AuspiciousTimingDto> AuspiciousTimings { get; set; } = new();
        public List<InauspiciousTimingDto> InauspiciousTimings { get; set; } = new();
    }

    public class AuspiciousTimingDto
    {
        public string Name { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class InauspiciousTimingDto
    {
        public string Name { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class HoroscopeDataDto
    {
        public DateTime Date { get; set; }
        public string ZodiacSign { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Prediction { get; set; } = string.Empty;
        public string Love { get; set; } = string.Empty;
        public string Career { get; set; } = string.Empty;
        public string Health { get; set; } = string.Empty;
        public string Finance { get; set; } = string.Empty;
        public string LuckyNumber { get; set; } = string.Empty;
        public string LuckyColor { get; set; } = string.Empty;
    }

    public class WeeklyHoroscopeDataDto : HoroscopeDataDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
    }

    public class MonthlyHoroscopeDataDto : HoroscopeDataDto
    {
        public DateTime MonthStart { get; set; }
        public DateTime MonthEnd { get; set; }
        public string MonthName { get; set; } = string.Empty;
    }
}

