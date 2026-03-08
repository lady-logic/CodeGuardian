using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace CodeGuardian.Analyzer;

public class AnthropicAnalyzer
{
    private readonly AnthropicClient _client;
    private const string Model = "claude-haiku-4-5-20251001";

    public AnthropicAnalyzer(string apiKey)
    {
        _client = new AnthropicClient(apiKey);
    }

    public async Task<string> AnalyzeIssuesAsync(List<SonarIssue> issues)
    {
        var issuesSummary = string.Join("\\n", issues.Select(i =>
            $"- [{i.Severity}] {i.Rule}: {i.Message} " +
            $"(File: {i.Component}, Line: {i.Line})"));

        var prompt = $"""
            Du bist CodeGuardian, ein freundlicher Code Review Agent.
            
            SonarCloud hat folgende Issues in diesem Pull Request gefunden:
            
            {issuesSummary}
            
            Bitte analysiere diese Issues und erstelle einen strukturierten 
            GitHub PR Kommentar auf Deutsch mit:
            
            1. Eine kurze Zusammenfassung (1-2 Sätze)
            2. Die kritischsten Issues zuerst (BUG > VULNERABILITY > CODE_SMELL)
            3. Für jeden Issue einen konkreten Fix-Vorschlag in C#
            4. Eine ermutigende Schlussbemerkung
            
            Formatiere die Antwort als Markdown für GitHub.
            Sei konstruktiv und lehrreich, nicht kritisch.
            """;

        var response = await _client.Messages.GetClaudeMessageAsync(
            new MessageParameters
            {
                Model = Model,
                MaxTokens = 1500,
                Messages =
                [
                    new Message(RoleType.User, prompt)
                ]
            });

        return response.Content.OfType<TextContent>().FirstOrDefault()?.Text
               ?? "CodeGuardian konnte keine Analyse erstellen.";
    }
}