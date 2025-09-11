using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TempleApi.Configuration;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class JyotishamApiService : IJyotishamApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JyotishamApiSettings _settings;
        private readonly ILogger<JyotishamApiService> _logger;

        public JyotishamApiService(
            HttpClient httpClient, 
            IOptions<JyotishamApiSettings> settings,
            ILogger<JyotishamApiService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.Timeout);
            _httpClient.DefaultRequestHeaders.Add("key", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<JyotishamApiResponse<PanchangDataDto>> GetPanchangAsync(PanchangRequestDto request)
        {
            try
            {
                _logger.LogInformation("Fetching Panchang data for date: {Date}, Lat: {Latitude}, Lng: {Longitude}", 
                    request.Date, request.Latitude, request.Longitude);

                var queryParams = new Dictionary<string, string>
                {
                    ["date"] = request.Date.ToString("yyyy-MM-dd"),
                    ["lat"] = request.Latitude.ToString(),
                    ["lon"] = request.Longitude.ToString(),
                    ["timezone"] = request.Timezone
                };

                if (!string.IsNullOrEmpty(request.Time))
                {
                    queryParams["time"] = request.Time;
                }

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var response = await _httpClient.GetAsync($"/api/panchang?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JyotishamApiResponse<PanchangDataDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result ?? new JyotishamApiResponse<PanchangDataDto>
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }

                _logger.LogWarning("Panchang API request failed with status: {StatusCode}", response.StatusCode);
                return new JyotishamApiResponse<PanchangDataDto>
                {
                    Success = false,
                    Message = $"API request failed with status: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Panchang data");
                return new JyotishamApiResponse<PanchangDataDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching Panchang data"
                };
            }
        }

        public async Task<JyotishamApiResponse<HoroscopeDataDto>> GetDailyHoroscopeAsync(HoroscopeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Fetching daily horoscope for sign: {ZodiacSign}, date: {Date}", 
                    request.ZodiacSign, request.Date);

                // Convert zodiac sign to number (1-12)
                var zodiacNumber = GetZodiacNumber(request.ZodiacSign);
                
                var queryParams = new Dictionary<string, string>
                {
                    ["zodiac"] = zodiacNumber.ToString(),
                    ["lang"] = request.Language
                };

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var response = await _httpClient.GetAsync($"/api/prediction/daily?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response: {Content}", content);
                    
                    // Try to deserialize as direct data first
                    try
                    {
                        var directData = JsonSerializer.Deserialize<HoroscopeDataDto>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        return new JyotishamApiResponse<HoroscopeDataDto>
                        {
                            Success = true,
                            Message = "Success",
                            Data = directData
                        };
                    }
                    catch
                    {
                        // Fallback to wrapped response
                        var wrappedResult = JsonSerializer.Deserialize<JyotishamApiResponse<HoroscopeDataDto>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return wrappedResult ?? new JyotishamApiResponse<HoroscopeDataDto>
                        {
                            Success = false,
                            Message = "Failed to deserialize response"
                        };
                    }
                }

                _logger.LogWarning("Daily horoscope API request failed with status: {StatusCode}", response.StatusCode);
                return new JyotishamApiResponse<HoroscopeDataDto>
                {
                    Success = false,
                    Message = $"API request failed with status: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching daily horoscope");
                return new JyotishamApiResponse<HoroscopeDataDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching daily horoscope"
                };
            }
        }

        public async Task<JyotishamApiResponse<WeeklyHoroscopeDataDto>> GetWeeklyHoroscopeAsync(HoroscopeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Fetching weekly horoscope for sign: {ZodiacSign}, date: {Date}", 
                    request.ZodiacSign, request.Date);

                // Convert zodiac sign to number (1-12)
                var zodiacNumber = GetZodiacNumber(request.ZodiacSign);
                
                var queryParams = new Dictionary<string, string>
                {
                    ["zodiac"] = zodiacNumber.ToString(),
                    ["lang"] = request.Language
                };

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var response = await _httpClient.GetAsync($"/api/prediction/weekly?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response: {Content}", content);
                    
                    // Try to deserialize as direct data first
                    try
                    {
                        var directData = JsonSerializer.Deserialize<WeeklyHoroscopeDataDto>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        return new JyotishamApiResponse<WeeklyHoroscopeDataDto>
                        {
                            Success = true,
                            Message = "Success",
                            Data = directData
                        };
                    }
                    catch
                    {
                        // Fallback to wrapped response
                        var wrappedResult = JsonSerializer.Deserialize<JyotishamApiResponse<WeeklyHoroscopeDataDto>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return wrappedResult ?? new JyotishamApiResponse<WeeklyHoroscopeDataDto>
                        {
                            Success = false,
                            Message = "Failed to deserialize response"
                        };
                    }
                }

                _logger.LogWarning("Weekly horoscope API request failed with status: {StatusCode}", response.StatusCode);
                return new JyotishamApiResponse<WeeklyHoroscopeDataDto>
                {
                    Success = false,
                    Message = $"API request failed with status: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weekly horoscope");
                return new JyotishamApiResponse<WeeklyHoroscopeDataDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching weekly horoscope"
                };
            }
        }

        public async Task<JyotishamApiResponse<MonthlyHoroscopeDataDto>> GetMonthlyHoroscopeAsync(HoroscopeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Fetching monthly horoscope for sign: {ZodiacSign}, date: {Date}", 
                    request.ZodiacSign, request.Date);

                // Convert zodiac sign to number (1-12)
                var zodiacNumber = GetZodiacNumber(request.ZodiacSign);
                
                var queryParams = new Dictionary<string, string>
                {
                    ["zodiac"] = zodiacNumber.ToString(),
                    ["lang"] = request.Language
                };

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var response = await _httpClient.GetAsync($"/api/prediction/monthly?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response: {Content}", content);
                    
                    // Try to deserialize as direct data first
                    try
                    {
                        var directData = JsonSerializer.Deserialize<MonthlyHoroscopeDataDto>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        return new JyotishamApiResponse<MonthlyHoroscopeDataDto>
                        {
                            Success = true,
                            Message = "Success",
                            Data = directData
                        };
                    }
                    catch
                    {
                        // Fallback to wrapped response
                        var wrappedResult = JsonSerializer.Deserialize<JyotishamApiResponse<MonthlyHoroscopeDataDto>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return wrappedResult ?? new JyotishamApiResponse<MonthlyHoroscopeDataDto>
                        {
                            Success = false,
                            Message = "Failed to deserialize response"
                        };
                    }
                }

                _logger.LogWarning("Monthly horoscope API request failed with status: {StatusCode}", response.StatusCode);
                return new JyotishamApiResponse<MonthlyHoroscopeDataDto>
                {
                    Success = false,
                    Message = $"API request failed with status: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly horoscope");
                return new JyotishamApiResponse<MonthlyHoroscopeDataDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching monthly horoscope"
                };
            }
        }

        private int GetZodiacNumber(string zodiacSign)
        {
            return zodiacSign.ToLower() switch
            {
                "aries" => 1,
                "taurus" => 2,
                "gemini" => 3,
                "cancer" => 4,
                "leo" => 5,
                "virgo" => 6,
                "libra" => 7,
                "scorpio" => 8,
                "sagittarius" => 9,
                "capricorn" => 10,
                "aquarius" => 11,
                "pisces" => 12,
                _ => 1 // Default to Aries
            };
        }
    }
}

