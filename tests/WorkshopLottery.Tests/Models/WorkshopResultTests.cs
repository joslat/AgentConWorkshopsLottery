using FluentAssertions;
using WorkshopLottery.Models;

namespace WorkshopLottery.Tests.Models;

/// <summary>
/// Unit tests for the WorkshopResult model.
/// </summary>
public class WorkshopResultTests
{
    [Fact]
    public void Accepted_ShouldFilterAcceptedAssignments()
    {
        // Arrange
        var reg1 = CreateRegistration("User 1");
        var reg2 = CreateRegistration("User 2");
        var reg3 = CreateRegistration("User 3");

        var result = new WorkshopResult
        {
            WorkshopId = WorkshopId.W1,
            Assignments =
            [
                new WorkshopAssignment { Registration = reg1, Status = AssignmentStatus.Accepted, Wave = 1, Order = 1 },
                new WorkshopAssignment { Registration = reg2, Status = AssignmentStatus.Waitlisted, Wave = null, Order = 2 },
                new WorkshopAssignment { Registration = reg3, Status = AssignmentStatus.Accepted, Wave = 2, Order = 3 }
            ]
        };

        // Act
        var accepted = result.Accepted.ToList();

        // Assert
        accepted.Should().HaveCount(2);
        accepted.Select(a => a.Registration.FullName).Should().Contain("User 1", "User 3");
    }

    [Fact]
    public void Waitlisted_ShouldFilterWaitlistedAssignments()
    {
        // Arrange
        var reg1 = CreateRegistration("User 1");
        var reg2 = CreateRegistration("User 2");
        var reg3 = CreateRegistration("User 3");

        var result = new WorkshopResult
        {
            WorkshopId = WorkshopId.W1,
            Assignments =
            [
                new WorkshopAssignment { Registration = reg1, Status = AssignmentStatus.Accepted, Wave = 1, Order = 1 },
                new WorkshopAssignment { Registration = reg2, Status = AssignmentStatus.Waitlisted, Wave = null, Order = 2 },
                new WorkshopAssignment { Registration = reg3, Status = AssignmentStatus.Waitlisted, Wave = null, Order = 3 }
            ]
        };

        // Act
        var waitlisted = result.Waitlisted.ToList();

        // Assert
        waitlisted.Should().HaveCount(2);
        waitlisted.Select(a => a.Registration.FullName).Should().Contain("User 2", "User 3");
    }

    [Fact]
    public void AcceptedCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var result = new WorkshopResult
        {
            WorkshopId = WorkshopId.W1,
            Assignments =
            [
                new WorkshopAssignment { Registration = CreateRegistration("User 1"), Status = AssignmentStatus.Accepted, Wave = 1, Order = 1 },
                new WorkshopAssignment { Registration = CreateRegistration("User 2"), Status = AssignmentStatus.Waitlisted, Wave = null, Order = 2 },
                new WorkshopAssignment { Registration = CreateRegistration("User 3"), Status = AssignmentStatus.Accepted, Wave = 2, Order = 3 }
            ]
        };

        // Act & Assert
        result.AcceptedCount.Should().Be(2);
    }

    [Fact]
    public void Wave1Count_ShouldReturnCorrectCount()
    {
        // Arrange
        var result = new WorkshopResult
        {
            WorkshopId = WorkshopId.W1,
            Assignments =
            [
                new WorkshopAssignment { Registration = CreateRegistration("User 1"), Status = AssignmentStatus.Accepted, Wave = 1, Order = 1 },
                new WorkshopAssignment { Registration = CreateRegistration("User 2"), Status = AssignmentStatus.Accepted, Wave = 1, Order = 2 },
                new WorkshopAssignment { Registration = CreateRegistration("User 3"), Status = AssignmentStatus.Accepted, Wave = 2, Order = 3 },
                new WorkshopAssignment { Registration = CreateRegistration("User 4"), Status = AssignmentStatus.Waitlisted, Wave = null, Order = 4 }
            ]
        };

        // Act & Assert
        result.Wave1Count.Should().Be(2);
    }

    [Fact]
    public void Wave2Count_ShouldReturnCorrectCount()
    {
        // Arrange
        var result = new WorkshopResult
        {
            WorkshopId = WorkshopId.W1,
            Assignments =
            [
                new WorkshopAssignment { Registration = CreateRegistration("User 1"), Status = AssignmentStatus.Accepted, Wave = 1, Order = 1 },
                new WorkshopAssignment { Registration = CreateRegistration("User 2"), Status = AssignmentStatus.Accepted, Wave = 2, Order = 2 },
                new WorkshopAssignment { Registration = CreateRegistration("User 3"), Status = AssignmentStatus.Accepted, Wave = 2, Order = 3 },
                new WorkshopAssignment { Registration = CreateRegistration("User 4"), Status = AssignmentStatus.Accepted, Wave = 2, Order = 4 }
            ]
        };

        // Act & Assert
        result.Wave2Count.Should().Be(3);
    }

    [Fact]
    public void WaitlistCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var result = new WorkshopResult
        {
            WorkshopId = WorkshopId.W1,
            Assignments =
            [
                new WorkshopAssignment { Registration = CreateRegistration("User 1"), Status = AssignmentStatus.Accepted, Wave = 1, Order = 1 },
                new WorkshopAssignment { Registration = CreateRegistration("User 2"), Status = AssignmentStatus.Waitlisted, Wave = null, Order = 2 },
                new WorkshopAssignment { Registration = CreateRegistration("User 3"), Status = AssignmentStatus.Waitlisted, Wave = null, Order = 3 }
            ]
        };

        // Act & Assert
        result.WaitlistCount.Should().Be(2);
    }

    [Fact]
    public void EmptyAssignments_ShouldReturnZeroCounts()
    {
        // Arrange
        var result = new WorkshopResult { WorkshopId = WorkshopId.W1 };

        // Assert
        result.AcceptedCount.Should().Be(0);
        result.Wave1Count.Should().Be(0);
        result.Wave2Count.Should().Be(0);
        result.WaitlistCount.Should().Be(0);
        result.Accepted.Should().BeEmpty();
        result.Waitlisted.Should().BeEmpty();
    }

    private static Registration CreateRegistration(string name) => 
        new() { FullName = name, Email = $"{name.Replace(" ", "").ToLower()}@test.com" };
}
