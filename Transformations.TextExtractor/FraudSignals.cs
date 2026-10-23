using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// Social-engineering and fraud scenarios aimed at the human reader (business email compromise, phishing, gift-card and
/// extortion scams). Single cue words ("urgent", "invoice", "password") are everyday business language, so nothing fires
/// on one word: a scenario needs its cues within <see cref="Window"/> characters of each other in the same part.
/// Urgency, secrecy and authority cues never fire alone; next to a scenario they raise its confidence.
/// Matching runs on the safety gate's folded copy, so lookalike letters and invisible characters do not hide cues.
/// </summary>
internal static class FraudSignals
{
    /// <summary>How close the cues of one scenario must be.</summary>
    internal const int Window = 600;

    private static Regex Cue(string pattern) => new(pattern,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromSeconds(2));

    // ── Cues ──
    private static readonly Regex Money = Cue(@"\b(invoices?|payments?|remittance|wire|bank\s+transfer|transfers?|payable|suppliers?|vendors?|beneficiar(y|ies))\b");
    private static readonly Regex ChangedBankDetails = Cue(@"\b(new|updated?|changed?|different|alternative)\s+(bank(ing)?|account|payment|remittance)\s+(details|information|info|number|account)\b|\b(bank(ing)?|payment|account)\s+details\s+(have|has)\s+(changed|been\s+updated)\b|\b(iban|sort\s+code|routing\s+number|swift|bic)\b");
    private static readonly Regex CredentialRequest = Cue(@"\b(confirm|verify|validate|re-?enter|update|enter|provide|send)\s+(your|the)\s+(password|passcode|credentials|login\s+details|sign-?in\s+details|account\s+details|username\s+and\s+password)\b");
    private static readonly Regex AccountThreat = Cue(@"\b(account|mailbox|email|subscription|access|profile)\s+(will\s+be|has\s+been|is|was)\s+(temporarily\s+)?(suspended|locked|disabled|deactivated|terminated|closed|deleted|restricted|on\s+hold)\b");
    private static readonly Regex ActionLink = Cue(@"\b(verify|confirm|click|log\s*in|sign\s*in|update|validate|reactivate|restore)\b|https?://");
    private static readonly Regex CodeRequest = Cue(@"(?<!\b(not|never|n't|nobody|no\s+one)\s+(\w+\s+)?)\b(send|share|forward|give|read|tell|text)\s+(me|us)?\s*(the|your|this)?\s*(verification|security|one-?time|otp|2fa|mfa|authenticat(ion|or)|access|login|sms)\s+(code|passcode|pin)s?\b");
    private static readonly Regex GiftCards = Cue(@"\b(gift\s*cards?|itunes\s+cards?|google\s+play\s+cards?|steam\s+cards?|amazon\s+cards?|apple\s+cards?|prepaid\s+cards?)\b");
    private static readonly Regex SendCardCodes = Cue(@"\b(send|email|text|forward)\s+(me|us)\s+(the\s+)?(codes?|card\s+numbers|numbers|photos?|pictures?|images?)\b|\bscratch\b|\b(photo|picture)\s+of\s+(the\s+)?(back|cards?)\b");
    private static readonly Regex Urgency = Cue(@"\b(urgent(ly)?|immediately|asap|as\s+soon\s+as\s+possible|right\s+away|within\s+(24|48|72)\s+hours|by\s+end\s+of\s+(the\s+)?day|final\s+(notice|warning|reminder)|act\s+now|time[-\s]sensitive)\b");
    private static readonly Regex Secrecy = Cue(@"\b(strictly\s+)?confidential\b|\bkeep\s+(this|it)\s+(between\s+us|quiet|private|secret)\b|\b(don't|do\s+not)\s+(tell|discuss|mention|involve|share\s+this\s+with)\b|\bdiscreet(ly)?\b");
    private static readonly Regex Authority = Cue(@"\b(ceo|cfo|coo|managing\s+director|chief\s+executive|chairman|president|my\s+boss)\b");
    private static readonly Regex Transfer = Cue(@"\b(wire|transfer|pay|payment|send\s+(the\s+)?money|purchase|buy)\b");
    private static readonly Regex Crypto = Cue(@"\b(bitcoin|btc|ethereum|usdt|tether|monero|crypto(currency)?|wallet\s+address)\b");
    private static readonly Regex Extortion = Cue(@"\b(recorded\s+you|have\s+(access\s+to|control\s+of)\s+your|your\s+(files|data|device)\s+(have|has)\s+been\s+(encrypted|hacked|compromised)|(publish|leak|expose|send)\s+(it|them|the\s+(video|photos?|files))\s+to\s+(your|all\s+your)|i\s+hacked)\b");
    private static readonly Regex MacroLure = Cue(@"\benable\s+(editing|content|macros?)\b");

    private sealed record Scenario(string Code, string Reason, Regex[] Required);

    private static readonly Scenario[] Scenarios =
    [
        new(SafetyFindingCodes.FraudPaymentRedirection, "Payment wording next to new or changed bank details: a common business email compromise pattern. Verify the change by phone.", [Money, ChangedBankDetails]),
        new(SafetyFindingCodes.FraudCredentialRequest, "A request to confirm or enter a password or login details.", [CredentialRequest]),
        new(SafetyFindingCodes.FraudAccountThreat, "A threat to suspend or close an account, with a link or sign-in request: a common phishing pattern.", [AccountThreat, ActionLink]),
        new(SafetyFindingCodes.FraudCodeRequest, "A request to send or share a verification, one-time or access code.", [CodeRequest]),
        new(SafetyFindingCodes.FraudGiftCards, "Gift cards with a request to send their codes or photos: a common scam pattern.", [GiftCards, SendCardCodes]),
        new(SafetyFindingCodes.FraudPressurePayment, "Urgency and secrecy around a payment or purchase: a common CEO-fraud pattern.", [Urgency, Secrecy, Transfer]),
        new(SafetyFindingCodes.FraudExtortion, "A threat combined with a cryptocurrency payment request: an extortion pattern.", [Extortion, Crypto]),
        new(SafetyFindingCodes.FraudMacroLure, "Instructions to enable editing, content or macros: a common malware lure.", [MacroLure]),
    ];

    private static readonly Regex[] Boosters = [Urgency, Secrecy, Authority];

    /// <summary>Scans the folded text; offsets in findings point into the original text through <paramref name="map"/>.</summary>
    internal static void Scan(string original, string folded, int[] map, string source, List<SafetyFinding> findings,
        Func<string, int, int, string> excerpt)
    {
        if (folded.Length == 0) return;
        try
        {
            var cache = new Dictionary<Regex, List<Match>>();
            List<Match> Matches(Regex r) => cache.TryGetValue(r, out var m) ? m : cache[r] = r.Matches(folded).Where(x => x.Length > 0).ToList();

            foreach (var scenario in Scenarios)
            {
                var anchors = Matches(scenario.Required[0]);
                foreach (var anchor in anchors)
                {
                    int lo = anchor.Index - Window, hi = anchor.Index + anchor.Length + Window;
                    var parts = new List<Match> { anchor };
                    bool all = true;
                    foreach (var other in scenario.Required.Skip(1))
                    {
                        var near = Matches(other).FirstOrDefault(m => m.Index >= lo && m.Index < hi);
                        if (near == null) { all = false; break; }
                        parts.Add(near);
                    }
                    if (!all) continue;

                    var boosters = Boosters.Where(b => !scenario.Required.Contains(b))
                        .Count(b => Matches(b).Any(m => m.Index >= lo && m.Index < hi));
                    int start = map[parts.Min(p => p.Index)];
                    var last = parts.MaxBy(p => p.Index + p.Length)!;
                    int end = map[last.Index + last.Length - 1] + 1;

                    findings.Add(new SafetyFinding(scenario.Code, SafetySeverity.Medium, source,
                        boosters > 0 ? scenario.Reason + " With urgency, secrecy or authority pressure." : scenario.Reason,
                        excerpt(original, start, end - start))
                    {
                        Offset = start,
                        Category = SafetyCategory.Fraud,
                        Confidence = boosters > 0 ? SafetyConfidence.High : SafetyConfidence.Medium,
                    });
                    break; // one finding per scenario per part
                }
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Pathological input: keep what was found.
        }
    }

    /// <summary>
    /// 0–100 heuristic score over all findings, for ranking and thresholds (not a probability): severity weight
    /// (Low 8, Medium 25, High 60) plus 10 for High confidence, capped at 100.
    /// </summary>
    internal static int RiskScore(IEnumerable<SafetyFinding> findings)
    {
        int score = 0;
        foreach (var f in findings)
        {
            score += f.Severity switch { SafetySeverity.High => 60, SafetySeverity.Medium => 25, _ => 8 };
            if (f.Confidence == SafetyConfidence.High) score += 10;
            if (score >= 100) return 100;
        }
        return score;
    }
}
