using ClosedXML.Excel;
using FluentAssertions;
using WorkshopLottery.Services;

namespace WorkshopLottery.Tests.Services;

/// <summary>
/// Unit tests for the ExcelParserService.
/// </summary>
public class ExcelParserServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ExcelParserService _parser;

    public ExcelParserServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ExcelParserTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _parser = new ExcelParserService();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    #region Basic Parsing Tests

    [Fact]
    public void ParseRegistrations_WithValidFile_ReturnsRegistrations()
    {
        // Arrange
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "Full Name";
            ws.Cell("B1").Value = "Email";
            ws.Cell("C1").Value = "Laptop";
            ws.Cell("D1").Value = "Commit early";

            ws.Cell("A2").Value = "John Doe";
            ws.Cell("B2").Value = "john@example.com";
            ws.Cell("C2").Value = "Yes";
            ws.Cell("D2").Value = "Yes";

            ws.Cell("A3").Value = "Jane Smith";
            ws.Cell("B3").Value = "jane@example.com";
            ws.Cell("C3").Value = "No";
            ws.Cell("D3").Value = "Yes";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert
        registrations.Should().HaveCount(2);
        registrations[0].FullName.Should().Be("John Doe");
        registrations[0].Email.Should().Be("john@example.com");
        registrations[0].LaptopResponse.Should().Be("Yes");
        registrations[1].FullName.Should().Be("Jane Smith");
        registrations[1].Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void ParseRegistrations_WithMissingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "non_existent.xlsx");

        // Act
        var action = () => _parser.ParseRegistrations(nonExistentPath);

        // Assert
        action.Should().Throw<FileNotFoundException>()
            .WithMessage("*Input file not found*");
    }

    [Fact]
    public void ParseRegistrations_MissingRequiredColumn_ThrowsInvalidOperationException()
    {
        // Arrange - missing Email column
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "Full Name";
            ws.Cell("B1").Value = "Laptop";
            ws.Cell("C1").Value = "Commit";
            // Missing Email!

            ws.Cell("A2").Value = "John Doe";
            ws.Cell("B2").Value = "Yes";
            ws.Cell("C2").Value = "Yes";
        });

        // Act
        var action = () => _parser.ParseRegistrations(filePath);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Required columns not found: Email*");
    }

    #endregion

    #region Row Handling Tests

    [Fact]
    public void ParseRegistrations_SkipsEmptyRows()
    {
        // Arrange
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "Name";
            ws.Cell("B1").Value = "Email";
            ws.Cell("C1").Value = "Laptop";
            ws.Cell("D1").Value = "Commit";

            ws.Cell("A2").Value = "John Doe";
            ws.Cell("B2").Value = "john@example.com";
            ws.Cell("C2").Value = "Yes";
            ws.Cell("D2").Value = "Yes";

            // Row 3 is completely empty

            ws.Cell("A4").Value = "Jane Smith";
            ws.Cell("B4").Value = "jane@example.com";
            ws.Cell("C4").Value = "Yes";
            ws.Cell("D4").Value = "Yes";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert - should have 2 registrations, empty row skipped
        registrations.Should().HaveCount(2);
    }

    [Fact]
    public void ParseRegistrations_TrimsWhitespace()
    {
        // Arrange
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "  Name  ";
            ws.Cell("B1").Value = "  Email  ";
            ws.Cell("C1").Value = "Laptop";
            ws.Cell("D1").Value = "Commit";

            ws.Cell("A2").Value = "  John Doe  ";
            ws.Cell("B2").Value = "  john@example.com  ";
            ws.Cell("C2").Value = "  Yes  ";
            ws.Cell("D2").Value = "  Yes  ";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert
        registrations.Should().HaveCount(1);
        registrations[0].FullName.Should().Be("John Doe");
        registrations[0].Email.Should().Be("john@example.com");
        registrations[0].LaptopResponse.Should().Be("Yes");
    }

    [Fact]
    public void ParseRegistrations_IncludesRowNumber()
    {
        // Arrange
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "Name";
            ws.Cell("B1").Value = "Email";
            ws.Cell("C1").Value = "Laptop";
            ws.Cell("D1").Value = "Commit";

            ws.Cell("A2").Value = "John Doe";
            ws.Cell("B2").Value = "john@example.com";
            ws.Cell("C2").Value = "Yes";
            ws.Cell("D2").Value = "Yes";

            ws.Cell("A3").Value = "Jane Smith";
            ws.Cell("B3").Value = "jane@example.com";
            ws.Cell("C3").Value = "Yes";
            ws.Cell("D3").Value = "Yes";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert
        registrations[0].RowNumber.Should().Be(2);
        registrations[1].RowNumber.Should().Be(3);
    }

    #endregion

    #region Optional Column Tests

    [Fact]
    public void ParseRegistrations_HandlesOptionalColumns()
    {
        // Arrange - include workshop columns
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "Name";
            ws.Cell("B1").Value = "Email";
            ws.Cell("C1").Value = "Laptop";
            ws.Cell("D1").Value = "Commit";
            ws.Cell("E1").Value = "Workshop 1";
            ws.Cell("F1").Value = "Workshop 2";
            ws.Cell("G1").Value = "Rank your choices";

            ws.Cell("A2").Value = "John Doe";
            ws.Cell("B2").Value = "john@example.com";
            ws.Cell("C2").Value = "Yes";
            ws.Cell("D2").Value = "Yes";
            ws.Cell("E2").Value = "Yes, I want Workshop 1";
            ws.Cell("F2").Value = "No";
            ws.Cell("G2").Value = "Workshop 1;Workshop 2";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert
        registrations.Should().HaveCount(1);
        registrations[0].RequestedW1Response.Should().Be("Yes, I want Workshop 1");
        registrations[0].RequestedW2Response.Should().Be("No");
        registrations[0].RequestedW3Response.Should().BeNull(); // Not in file
        registrations[0].RankingsResponse.Should().Be("Workshop 1;Workshop 2");
    }

    [Fact]
    public void ParseRegistrations_WorksWithoutOptionalColumns()
    {
        // Arrange - only required columns
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "Name";
            ws.Cell("B1").Value = "Email";
            ws.Cell("C1").Value = "Laptop";
            ws.Cell("D1").Value = "Commit";

            ws.Cell("A2").Value = "John Doe";
            ws.Cell("B2").Value = "john@example.com";
            ws.Cell("C2").Value = "Yes";
            ws.Cell("D2").Value = "Yes";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert
        registrations.Should().HaveCount(1);
        registrations[0].RequestedW1Response.Should().BeNull();
        registrations[0].RequestedW2Response.Should().BeNull();
        registrations[0].RequestedW3Response.Should().BeNull();
        registrations[0].RankingsResponse.Should().BeNull();
    }

    #endregion

    #region Fuzzy Matching Tests

    [Fact]
    public void ParseRegistrations_MatchesFuzzyColumnHeaders()
    {
        // Arrange - use verbose MS Forms style headers
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "What is your full name?";
            ws.Cell("B1").Value = "Your email address";
            ws.Cell("C1").Value = "Will you bring a laptop to the workshop?";
            ws.Cell("D1").Value = "Do you commit to arrive 10 minutes before?";

            ws.Cell("A2").Value = "John Doe";
            ws.Cell("B2").Value = "john@example.com";
            ws.Cell("C2").Value = "Yes, I will bring a laptop";
            ws.Cell("D2").Value = "Yes, I commit";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert
        registrations.Should().HaveCount(1);
        registrations[0].FullName.Should().Be("John Doe");
        registrations[0].Email.Should().Be("john@example.com");
        registrations[0].LaptopResponse.Should().Be("Yes, I will bring a laptop");
        registrations[0].Commit10MinResponse.Should().Be("Yes, I commit");
    }

    [Fact]
    public void ParseRegistrations_EmailColumnTakesPriorityOverName()
    {
        // Arrange - "email address" contains both "email" and "address"
        // Name column should NOT match "email address"
        var filePath = CreateTestExcel(wb =>
        {
            var ws = wb.Worksheets.Add("Data");
            ws.Cell("A1").Value = "Email address";  // Should match Email, not Name
            ws.Cell("B1").Value = "Your name";
            ws.Cell("C1").Value = "Laptop";
            ws.Cell("D1").Value = "Commit";

            ws.Cell("A2").Value = "john@example.com";
            ws.Cell("B2").Value = "John Doe";
            ws.Cell("C2").Value = "Yes";
            ws.Cell("D2").Value = "Yes";
        });

        // Act
        var registrations = _parser.ParseRegistrations(filePath);

        // Assert
        registrations.Should().HaveCount(1);
        registrations[0].Email.Should().Be("john@example.com");
        registrations[0].FullName.Should().Be("John Doe");
    }

    #endregion

    #region Helper Methods

    private string CreateTestExcel(Action<XLWorkbook> configure)
    {
        var filePath = Path.Combine(_testDirectory, $"test_{Guid.NewGuid()}.xlsx");
        using var workbook = new XLWorkbook();
        configure(workbook);
        workbook.SaveAs(filePath);
        return filePath;
    }

    #endregion
}
