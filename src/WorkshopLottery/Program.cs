using WorkshopLottery.Models;
using WorkshopLottery.Services;

Console.WriteLine("🎰 Workshop Lottery - Phase 2 Complete 🎰");
Console.WriteLine("=========================================");
Console.WriteLine();

// Test with real sample file if available
var sampleFile = "input/AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx";
if (File.Exists(sampleFile))
{
    Console.WriteLine($"📁 Testing with real sample file: {sampleFile}");
    Console.WriteLine();
    
    var parser = new ExcelParserService();
    var registrations = parser.ParseRegistrations(sampleFile);
    
    Console.WriteLine();
    Console.WriteLine($"🎯 Sample registrations (first 3):");
    foreach (var reg in registrations.Take(3))
    {
        Console.WriteLine($"   Row {reg.RowNumber}: {reg.FullName} ({reg.Email})");
        Console.WriteLine($"      Laptop: {reg.LaptopResponse ?? "N/A"}, Commit: {reg.Commit10MinResponse ?? "N/A"}");
        if (reg.RankingsResponse != null)
            Console.WriteLine($"      Rankings: {reg.RankingsResponse}");
    }
}
else
{
    Console.WriteLine("⚠️ Sample file not found. Run from project root directory.");
}

Console.WriteLine();
Console.WriteLine("🎸 Phase 2 ROCKS! Excel Parser Complete! 🎸");
