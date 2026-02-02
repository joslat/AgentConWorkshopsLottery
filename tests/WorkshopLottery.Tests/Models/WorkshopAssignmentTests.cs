using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the WorkshopAssignment model.
/// </summary>
public class WorkshopAssignmentTests
{
    [Fact]
    public void WorkshopAssignment_ShouldStoreRegistration()
    {
        // Arrange
        var registration = new Registration
        {
            FullName = "Test User",
            Email = "test@test.com"
        };

        // Act
        var assignment = new WorkshopAssignment
        {
            Registration = registration,
            Status = AssignmentStatus.Accepted,
            Wave = 1,
            Order = 1
        };

        // Assert
        assignment.Registration.Should().Be(registration);
        assignment.Registration.FullName.Should().Be("Test User");
    }

    [Fact]
    public void AcceptedAssignment_ShouldHaveWave()
    {
        // Arrange & Act
        var assignment = new WorkshopAssignment
        {
            Registration = CreateRegistration(),
            Status = AssignmentStatus.Accepted,
            Wave = 1,
            Order = 1
        };

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Accepted);
        assignment.Wave.Should().Be(1);
    }

    [Fact]
    public void WaitlistedAssignment_ShouldHaveNullWave()
    {
        // Arrange & Act
        var assignment = new WorkshopAssignment
        {
            Registration = CreateRegistration(),
            Status = AssignmentStatus.Waitlisted,
            Wave = null,
            Order = 35
        };

        // Assert
        assignment.Status.Should().Be(AssignmentStatus.Waitlisted);
        assignment.Wave.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Wave_ShouldAcceptValidValues(int wave)
    {
        // Arrange & Act
        var assignment = new WorkshopAssignment
        {
            Registration = CreateRegistration(),
            Status = AssignmentStatus.Accepted,
            Wave = wave,
            Order = 1
        };

        // Assert
        assignment.Wave.Should().Be(wave);
    }

    [Fact]
    public void WorkshopAssignment_ShouldBeRecordType_WithValueEquality()
    {
        // Arrange
        var registration = CreateRegistration();
        var assignment1 = new WorkshopAssignment
        {
            Registration = registration,
            Status = AssignmentStatus.Accepted,
            Wave = 1,
            Order = 1
        };
        var assignment2 = new WorkshopAssignment
        {
            Registration = registration,
            Status = AssignmentStatus.Accepted,
            Wave = 1,
            Order = 1
        };
        var assignment3 = new WorkshopAssignment
        {
            Registration = registration,
            Status = AssignmentStatus.Accepted,
            Wave = 2,
            Order = 1
        };

        // Assert
        assignment1.Should().Be(assignment2);
        assignment1.Should().NotBe(assignment3);
    }

    [Fact]
    public void Order_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var assignment = new WorkshopAssignment
        {
            Registration = CreateRegistration(),
            Status = AssignmentStatus.Accepted,
            Wave = 1,
            Order = 42
        };

        // Assert
        assignment.Order.Should().Be(42);
    }

    private static Registration CreateRegistration() => 
        new() { FullName = "Test User", Email = "test@test.com" };
}
