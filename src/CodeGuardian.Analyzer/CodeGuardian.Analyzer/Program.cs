using CodeGuardian.Analyzer;

Console.WriteLine("🛡️ CodeGuardian is analyzing your code...");

var sonarProjectKey = args.ElementAtOrDefault(0)
    ?? throw new ArgumentException("SonarCloud Project Key fehlt");
var sonarToken = Environment.GetEnvironmentVariable("SONAR_TOKEN")
    ?? throw new ArgumentException("SONAR_TOKEN nicht gesetzt");
var anthropicApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new ArgumentException("ANTHROPIC_API_KEY nicht gesetzt");
var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new ArgumentException("GITHUB_TOKEN nicht gesetzt");
var githubRepository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")
    ?? throw new ArgumentException("GITHUB_REPOSITORY nicht gesetzt");
var pullRequestNumber = int.Parse(
    Environment.GetEnvironmentVariable("PR_NUMBER")
    ?? throw new ArgumentException("PR_NUMBER nicht gesetzt"));

Console.WriteLine($"📡 Fetching SonarCloud issues for {sonarProjectKey}...");
var sonarClient = new SonarCloudClient(sonarToken);
var prNumber = Environment.GetEnvironmentVariable("PR_NUMBER");
var issues = await sonarClient.GetIssuesAsync(sonarProjectKey, prNumber);

if (!issues.Any())
{
    Console.WriteLine("✅ No issues found – your code is clean!");
    return;
}

Console.WriteLine($"🔍 Found {issues.Count} issues – asking Claude for analysis...");
var anthropicClient = new AnthropicAnalyzer(anthropicApiKey);
var analysis = await anthropicClient.AnalyzeIssuesAsync(issues);

Console.WriteLine("💬 Posting analysis to GitHub PR...");
var githubClient = new GitHubCommentClient(githubToken, githubRepository);
await githubClient.PostCommentAsync(pullRequestNumber, analysis);

Console.WriteLine("🎉 CodeGuardian finished!");