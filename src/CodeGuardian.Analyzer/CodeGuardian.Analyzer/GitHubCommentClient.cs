using Octokit;

namespace CodeGuardian.Analyzer;

public class GitHubCommentClient
{
    private readonly GitHubClient _client;
    private readonly string _owner;
    private readonly string _repo;

    public GitHubCommentClient(string token, string repository)
    {
        var parts = repository.Split('/');
        _owner = parts[0];
        _repo = parts[1];

        _client = new GitHubClient(new ProductHeaderValue("CodeGuardian"))
        {
            Credentials = new Credentials(token)
        };
    }

    public async Task PostCommentAsync(int pullRequestNumber, string analysis)
    {
        var comment = $"""
            ## 🛡️ CodeGuardian Analysis

            {analysis}

            ---
            *Powered by CodeGuardian + Claude AI*
            """;

        await _client.Issue.Comment.Create(
            _owner,
            _repo,
            pullRequestNumber,
            comment);
    }
}