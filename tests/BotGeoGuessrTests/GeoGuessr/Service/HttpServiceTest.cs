using System.Net;
using BotGeoGuessr.GeoGuessr.Models;
using BotGeoGuessr.GeoGuessr.Services;
using Discord;
using Moq;
using Moq.Protected;
using Serilog;

namespace BotGeoGuessrTests.GeoGuessr.Service
{
    public class HttpServiceTest
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new();
        private readonly Mock<ILogger> _mockLogger = new();

        private const string NCFA = "ncfa";
        private const string BUILDID = "buildid";
        private const int NB_USERS = 2;
        private static readonly HttpResponseMessage HttprequestmsgValid = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{'pageProps':{'initialMemberInfo':{'members':[{'isPresent':true},{'isPresent':true}]}}}")
        };

        private static readonly GameSettings Gamesettings = new GameSettings("FR", 90);
        
        private const string MAP = "france";
        private static readonly HttpResponseMessage HttprequestmsgMapsValid = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"[{'slug':'france'}]")
        };
        [Theory]
        [InlineData("ncfa","")]
        [InlineData("","buildid")]
        [InlineData("","")]
        public async Task GetPresentUser_ShouldCheckParams(string ncfaCookie, string buildId)
        {
            HttpClient httpClient = new(_mockHttpMessageHandler.Object);
            HttpService httpService = new(httpClient, _mockLogger.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => httpService.GetPresentUser(ncfaCookie, buildId));
        }
        
        [Fact]
        public async Task GetPresentUser_ShouldClearClientHeaders()
        {            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(HttprequestmsgValid);
            
            Mock<HttpClient> httpClient = new(_mockHttpMessageHandler.Object);
            httpClient.Object.DefaultRequestHeaders.Add("test", "empty");
            HttpService httpService = new(httpClient.Object, _mockLogger.Object);
        
            await httpService.GetPresentUser(NCFA, BUILDID);
            
            Assert.False(httpClient.Object.DefaultRequestHeaders.Contains("test"));
        }
        
        [Fact]
        public async Task GetPresentUser_ShouldReturnInt()
        {            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(HttprequestmsgValid);
            
            Mock<HttpClient> httpClient = new(_mockHttpMessageHandler.Object);
            HttpService httpService = new(httpClient.Object, _mockLogger.Object);
        
            int response = await httpService.GetPresentUser(NCFA, BUILDID);
            
            Assert.Equal(NB_USERS, response);
        }

        public static IEnumerable<object[]> UpdateData =>
            new List<object[]>
            {
                new object[] { "ncfa", new GameSettings("fr", -1) },
                new object[] { "ncfa", new GameSettings("", 60) },
                new object[] { "", new GameSettings("fr", 60) },
            };

        [Theory]
        [MemberData(nameof(UpdateData))]
        public async Task UpdateSettings_ShouldCheckInputParams(string ncfa, GameSettings settings)
        {
            HttpClient httpClient = new(_mockHttpMessageHandler.Object);
            HttpService httpService = new(httpClient, _mockLogger.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => httpService.UpdateSettings(ncfa, settings));
        }
        
        [Fact]
        public async Task UpdateSettings_ShouldClearClientHeaders()
        {            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(HttprequestmsgValid);
            
            Mock<HttpClient> httpClient = new(_mockHttpMessageHandler.Object);
            httpClient.Object.DefaultRequestHeaders.Add("test", "empty");
            HttpService httpService = new(httpClient.Object, _mockLogger.Object);
        
            await httpService.UpdateSettings(NCFA, Gamesettings);
            
            Assert.False(httpClient.Object.DefaultRequestHeaders.Contains("test"));
        }

        [Fact]
        public async Task GetMaps_ShouldCheckInputParams()
        {
            HttpClient httpClient = new(_mockHttpMessageHandler.Object);
            HttpService httpService = new(httpClient, _mockLogger.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => httpService.GetMaps(string.Empty));
        }
        
        [Fact]
        public async Task GetMaps_ShouldClearClientHeaders()
        {            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(HttprequestmsgMapsValid);
            
            Mock<HttpClient> httpClient = new(_mockHttpMessageHandler.Object);
            httpClient.Object.DefaultRequestHeaders.Add("test", "empty");
            HttpService httpService = new(httpClient.Object, _mockLogger.Object);
        
            await httpService.GetMaps(NCFA);
            
            Assert.False(httpClient.Object.DefaultRequestHeaders.Contains("test"));
        }
        
        [Fact]
        public async Task GetMaps_ShouldReturnMap()
        {            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(HttprequestmsgMapsValid);
            
            Mock<HttpClient> httpClient = new(_mockHttpMessageHandler.Object);
            httpClient.Object.DefaultRequestHeaders.Add("test", "empty");
            HttpService httpService = new(httpClient.Object, _mockLogger.Object);
        
            List<string> results = await httpService.GetMaps(NCFA);
            
            Assert.Contains(MAP, results);
        }
    }
}
