using EtherApp.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Implementations
{
    public class HuggingFaceContentAnalysisService : IContentAnalysisService
    {
        private readonly AppDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HuggingFaceContentAnalysisService> _logger;
        private readonly string _apiKey;
        private readonly string _modelEndpoint;
        private readonly IConfiguration _configuration;

        public HuggingFaceContentAnalysisService(
            AppDBContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<HuggingFaceContentAnalysisService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("HuggingFace");
            _logger = logger;
            _configuration = configuration;

            _apiKey = configuration["HuggingFace:ApiKey"] ??
                throw new InvalidOperationException("HuggingFace API key not found in configuration.");
            _modelEndpoint = configuration["HuggingFace:ModelEndpoint"]!;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<List<(int InterestId, double Score)>> AnalyzeContentAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Empty content provided for analysis");
                return new List<(int InterestId, double Score)>();
            }

            try
            {
                var interests = await _context.Interests.ToListAsync();

                // Prepare interest labels for zero-shot classification
                var interestLabels = new List<string>();
                foreach (var interest in interests)
                {
                    // Combine name and description for better context
                    interestLabels.Add(interest.Name);
                }

                // Prepare request for zero-shot classification
                var requestData = new
                {
                    inputs = content,
                    parameters = new
                    {
                        candidate_labels = interestLabels,
                        multi_label = true
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending content to Hugging Face API for analysis");
                var response = await _httpClient.PostAsync(_modelEndpoint, requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Hugging Face API returned error: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);

                    // Fall back to keyword-based analysis if API fails
                    return await FallbackKeywordAnalysisAsync(content);
                }

                var result = await response.Content.ReadFromJsonAsync<ZeroShotClassificationResult>();
                if (result == null)
                {
                    _logger.LogError("Failed to deserialize Hugging Face API response");
                    return await FallbackKeywordAnalysisAsync(content);
                }

                var scores = new List<(int InterestId, double Score)>();

                // Process the classification results
                for (int i = 0; i < result.Labels.Count; i++)
                {
                    var label = result.Labels[i];
                    var score = result.Scores[i];

                    // Extract interest ID from label (format: "Name: Description")
                    var interest = interests.FirstOrDefault(x =>
                                    x.Name.Equals(label.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (interest != null && score > 0.7)
                    {
                        scores.Add((interest.Id, score));
                    }
                }

                // Normalize scores
                var totalScore = scores.Sum(s => s.Score);
                if (totalScore > 0)
                {
                    scores = scores.Select(s => (s.InterestId, s.Score / totalScore)).ToList();
                }

                return scores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing content with Hugging Face API");
                return await FallbackKeywordAnalysisAsync(content);
            }
        }

        private async Task<List<(int InterestId, double Score)>> FallbackKeywordAnalysisAsync(string content)
        {
            _logger.LogInformation("Using fallback keyword analysis");

            var interests = await _context.Interests.ToListAsync();
            var result = new List<(int InterestId, double Score)>();
            var contentLower = content.ToLower();

            // Split content into words for more accurate matching
            var contentWords = contentLower
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '-', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .ToHashSet();

            foreach (var interest in interests)
            {
                // Use the Keywords field from the database if available
                var keywordsString = !string.IsNullOrEmpty(interest.Keywords)
                    ? interest.Keywords
                    : string.Empty;

                var keywords = keywordsString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim().ToLower())
                    .ToArray();

                // Match only on exact words, not substrings
                double wordMatches = keywords.Count(keyword => contentWords.Contains(keyword));

                // Calculate score based on exact matches and their proportion
                if (wordMatches > 0)
                {
                    // Weighted scoring: more matches = higher confidence
                    double score = wordMatches / keywords.Length;

                    // Apply minimum threshold
                    if (score >= 0.1)
                    {
                        result.Add((interest.Id, score));
                    }
                }
            }

            // Normalize scores
            double totalScore = result.Sum(r => r.Score);
            if (totalScore > 0)
            {
                result = result.Select(r => (r.InterestId, r.Score / totalScore)).ToList();
            }

            return result;
        }

        private class ZeroShotClassificationResult
        {
            public List<string> Labels { get; set; } = new();
            public List<double> Scores { get; set; } = new();
            public string Sequence { get; set; } = string.Empty;
        }
    }
}