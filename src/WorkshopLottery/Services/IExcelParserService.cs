using WorkshopLottery.Models;

namespace WorkshopLottery.Services;

/// <summary>
/// Service for parsing Excel files exported from Microsoft Forms.
/// </summary>
public interface IExcelParserService
{
    /// <summary>
    /// Parses an Excel file and extracts raw registration data.
    /// </summary>
    /// <param name="filePath">Path to the Excel file.</param>
    /// <returns>List of raw registrations extracted from the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required columns are missing.</exception>
    IReadOnlyList<RawRegistration> ParseRegistrations(string filePath);
}
