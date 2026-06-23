using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TourPlanner.Business.Interfaces;
using TourPlanner.Models.Enums;

namespace TourPlanner.Business.Services;

public class OpenRouteServiceClient : IRouteService
{
    private readonly HttpClient httpClient;
    private readonly OpenRouteServiceSettings settings;

    public OpenRouteServiceClient(HttpClient httpClient, IOptions<OpenRouteServiceSettings> options)
    {
        this.httpClient = httpClient;
        settings = options.Value;
    }

    public async Task<RouteResult> GetRouteAsync(string from, string to, TransportType transportType)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenRouteService API key is not configured. Set it with: dotnet user-secrets set \"OpenRouteService:ApiKey\" \"<your-key>\" --project TourPlanner.Api");
        }

        (double Longitude, double Latitude) fromCoordinates = await GeocodeAsync(from);
        (double Longitude, double Latitude) toCoordinates = await GeocodeAsync(to);

        string profile = MapTransportTypeToProfile(transportType);

        string directionsUrl =
            $"{settings.BaseUrl}/v2/directions/{profile}" +
            $"?api_key={Uri.EscapeDataString(settings.ApiKey)}" +
            $"&start={FormatCoordinate(fromCoordinates.Longitude)},{FormatCoordinate(fromCoordinates.Latitude)}" +
            $"&end={FormatCoordinate(toCoordinates.Longitude)},{FormatCoordinate(toCoordinates.Latitude)}";

        HttpResponseMessage directionsHttpResponse = await httpClient.GetAsync(directionsUrl);
        if (!directionsHttpResponse.IsSuccessStatusCode)
        {
            if (directionsHttpResponse.StatusCode is System.Net.HttpStatusCode.BadRequest or System.Net.HttpStatusCode.NotFound)
            {
                throw new ArgumentException($"No route could be found between '{from}' and '{to}'.");
            }

            throw new InvalidOperationException(
                $"OpenRouteService directions request failed with status {(int)directionsHttpResponse.StatusCode}.");
        }

        DirectionsResponse? directionsResponse = await directionsHttpResponse.Content.ReadFromJsonAsync<DirectionsResponse>();

        RouteSummary? summary = directionsResponse?.Features?.FirstOrDefault()?.Properties?.Summary;
        if (summary is null)
        {
            throw new ArgumentException($"No route could be found between '{from}' and '{to}'.");
        }

        TimeSpan duration = TimeSpan.FromSeconds(summary.Duration);
        string durationText = duration.Days > 0
            ? $"{duration.Days}d {duration:hh\\:mm\\:ss}"
            : duration.ToString(@"hh\:mm\:ss");

        return new RouteResult
        {
            DistanceMeters = summary.Distance,
            Duration = duration,
            RouteInformation = $"Distance: {summary.Distance / 1000:F2} km, Duration: {durationText}"
        };
    }

    private async Task<(double Longitude, double Latitude)> GeocodeAsync(string address)
    {
        string geocodeUrl =
            $"{settings.BaseUrl}/geocode/search" +
            $"?api_key={Uri.EscapeDataString(settings.ApiKey)}" +
            $"&text={Uri.EscapeDataString(address)}" +
            "&size=1";

        HttpResponseMessage geocodeHttpResponse = await httpClient.GetAsync(geocodeUrl);
        if (!geocodeHttpResponse.IsSuccessStatusCode)
        {
            if (geocodeHttpResponse.StatusCode is System.Net.HttpStatusCode.BadRequest or System.Net.HttpStatusCode.NotFound)
            {
                throw new ArgumentException($"Location '{address}' could not be found.");
            }

            throw new InvalidOperationException(
                $"OpenRouteService geocoding request failed with status {(int)geocodeHttpResponse.StatusCode}.");
        }

        GeocodeResponse? geocodeResponse = await geocodeHttpResponse.Content.ReadFromJsonAsync<GeocodeResponse>();

        double[]? coordinates = geocodeResponse?.Features?.FirstOrDefault()?.Geometry?.Coordinates;
        if (coordinates is null || coordinates.Length < 2)
        {
            throw new ArgumentException($"Location '{address}' could not be found.");
        }

        return (coordinates[0], coordinates[1]);
    }

    private static string FormatCoordinate(double value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string MapTransportTypeToProfile(TransportType transportType)
    {
        return transportType switch
        {
            TransportType.Bike => "cycling-regular",
            TransportType.Hike => "foot-hiking",
            TransportType.Running => "foot-walking",
            TransportType.Vacation => "driving-car",
            _ => "driving-car"
        };
    }

    private class GeocodeResponse
    {
        [JsonPropertyName("features")]
        public List<GeocodeFeature>? Features { get; set; }
    }

    private class GeocodeFeature
    {
        [JsonPropertyName("geometry")]
        public GeocodeGeometry? Geometry { get; set; }
    }

    private class GeocodeGeometry
    {
        [JsonPropertyName("coordinates")]
        public double[]? Coordinates { get; set; }
    }

    private class DirectionsResponse
    {
        [JsonPropertyName("features")]
        public List<DirectionsFeature>? Features { get; set; }
    }

    private class DirectionsFeature
    {
        [JsonPropertyName("properties")]
        public DirectionsProperties? Properties { get; set; }
    }

    private class DirectionsProperties
    {
        [JsonPropertyName("summary")]
        public RouteSummary? Summary { get; set; }
    }

    private class RouteSummary
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }
    }
}
