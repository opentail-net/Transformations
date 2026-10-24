using NUnit.Framework;
using Transformations.Text;
using static Transformations.Tests.SafetyTestDocs;

namespace Transformations.Tests;

/// <summary>Social-engineering and fraud scenarios: each fires on its combination of cues, never on single words.</summary>
[TestFixture]
public class TextExtractorFraudSignalTests
{
    private TextExtractor _extractor = null!;

    [SetUp]
    public void Setup() => _extractor = new TextExtractor();

    private IReadOnlyList<SafetyFinding> Fraud(string text)
        => _extractor.GetText("m.txt", Utf8(text)).Findings.Where(f => f.Category == SafetyCategory.Fraud).ToList();

    [TestCase("Please note our bank details have changed. Pay invoice 4471 to the new account below.", SafetyFindingCodes.FraudPaymentRedirection)]
    [TestCase("Supplier update: please use our new banking details for all future payments. IBAN GB00 0000 0000.", SafetyFindingCodes.FraudPaymentRedirection)]
    [TestCase("Security check: please confirm your password by replying to this email.", SafetyFindingCodes.FraudCredentialRequest)]
    [TestCase("Your mailbox has been suspended. Click here to verify your account: https://example.test/x", SafetyFindingCodes.FraudAccountThreat)]
    [TestCase("Hi, I sent you a code by mistake, can you send me the verification code you just got?", SafetyFindingCodes.FraudCodeRequest)]
    [TestCase("I need 5 Google Play cards for clients. Scratch them and send me the codes.", SafetyFindingCodes.FraudGiftCards)]
    [TestCase("I need you to make a transfer today, urgently. Keep this between us until the deal closes.", SafetyFindingCodes.FraudPressurePayment)]
    [TestCase("I recorded you through your webcam. Pay 1000 USD in bitcoin to this wallet address.", SafetyFindingCodes.FraudExtortion)]
    [TestCase("This document is protected. Enable editing and enable content to view it.", SafetyFindingCodes.FraudMacroLure)]
    public void Scenario_IsDetected(string text, string code)
    {
        var finding = Fraud(text).Single(f => f.Code == code);

        Assert.That(finding.Severity, Is.EqualTo(SafetySeverity.Medium));
        Assert.That(finding.Excerpt, Is.Not.Empty);
    }

    [TestCase("Please find attached invoice 4471. Payment is due within 30 days.")]
    [TestCase("Your verification code is 381204. Do not share this code with anyone.")]
    [TestCase("You asked to reset your password. Click the link to choose a new one. If this wasn't you, ignore this email.")]
    [TestCase("Urgent: the 3pm meeting has moved to 4pm.")]
    [TestCase("Confidential: draft HR policy for review.")]
    [TestCase("Buy a gift card for a friend this Christmas and send it straight to their inbox!")]
    [TestCase("Bitcoin rose 4% this week as markets rallied.")]
    [TestCase("Our new office address is 10 High Street.")]
    [TestCase("Invoice 4471: please pay to IBAN GB12 3456 7890, sort code 12-34-56, SWIFT ABCDGB2L.")]
    public void NearMiss_IsNotFraud(string text)
    {
        Assert.That(Fraud(text), Is.Empty);
    }

    [Test]
    public void CuesFarApart_DoNotCombine()
    {
        var text = "Invoice 4471 is attached. " + new string('x', 1000) + " Our new bank details will follow by post.";

        Assert.That(Fraud(text).Any(f => f.Code == SafetyFindingCodes.FraudPaymentRedirection), Is.False);
    }

    [Test]
    public void PressureCues_RaiseConfidence()
    {
        var plain = Fraud("Please pay invoice 88 to our new bank account details below.").Single();
        var pressured = Fraud("URGENT - please pay invoice 88 to our new bank account details today. This is confidential.").Single(f => f.Code == SafetyFindingCodes.FraudPaymentRedirection);

        Assert.That(plain.Confidence, Is.EqualTo(SafetyConfidence.Medium));
        Assert.That(pressured.Confidence, Is.EqualTo(SafetyConfidence.High));
    }

    [Test]
    public void LookalikeLetters_DoNotHideCues()
    {
        // Cyrillic letters in "password" and "confirm".
        Assert.That(Fraud("Please cоnfirm your pаssword now.").Single().Code, Is.EqualTo(SafetyFindingCodes.FraudCredentialRequest));
    }

    [Test]
    public void FraudInEmail_IsReviewedUnderAiProfile_AndScored()
    {
        var eml = Eml("Hi, our bank details have changed. Please pay the attached invoice to the new account today. Keep this confidential.", null);

        var result = _extractor.GetText("m.eml", eml, ExtractionOptions.ForAiIngestion());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.SafetyVerdict, Is.EqualTo(SafetyVerdict.ReviewRequired));
        Assert.That(result.RiskScore, Is.InRange(25, 100));
        Assert.That(_extractor.GetText("a.txt", Utf8("Plain meeting notes.")).RiskScore, Is.Zero);
    }

    [Test]
    public void FraudInOcrText_IsDetectedToo()
    {
        var options = ExtractionOptions.ForAiIngestion() with { OcrEngine = new FakeOcr("Your account has been suspended. Sign in to restore access.") };

        var result = _extractor.GetText("scan.png", Png1x1, options);

        Assert.That(result.Findings.Select(f => f.Code), Does.Contain(SafetyFindingCodes.FraudAccountThreat));
    }
}
