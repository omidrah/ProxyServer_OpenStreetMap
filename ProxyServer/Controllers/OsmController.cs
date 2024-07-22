using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;

namespace ProxyServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OsmController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        public OsmController(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }
        // GET: api/osm/{z}/{x}/{y}
        //https://localhost:3000/api/osm/6/10/10
        [HttpGet("{z}/{x}/{y}")]
        public async Task<IActionResult> GetTile(int z, int x, int y)
        {
            var cacheKey = $"{z}/{x}/{y}";
            
            if (!_cache.TryGetValue(cacheKey, out byte[] imageBytes))
            {
                var client = _httpClientFactory.CreateClient("OSMClient");
                var osmUrl = $"https://tile.openstreetmap.org/{z}/{x}/{y}.png";
                var response = await client.GetAsync(osmUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }

                imageBytes = await response.Content.ReadAsByteArrayAsync();
                _cache.Set(cacheKey, imageBytes, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
            }
            return File(imageBytes, "image/png");
        }
    }
}
