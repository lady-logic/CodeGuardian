using System.Net.Http.Headers;
using System.Text.Json;

namespace CodeGuardian.Analyzer;

public record SonarIssue(
    string Key,
    string Rule,
    string Severity,
    string Message,
    string Component,
    int Line
);

public class SonarCloudClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://sonarcloud.io/api";

    public SonarCloudClient(string token)
    {
        _httpClient = new HttpClient();
        var encoded = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{token}:"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", encoded);
    }

    public async Task<List<SonarIssue>> GetIssuesAsync(string projectKey, string? pullRequestNumber = null)
    {
        var url = $"{BaseUrl}/issues/search" +
                  $"?projectKeys={projectKey}" +
                  $"&resolved=false" +
                  $"&types=CODE_SMELL,BUG,VULNERABILITY" +
                  $"&ps=20";

        if (pullRequestNumber != null)
            url += $"&pullRequest={pullRequestNumber}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var issues = new List<SonarIssue>();
        foreach (var issue in doc.RootElement.GetProperty("issues").EnumerateArray())
        {
            issues.Add(new SonarIssue(
                issue.GetProperty("key").GetString()!,
                issue.GetProperty("rule").GetString()!,
                issue.GetProperty("severity").GetString()!,
                issue.GetProperty("message").GetString()!,
                issue.GetProperty("component").GetString()!,
                issue.TryGetProperty("line", out var line) ? line.GetInt32() : 0
            ));
        }

        return issues;
    }
}