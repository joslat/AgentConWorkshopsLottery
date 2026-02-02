using WorkshopLottery.Models;

Console.WriteLine("🎰 Workshop Lottery - Phase 1 Complete 🎰");
Console.WriteLine("=========================================");
Console.WriteLine();

var config = new LotteryConfiguration { InputPath = "test.xlsx" };
Console.WriteLine($"✅ Domain models loaded successfully!");
Console.WriteLine($"   Default capacity: {config.Capacity} seats per workshop");
Console.WriteLine($"   Effective seed: {config.GetEffectiveSeed()}");
Console.WriteLine($"   Workshop order: {string.Join(", ", config.WorkshopOrder)}");
Console.WriteLine();

// Quick model validation
var registration = new Registration 
{ 
    FullName = "Test User", 
    Email = "TEST@Example.com",
    HasLaptop = true,
    WillCommit10Min = true
};

Console.WriteLine($"📧 Email normalization test:");
Console.WriteLine($"   Original: '{registration.Email}'");
Console.WriteLine($"   Normalized: '{registration.NormalizedEmail}'");
Console.WriteLine($"   Meets basic requirements: {registration.MeetsBasicRequirements()}");
Console.WriteLine();

var pref = new WorkshopPreference { Requested = true, Rank = 1 };
Console.WriteLine($"⚖️ Weight calculation test:");
Console.WriteLine($"   Rank 1 weight: {new WorkshopPreference { Rank = 1 }.Weight} (expected: 5)");
Console.WriteLine($"   Rank 2 weight: {new WorkshopPreference { Rank = 2 }.Weight} (expected: 2)");
Console.WriteLine($"   Rank 3 weight: {new WorkshopPreference { Rank = 3 }.Weight} (expected: 1)");
Console.WriteLine($"   No rank weight: {new WorkshopPreference().Weight} (expected: 1)");
Console.WriteLine();

Console.WriteLine("🎸 Phase 1 ROCKS! Ready for Phase 2! 🎸");
