using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Interface for the lottery engine that performs weighted random selection
/// and wave-based seat assignment.
/// </summary>
public interface ILotteryEngine
{
    /// <summary>
    /// Runs the lottery to assign workshop seats using the Efraimidis-Spirakis algorithm.
    /// </summary>
    /// <param name="eligibleRegistrations">List of eligible registrations to process.</param>
    /// <param name="config">Lottery configuration including seed, capacity, and workshop order.</param>
    /// <returns>The lottery result containing per-workshop assignments.</returns>
    LotteryResult RunLottery(
        IReadOnlyList<Registration> eligibleRegistrations,
        LotteryConfiguration config);
}
