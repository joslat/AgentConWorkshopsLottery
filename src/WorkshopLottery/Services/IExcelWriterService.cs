using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Interface for writing lottery results to Excel.
/// </summary>
public interface IExcelWriterService
{
    /// <summary>
    /// Writes the lottery results to an Excel file with one sheet per workshop.
    /// </summary>
    /// <param name="filePath">Output file path.</param>
    /// <param name="result">Lottery results to write.</param>
    void WriteResults(string filePath, LotteryResult result);
}
