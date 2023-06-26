using System.Net.Http.Headers;
using BotGeoGuessr.GeoGuessr.Models;
using BotGeoGuessr.Validators;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace BotGeoGuessr.GeoGuessr.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public HttpService(HttpClient httpClient, ILogger logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<int> GetPresentUser(string ncfaCookie, string buildId)
        {
            _logger.Information("{Class}.{Function}", nameof(HttpService), nameof(GetPresentUser));
            
            if (string.IsNullOrWhiteSpace(ncfaCookie) || string.IsNullOrWhiteSpace(buildId))
                throw new ArgumentException("wrong arguments");
            
            _httpClient.DefaultRequestHeaders.Clear();
            BuildHttpClient(ncfaCookie);
            
            dynamic gameInfos = await GetInfos(buildId);
            
            return CountUserPresent(gameInfos.pageProps.initialMemberInfo.members);
        }

        public async Task UpdateSettings(string ncfaCookie, GameSettings settings)
        {
            _logger.Information("{Class}.{Function}", nameof(HttpService), nameof(UpdateSettings));

            IValidator<GameSettings> validator = new GameSettingsValidator();
            ValidationResult result = await validator.ValidateAsync(settings);
            if (!result.IsValid || string.IsNullOrWhiteSpace(ncfaCookie))
                throw new ArgumentException("wrong arguments");
            
            _httpClient.DefaultRequestHeaders.Clear();
            BuildHttpClient(ncfaCookie);
            await SendUpdateSettings(settings);
        }

        public async Task<List<string>> GetMaps(string ncfaCookie)
        {
            const string MAPS_URL = "https://www.geoguessr.com/api/v3/social/maps/browse/popular/official?count=25&page=0";
            _logger.Information("{Class}.{Function}", nameof(HttpService), nameof(GetMaps));
            
            if (string.IsNullOrWhiteSpace(ncfaCookie))
                throw new ArgumentException("wrong arguments");
            
            _httpClient.DefaultRequestHeaders.Clear();
            BuildHttpClient(ncfaCookie);
            string response = await SendAsync(MAPS_URL, HttpMethod.Get);
            return FormatMapsResponse(response);
        }

        private static List<string> FormatMapsResponse(string strMaps)
        {
            List<string> responseMaps = new();
            dynamic dynamicMaps = JsonConvert.DeserializeObject<dynamic>(strMaps)!;
            foreach (dynamic map in dynamicMaps)
                responseMaps.Add((string)map.slug);
            
            return responseMaps;
        }

        private async Task SendUpdateSettings(GameSettings settings)
        {
            const string UPDATE_SETTINGS_URL = "https://www.geoguessr.com/api/v4/parties/v2/game-settings";
            await SendAsync(UPDATE_SETTINGS_URL, HttpMethod.Put, JsonConvert.SerializeObject(new
            {
                forbidMoving = false,
                forbidRotating = false,
                forbidZooming = false,
                mapSlug = settings.Map,
                roundTime = settings.Duration,
            }));
        }

        private static int CountUserPresent(dynamic members)
        {
            JArray result = JArray.FromObject(((JArray)members).Where(m => (bool)(m["isPresent"] ?? false)));
            return result.Count;
        }

        private void BuildHttpClient(string ncfaCookie)
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.5");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "json");
            _httpClient.DefaultRequestHeaders.Add("x-nextjs-data", "1");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"{ncfaCookie}");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            _httpClient.DefaultRequestHeaders.Add("TE", "trailers");
        }

        private async Task<dynamic> GetInfos(string buildId)
        {
            _logger.Information("{Class}.{Function} : Getting games infos", nameof(HttpService), nameof(GetInfos));
            string url = $"https://www.geoguessr.com/_next/data/{buildId}/en/party.json";

            string strResponse = await SendAsync(url, HttpMethod.Post);
            return JsonConvert.DeserializeObject<dynamic>(strResponse)!;
        }

        private async Task<string> SendAsync(string url, HttpMethod method, string? content = null)
        {
            HttpRequestMessage request = new(method, url);
            if (content != null)
            {
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            }

            _logger.Information("{Class}.{Function} : request send to {Url}", nameof(HttpService), nameof(SendAsync), url);
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            _logger.Information("{Class}.{Function} : Status code from {Url} : {StatusCode}", nameof(HttpService), nameof(SendAsync), url, response.StatusCode);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(response.StatusCode.ToString());

            return await response.Content.ReadAsStringAsync();
        }
    } 
}
