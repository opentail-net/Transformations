using NUnit.Framework;
using Transformations.Text;

namespace Transformations.Tests;

[TestFixture]
public class CsvDataProfilerTests
{
    [Test]
    public void ProfileCsv_DetectsPipeDelimiter()
    {
        string csv = "Name|Age|City\nAlice|30|London\nBob|25|Paris";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.ColumnCount, Is.EqualTo(3));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Name", "Age", "City" }));
    }

    [Test]
    public void ProfileCsv_DetectsDelimiterOutsideQuotesOnly()
    {
        string csv = "Name,Note,Age\n\"Doe, John\",\"value;inside\",30";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.ColumnCount, Is.EqualTo(3));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Name", "Note", "Age" }));
    }

    [Test]
    public void ProfileCsv_TopValues_CapsAtFiveDistinctEntries()
    {
        string csv = "Col\nA\nB\nC\nD\nE\nF";

        var profile = CsvDataProfiler.ProfileCsv(csv);
        var dist = profile.Distributions.Single(d => d.ColumnName == "Col");

        Assert.That(dist.TopValues.Count, Is.EqualTo(5));
    }

    [Test]
    public void ProfileCsv_AllMissingColumn_IsInferredAsEmptyAndMissingRatioOne()
    {
        string csv = "Name,Note\nAlice,\nBob,\nCharlie,";

        var profile = CsvDataProfiler.ProfileCsv(csv);
        var noteDist = profile.Distributions.Single(d => d.ColumnName == "Note");

        Assert.That(profile.ColumnTypes["Note"], Is.EqualTo(CsvColumnType.Empty));
        Assert.That(noteDist.MissingRatio, Is.EqualTo(1.0));
    }

    [Test]
    public void DetectDataIssues_DetectsDuplicateAndBlankHeaders()
    {
        string csv = "Name,Name,\nAlice,A1,X\nBob,B1,Y";

        var issues = CsvDataProfiler.DetectDataIssues(csv);

        Assert.That(issues.Any(i => i.Code == "duplicate_headers"), Is.True);
        Assert.That(issues.Any(i => i.Code == "blank_header"), Is.True);
    }

    [Test]
    public void ProfileCsv_HandlesQuotedCommasAndEscapedQuotes()
    {
        string csv = "Name,Note\n\"Doe, John\",\"He said \"\"hello\"\"\"";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.RowCount, Is.EqualTo(1));
        Assert.That(profile.ColumnCount, Is.EqualTo(2));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Name", "Note" }));

        var noteDist = profile.Distributions.First(d => d.ColumnName == "Note");
        Assert.That(noteDist.TopValues.ContainsKey("He said \"hello\""), Is.True);
    }

    [Test]
    public void ProfileCsv_HandlesMultilineQuotedFields()
    {
        string csv = "Id,Description\n1,\"Line one\nLine two\"\n2,\"Single line\"";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.RowCount, Is.EqualTo(2));
        Assert.That(profile.ColumnCount, Is.EqualTo(2));
        var dist = profile.Distributions.First(d => d.ColumnName == "Description");
        Assert.That(dist.DistinctCount, Is.EqualTo(2));
    }

    [Test]
    public void ProfileCsv_DetectsSemicolonDelimiter()
    {
        string csv = "Name;Age;City\nAlice;30;London\nBob;25;Paris";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.ColumnCount, Is.EqualTo(3));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Name", "Age", "City" }));
        Assert.That(profile.ColumnTypes["Age"], Is.EqualTo(CsvColumnType.Integer));
    }

    [Test]
    public void ProfileCsv_DetectsTabDelimiter()
    {
        string csv = "Name\tAge\tActive\nAlice\t30\ttrue\nBob\t25\tfalse";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.ColumnCount, Is.EqualTo(3));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Name", "Age", "Active" }));
        Assert.That(profile.ColumnTypes["Active"], Is.EqualTo(CsvColumnType.Boolean));
    }

    [Test]
    public void ProfileCsv_HandlesUtf8BomInFirstHeader()
    {
        string csv = "\uFEFFName,Age\nAlice,30";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.Headers[0], Is.EqualTo("Name"));
        Assert.That(profile.ColumnTypes.ContainsKey("Name"), Is.True);
    }

    [Test]
    public void ProfileCsv_SingleRowUsesHeaderWhenLabelLike()
    {
        string csv = "Name,Age,City";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.HasHeader, Is.True);
        Assert.That(profile.RowCount, Is.EqualTo(0));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Name", "Age", "City" }));
    }

    [Test]
    public void ProfileCsv_SingleRowUsesDataWhenNumericLike()
    {
        string csv = "1,2,3";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.HasHeader, Is.False);
        Assert.That(profile.RowCount, Is.EqualTo(1));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Column1", "Column2", "Column3" }));
    }

    [Test]
    public void ProfileCsv_InfersHeadersTypesAndDistributions()
    {
        string csv = "Name,Age,Active\nAlice,30,true\nBob,25,false\nAlice,30,true";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.HasHeader, Is.True);
        Assert.That(profile.RowCount, Is.EqualTo(3));
        Assert.That(profile.ColumnCount, Is.EqualTo(3));
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Name", "Age", "Active" }));
        Assert.That(profile.ColumnTypes["Name"], Is.EqualTo(CsvColumnType.String));
        Assert.That(profile.ColumnTypes["Age"], Is.EqualTo(CsvColumnType.Integer));
        Assert.That(profile.ColumnTypes["Active"], Is.EqualTo(CsvColumnType.Boolean));

        var nameDist = profile.Distributions.First(d => d.ColumnName == "Name");
        Assert.That(nameDist.DistinctCount, Is.EqualTo(2));
        Assert.That(nameDist.TopValues["Alice"], Is.EqualTo(2));

        Assert.That(profile.Issues.Any(i => i.Code == "duplicate_rows"), Is.True);
    }

    [Test]
    public void InferColumnTypes_ReturnsExpectedTypes_ForMixedColumns()
    {
        string csv = "Id,Amount,When,Token,Mixed\n1,12.5,2026-04-23,8f14e45f-ea78-4f7c-bc4d-9f8d7a1e1234,1\n2,14.0,2026-04-24,6c5b4a39-1e2f-4d3c-8b7a-5d4e3f2a1111,abc";

        var types = CsvDataProfiler.InferColumnTypes(csv);

        Assert.That(types["Id"], Is.EqualTo(CsvColumnType.Integer));
        Assert.That(types["Amount"], Is.EqualTo(CsvColumnType.Decimal));
        Assert.That(types["When"], Is.EqualTo(CsvColumnType.DateTime));
        Assert.That(types["Token"], Is.EqualTo(CsvColumnType.Guid));
        Assert.That(types["Mixed"], Is.EqualTo(CsvColumnType.Mixed));
    }

    [Test]
    public void DetectDataIssues_FindsMissingValuesAndWidthMismatch()
    {
        string csv = "Name,Age,City\nAlice,30,London\nBob,,Paris\nCharlie,22";

        var issues = CsvDataProfiler.DetectDataIssues(csv);

        Assert.That(issues.Any(i => i.Code == "missing_values" && i.ColumnName == "Age"), Is.True);
        Assert.That(issues.Any(i => i.Code == "row_width_mismatch"), Is.True);
    }

    [Test]
    public void DetectDataIssues_ReturnsEmptyDatasetIssue_ForNoRows()
    {
        string csv = "";

        var issues = CsvDataProfiler.DetectDataIssues(csv);

        Assert.That(issues.Any(i => i.Code == "empty_dataset"), Is.True);
    }

    [Test]
    public void ProfileCsv_InfersSyntheticHeaders_WhenNoHeaderPresent()
    {
        string csv = "1,2,3\n4,5,6";

        var profile = CsvDataProfiler.ProfileCsv(csv);

        Assert.That(profile.HasHeader, Is.False);
        Assert.That(profile.Headers, Is.EqualTo(new[] { "Column1", "Column2", "Column3" }));
        Assert.That(profile.ColumnTypes["Column1"], Is.EqualTo(CsvColumnType.Integer));
    }
}
