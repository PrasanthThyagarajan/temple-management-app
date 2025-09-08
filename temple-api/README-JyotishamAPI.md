# Jyotisham Astro API Integration

This document explains how to configure and use the Jyotisham Astro API integration in the Temple Management System.

## Configuration

### 1. API Key Setup

1. Visit [Jyotisham Astro API](https://www.jyotishamastroapi.com) and register for an account
2. Obtain your API key from the dashboard
3. Update the `appsettings.json` file with your API key:

```json
{
  "JyotishamApi": {
    "BaseUrl": "https://api.jyotishamastroapi.com",
    "ApiKey": "YOUR_ACTUAL_API_KEY_HERE",
    "Timeout": 30
  }
}
```

### 2. Environment Variables (Optional)

For production environments, you can use environment variables instead of hardcoding the API key:

```bash
# Set environment variable
export JyotishamApi__ApiKey="your_api_key_here"
```

## Available Services

### 1. Daily Panchang
- **Endpoint**: `POST /api/panchang`
- **Purpose**: Get Hindu calendar information including Tithi, Nakshatra, auspicious timings
- **Required Parameters**:
  - `date`: Date for which to get Panchang
  - `latitude`: Location latitude
  - `longitude`: Location longitude
  - `timezone`: Timezone (default: Asia/Kolkata)

### 2. Daily Horoscope
- **Endpoint**: `POST /api/horoscope/daily`
- **Purpose**: Get daily horoscope predictions
- **Required Parameters**:
  - `date`: Date for horoscope
  - `zodiacSign`: Zodiac sign (aries, taurus, etc.)
  - `language`: Language preference (en, hi, ta, ml, te)

### 3. Weekly Horoscope
- **Endpoint**: `POST /api/horoscope/weekly`
- **Purpose**: Get weekly horoscope predictions
- **Required Parameters**: Same as daily horoscope

### 4. Monthly Horoscope
- **Endpoint**: `POST /api/horoscope/monthly`
- **Purpose**: Get monthly horoscope predictions
- **Required Parameters**: Same as daily horoscope

## Frontend Usage

The frontend provides a user-friendly interface at `/astrology` with:

1. **Service Selection Dropdown**: Choose between Panchang, Daily/Weekly/Monthly Horoscope
2. **Input Forms**: Dynamic forms based on selected service
3. **Results Display**: Formatted display of API responses
4. **Error Handling**: User-friendly error messages

## API Response Format

All endpoints return responses in the following format:

```json
{
  "success": true,
  "message": "Success message",
  "data": {
    // Service-specific data
  }
}
```

## Error Handling

The service includes comprehensive error handling:
- API timeout handling
- Network error handling
- Invalid response handling
- User-friendly error messages

## Testing

You can test the API endpoints using:

1. **Swagger UI**: Available at `http://localhost:5051/swagger` when running in development
2. **Frontend Interface**: Navigate to `/astrology` in the web application
3. **Postman**: Import the API collection and test individual endpoints

## Pricing

Refer to the [Jyotisham Astro API pricing page](https://www.jyotishamastroapi.com/pricing) for current pricing information.

## Support

For API-related issues:
- Check the [Jyotisham API documentation](https://www.jyotishamastroapi.com)
- Contact Jyotisham support: support@jyotishamastroapi.com

For integration issues:
- Check the application logs
- Verify API key configuration
- Ensure network connectivity
