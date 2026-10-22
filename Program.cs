using TurnajManager;

const string DefaultFilePath = TournamentFileService.DefaultFilePath;
var currentTournament = new Turnaj("U32", "15.10.2026");

if (args.Length == 0)
{
    Console.WriteLine(currentTournament.DatumKonani);
    return;
}

if (args.Length > 2 || (args[0] != "save" && args[0] != "load"))
{
    PrintUsage();
    Environment.ExitCode = 2;
    return;
}

var filePath = args.Length == 2 ? args[1] : DefaultFilePath;

try
{
    if (args[0] == "save")
    {
        TournamentFileService.Save(currentTournament, filePath);
        Console.WriteLine($"Tournament saved to '{filePath}'.");
    }
    else
    {
        try
        {
            currentTournament = TournamentFileService.Load(filePath);
        }
        catch (InvalidDataException exception)
        {
            Console.Error.WriteLine($"Tournament file '{filePath}' is invalid: {exception.Message}");
            Console.Write($"Create a new tournament and replace this file? (y/N): ");
            var answer = Console.ReadLine();
            if (!string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
            {
                Environment.ExitCode = 1;
                return;
            }

            currentTournament = new Turnaj("U32", "15.10.2026");
            TournamentFileService.Save(currentTournament, filePath);
            Console.WriteLine($"A new tournament was created in '{filePath}'.");
        }

        currentTournament.VypsatHrace();
    }
}
catch (IOException exception)
{
    Console.Error.WriteLine($"Could not access tournament file '{filePath}': {exception.Message}");
    Environment.ExitCode = 1;
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 2;
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  sspsTurnajManager");
    Console.WriteLine("  sspsTurnajManager save [file]");
    Console.WriteLine("  sspsTurnajManager load [file]");
    Console.WriteLine($"Default file: {TournamentFileService.DefaultFilePath}");
}
