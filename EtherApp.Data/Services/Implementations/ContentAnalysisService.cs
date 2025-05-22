// Services/ContentAnalysisService.cs
using EtherApp.Data;
using EtherApp.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ContentAnalysisService : IContentAnalysisService
{
    private readonly AppDBContext _context;
    private readonly MLContext _mlContext;

    public ContentAnalysisService(AppDBContext context)
    {
        _context = context;
        _mlContext = new MLContext();
    }

    public async Task<List<(int InterestId, double Score)>> AnalyzeContentAsync(string content)
    {
        // For a production app, you would implement a proper ML model
        // For this implementation, we'll use a simple keyword matching approach

        var interests = await _context.Interests.ToListAsync();
        var result = new List<(int InterestId, double Score)>();

        // Simple keyword-based mapping
        var interestKeywords = new Dictionary<int, string[]>
        {
            { 1, new[] { "tech", "computer", "software", "programming", "code", "developer", "app" } }, // Technology
            { 2, new[] { "science", "research", "study", "experiment", "lab", "discovery" } }, // Science
            { 3, new[] { "art", "paint", "drawing", "design", "creative", "artist" } }, // Art
            { 4, new[] { "music", "song", "band", "concert", "album", "guitar", "piano" } }, // Music
            { 5, new[] { "sport", "team", "game", "play", "athlete", "fitness", "exercise" } }, // Sports
            { 6, new[] { "travel", "trip", "vacation", "destination", "journey", "explore" } }, // Travel
            { 7, new[] { "food", "recipe", "cook", "bake", "meal", "restaurant", "dish" } }, // Food
            { 8, new[] { "fashion", "style", "clothes", "outfit", "trend", "wear" } }, // Fashion
            { 9, new[] { "game", "gaming", "player", "console", "play", "level" } }, // Gaming
            { 10, new[] { "book", "read", "author", "novel", "story", "literature" } }, // Books
            { 11, new[] { "movie", "film", "cinema", "actor", "director", "watch", "scene" } }, // Movies
            { 12, new[] { "health", "wellness", "medical", "doctor", "exercise", "diet" } }, // Health
            { 13, new[] { "business", "company", "startup", "entrepreneur", "market", "invest" } }, // Business
            { 14, new[] { "education", "learn", "teach", "student", "school", "university" } }, // Education
            { 15, new[] { "politics", "government", "policy", "vote", "election", "law" } }  // Politics
        };

        var contentLower = content.ToLower();

        foreach (var interest in interests)
        {
            if (interestKeywords.TryGetValue(interest.Id, out var keywords))
            {
                double score = keywords.Sum(keyword =>
                    contentLower.Contains(keyword) ? 1.0 / keywords.Length : 0);

                if (score > 0)
                {
                    result.Add((interest.Id, score));
                }
            }
        }

        // Normalize scores to sum to 1.0
        double totalScore = result.Sum(r => r.Score);
        if (totalScore > 0)
        {
            result = result.Select(r => (r.InterestId, r.Score / totalScore)).ToList();
        }

        return result;
    }
}
