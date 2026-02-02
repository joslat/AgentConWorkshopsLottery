namespace WorkshopLottery.Tests.Services;

using FluentAssertions;
using NSubstitute;
using WorkshopLottery.Models;
using WorkshopLottery.Services;
using Xunit;

public class LotteryOrchestratorTests
{
    private readonly IExcelParserService _mockParser;
    private readonly IValidationService _mockValidator;
    private readonly ILotteryEngine _mockLotteryEngine;
    private readonly IExcelWriterService _mockWriter;
    private readonly LotteryOrchestrator _sut;

    public LotteryOrchestratorTests()
    {
        _mockParser = Substitute.For<IExcelParserService>();
        _mockValidator = Substitute.For<IValidationService>();
        _mockLotteryEngine = Substitute.For<ILotteryEngine>();
        _mockWriter = Substitute.For<IExcelWriterService>();

        _sut = new LotteryOrchestrator(
            _mockParser,
            _mockValidator,
            _mockLotteryEngine,
            _mockWriter);
    }

    [Fact]
    public void Constructor_ThrowsOnNullParser()
    {
        var act = () => new LotteryOrchestrator(
            null!,
            _mockValidator,
            _mockLotteryEngine,
            _mockWriter);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("parser");
    }

    [Fact]
    public void Constructor_ThrowsOnNullValidator()
    {
        var act = () => new LotteryOrchestrator(
            _mockParser,
            null!,
            _mockLotteryEngine,
            _mockWriter);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("validator");
    }

    [Fact]
    public void Constructor_ThrowsOnNullLotteryEngine()
    {
        var act = () => new LotteryOrchestrator(
            _mockParser,
            _mockValidator,
            null!,
            _mockWriter);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lotteryEngine");
    }

    [Fact]
    public void Constructor_ThrowsOnNullWriter()
    {
        var act = () => new LotteryOrchestrator(
            _mockParser,
            _mockValidator,
            _mockLotteryEngine,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("writer");
    }

    [Fact]
    public void Run_ThrowsOnNullInputPath()
    {
        var act = () => _sut.Run(null!, "output.xlsx");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Run_ThrowsOnEmptyInputPath()
    {
        var act = () => _sut.Run("", "output.xlsx");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Run_ThrowsOnNullOutputPath()
    {
        var act = () => _sut.Run("input.xlsx", null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Run_ThrowsOnEmptyOutputPath()
    {
        var act = () => _sut.Run("input.xlsx", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Run_CallsParserWithInputPath()
    {
        // Arrange
        SetupMocksForSuccessfulRun();
        var inputPath = "test/input.xlsx";
        var outputPath = "test/output.xlsx";

        // Act
        _sut.Run(inputPath, outputPath);

        // Assert
        _mockParser.Received(1).ParseRegistrations(inputPath);
    }

    [Fact]
    public void Run_CallsValidatorWithParsedRegistrations()
    {
        // Arrange
        var rawRegistrations = new List<RawRegistration>
        {
            new() { RowNumber = 2, FullName = "Test User", Email = "test@test.com" }
        };
        
        // Setup mocks first, then override parser to return our specific list
        SetupMocksForSuccessfulRun();
        _mockParser.ParseRegistrations(Arg.Any<string>()).Returns(rawRegistrations);

        // Act
        _sut.Run("input.xlsx", "output.xlsx");

        // Assert
        _mockValidator.Received(1).ValidateAndFilter(
            Arg.Is<IReadOnlyList<RawRegistration>>(list => 
                list.Count == 1 && list[0].Email == "test@test.com"));
    }

    [Fact]
    public void Run_CallsLotteryEngineWithEligibleRegistrations()
    {
        // Arrange
        var eligibleRegistrations = new List<Registration>
        {
            CreateRegistration("Test User", "test@test.com")
        };
        
        SetupMocksForSuccessfulRun();
        _mockValidator.ValidateAndFilter(Arg.Any<IReadOnlyList<RawRegistration>>())
            .Returns(new ValidationResult
            {
                EligibleRegistrations = eligibleRegistrations,
                DisqualifiedRegistrations = new List<Registration>()
            });

        // Act
        _sut.Run("input.xlsx", "output.xlsx");

        // Assert
        _mockLotteryEngine.Received(1).RunLottery(
            eligibleRegistrations,
            Arg.Any<LotteryConfiguration>());
    }

    [Fact]
    public void Run_PassesSeedToLotteryConfiguration()
    {
        // Arrange
        SetupMocksForSuccessfulRun();
        var expectedSeed = 42;

        // Act
        _sut.Run("input.xlsx", "output.xlsx", seed: expectedSeed);

        // Assert
        _mockLotteryEngine.Received(1).RunLottery(
            Arg.Any<IReadOnlyList<Registration>>(),
            Arg.Is<LotteryConfiguration>(c => c.Seed == expectedSeed));
    }

    [Fact]
    public void Run_PassesCapacityToLotteryConfiguration()
    {
        // Arrange
        SetupMocksForSuccessfulRun();
        var capacity = 25;

        // Act
        _sut.Run("input.xlsx", "output.xlsx", capacity: capacity);

        // Assert
        _mockLotteryEngine.Received(1).RunLottery(
            Arg.Any<IReadOnlyList<Registration>>(),
            Arg.Is<LotteryConfiguration>(c => c.Capacity == capacity));
    }

    [Fact]
    public void Run_CallsWriterWithOutputPathAndResult()
    {
        // Arrange
        var expectedResult = CreateSampleLotteryResult();
        SetupMocksForSuccessfulRun();
        _mockLotteryEngine.RunLottery(
            Arg.Any<IReadOnlyList<Registration>>(),
            Arg.Any<LotteryConfiguration>())
            .Returns(expectedResult);

        var outputPath = "test/output.xlsx";

        // Act
        _sut.Run("input.xlsx", outputPath);

        // Assert - orchestrator wraps result, so check it was called with matching properties
        _mockWriter.Received(1).WriteResults(
            outputPath, 
            Arg.Is<LotteryResult>(r => 
                r.Seed == expectedResult.Seed && 
                r.Capacity == expectedResult.Capacity &&
                r.Results == expectedResult.Results));
    }

    [Fact]
    public void Run_ReturnsLotteryResult()
    {
        // Arrange
        var expectedResult = CreateSampleLotteryResult();
        SetupMocksForSuccessfulRun();
        _mockLotteryEngine.RunLottery(
            Arg.Any<IReadOnlyList<Registration>>(),
            Arg.Any<LotteryConfiguration>())
            .Returns(expectedResult);

        // Act
        var result = _sut.Run("input.xlsx", "output.xlsx");

        // Assert - orchestrator wraps result with validation counts
        result.Seed.Should().Be(expectedResult.Seed);
        result.Capacity.Should().Be(expectedResult.Capacity);
        result.Results.Should().BeSameAs(expectedResult.Results);
    }

    [Fact]
    public void Run_UsesDefaultCapacity_WhenNotProvided()
    {
        // Arrange
        SetupMocksForSuccessfulRun();

        // Act
        _sut.Run("input.xlsx", "output.xlsx");

        // Assert
        _mockLotteryEngine.Received(1).RunLottery(
            Arg.Any<IReadOnlyList<Registration>>(),
            Arg.Is<LotteryConfiguration>(c => c.Capacity == 34));
    }

    [Fact]
    public void CreateDefault_ReturnsConfiguredOrchestrator()
    {
        // Act
        var orchestrator = LotteryOrchestrator.CreateDefault();

        // Assert
        orchestrator.Should().NotBeNull();
        orchestrator.Should().BeOfType<LotteryOrchestrator>();
    }

    private void SetupMocksForSuccessfulRun()
    {
        _mockParser.ParseRegistrations(Arg.Any<string>())
            .Returns(new List<RawRegistration>());

        _mockValidator.ValidateAndFilter(Arg.Any<IReadOnlyList<RawRegistration>>())
            .Returns(new ValidationResult
            {
                EligibleRegistrations = new List<Registration>(),
                DisqualifiedRegistrations = new List<Registration>()
            });

        _mockLotteryEngine.RunLottery(
            Arg.Any<IReadOnlyList<Registration>>(),
            Arg.Any<LotteryConfiguration>())
            .Returns(CreateSampleLotteryResult());
    }

    private static LotteryResult CreateSampleLotteryResult()
    {
        return new LotteryResult
        {
            TotalRegistrations = 0,
            EligibleCount = 0,
            DisqualifiedCount = 0,
            Seed = 42,
            Capacity = 34,
            Results = new Dictionary<WorkshopId, WorkshopResult>
            {
                [WorkshopId.W1] = new() { WorkshopId = WorkshopId.W1, Assignments = new List<WorkshopAssignment>() },
                [WorkshopId.W2] = new() { WorkshopId = WorkshopId.W2, Assignments = new List<WorkshopAssignment>() },
                [WorkshopId.W3] = new() { WorkshopId = WorkshopId.W3, Assignments = new List<WorkshopAssignment>() }
            }
        };
    }

    private static Registration CreateRegistration(string name, string email)
    {
        return new Registration
        {
            FullName = name,
            Email = email,
            HasLaptop = true,
            WillCommit10Min = true,
            WorkshopPreferences = new Dictionary<WorkshopId, WorkshopPreference>
            {
                [WorkshopId.W1] = new() { Requested = true, Rank = 1 },
                [WorkshopId.W2] = new() { Requested = false, Rank = null },
                [WorkshopId.W3] = new() { Requested = false, Rank = null }
            }
        };
    }
}
