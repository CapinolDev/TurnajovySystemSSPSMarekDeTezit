using System.Globalization;

namespace TurnajManager;

internal static class TournamentCli
{
    public static int Run(string[] args)
    {
        var (commandArgs, filePath) = ExtractFilePath(args);
        if (commandArgs is null)
        {
            PrintUsage();
            return 2;
        }

        if (commandArgs.Length == 0 || commandArgs[0] is "help" or "--help" or "-h")
        {
            PrintUsage();
            return 0;
        }

        try
        {
            return Execute(commandArgs, filePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Could not access tournament file '{filePath}': {exception.Message}");
            return 1;
        }
        catch (Exception exception) when (
            exception is ArgumentException or FormatException or InvalidDataException or InvalidOperationException)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }
    }

    private static int Execute(string[] args, string filePath)
    {
        switch (args[0])
        {
            case "create":
                RequireCount(args, 3, "create <name> <date>");
                if (File.Exists(filePath))
                {
                    throw new InvalidOperationException(
                        $"Tournament file '{filePath}' already exists. Choose another --file path or remove it first.");
                }

                var tournament = new Turnaj(args[1], args[2]);
                TournamentFileService.Save(tournament, filePath);
                Console.WriteLine($"Created tournament '{tournament.Nazev}' ({tournament.DatumKonani:d}).");
                return 0;

            case "info":
                RequireCount(args, 1, "info");
                PrintTournament(TournamentFileService.Load(filePath));
                return 0;

            case "players":
                RequireCount(args, 1, "players");
                PrintPlayers(TournamentFileService.Load(filePath));
                return 0;

            case "player":
                return ExecutePlayerCommand(args, filePath);

            case "round":
                return ExecuteRoundCommand(args, filePath);

            case "result":
                return RecordResult(args, filePath);

            case "standings":
                RequireCount(args, 1, "standings");
                PrintStandings(TournamentFileService.Load(filePath));
                return 0;

            case "save":
                RequireCount(args, 1, "save");
                TournamentFileService.Save(TournamentFileService.Load(filePath), filePath);
                Console.WriteLine($"Tournament saved to '{filePath}'.");
                return 0;

            case "load":
                RequireCount(args, 1, "load");
                PrintTournament(TournamentFileService.Load(filePath));
                return 0;

            default:
                PrintUsage();
                return 2;
        }
    }

    private static int ExecutePlayerCommand(string[] args, string filePath)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: player <add|remove> <name>");
        }

        var tournament = TournamentFileService.Load(filePath);
        switch (args[1])
        {
            case "add":
                RequireCount(args, 3, "player add <name>");
                if (!tournament.PridatHrace(new Hrac(args[2], 0, 0)))
                {
                    throw new ArgumentException("Player name must be non-empty and unique.");
                }

                TournamentFileService.Save(tournament, filePath);
                Console.WriteLine($"Added player '{args[2]}'.");
                return 0;

            case "remove":
                RequireCount(args, 3, "player remove <name>");
                tournament.OdebratHrace(args[2]);
                TournamentFileService.Save(tournament, filePath);
                Console.WriteLine($"Removed player '{args[2]}'.");
                return 0;

            default:
                throw new ArgumentException("Usage: player <add|remove> <name>");
        }
    }

    private static int ExecuteRoundCommand(string[] args, string filePath)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: round <create|list|close> [number]");
        }

        var tournament = TournamentFileService.Load(filePath);
        switch (args[1])
        {
            case "create":
                RequireCount(args, 2, "round create");
                var round = tournament.VytvoritDalsiKolo();
                TournamentFileService.Save(tournament, filePath);
                Console.WriteLine($"Created round {tournament.Kola.Count}.");
                PrintRound(round, tournament.Kola.Count);
                return 0;

            case "list":
                RequireCount(args, 2, "round list");
                PrintRounds(tournament);
                return 0;

            case "close":
                RequireCount(args, 3, "round close <number>");
                var roundNumber = ParsePositiveInteger(args[2], "round number");
                GetRound(tournament, roundNumber).Uzavrit();
                TournamentFileService.Save(tournament, filePath);
                Console.WriteLine($"Closed round {roundNumber}.");
                return 0;

            default:
                throw new ArgumentException("Usage: round <create|list|close> [number]");
        }
    }

    private static int RecordResult(string[] args, string filePath)
    {
        RequireCount(args, 4, "result <round> <match> <a|b|draw>");
        var tournament = TournamentFileService.Load(filePath);
        var roundNumber = ParsePositiveInteger(args[1], "round number");
        var matchNumber = ParsePositiveInteger(args[2], "match number");
        var round = GetRound(tournament, roundNumber);
        if (matchNumber > round.Zapasy.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(matchNumber), "Match number does not exist.");
        }

        var result = args[3].ToLowerInvariant() switch
        {
            "a" => VysledekZapasu.WinA,
            "b" => VysledekZapasu.WinB,
            "draw" => VysledekZapasu.Remiza,
            _ => throw new ArgumentException("Result must be 'a', 'b', or 'draw'.")
        };
        round.ZadatVysledek(round.Zapasy[matchNumber - 1], result);
        TournamentFileService.Save(tournament, filePath);
        Console.WriteLine($"Recorded result for round {roundNumber}, match {matchNumber}.");
        return 0;
    }

    private static void PrintTournament(Turnaj tournament)
    {
        Console.WriteLine($"Tournament: {tournament.Nazev}");
        Console.WriteLine($"Date: {tournament.DatumKonani.ToString("d", CultureInfo.CurrentCulture)}");
        Console.WriteLine($"Players: {tournament.HraciVsichni.Count}");
        Console.WriteLine($"Rounds: {tournament.Kola.Count}");
    }

    private static void PrintPlayers(Turnaj tournament)
    {
        Console.WriteLine($"Players in {tournament.Nazev}:");
        for (var index = 0; index < tournament.HraciVsichni.Count; index++)
        {
            var player = tournament.HraciVsichni[index];
            Console.WriteLine(
                $"{index + 1}. {player.Jmeno} [{player.Tag}] - {player.Body.ToString("0.##", CultureInfo.InvariantCulture)} points");
        }
    }

    private static void PrintRounds(Turnaj tournament)
    {
        if (tournament.Kola.Count == 0)
        {
            Console.WriteLine("No rounds have been created.");
            return;
        }

        for (var index = 0; index < tournament.Kola.Count; index++)
        {
            PrintRound(tournament.Kola[index], index + 1);
        }
    }

    private static void PrintRound(Kolo round, int roundNumber)
    {
        Console.WriteLine($"Round {roundNumber} ({(round.JeUzavrene ? "closed" : "open")}):");
        for (var index = 0; index < round.Zapasy.Count; index++)
        {
            var match = round.Zapasy[index];
            var result = match.Vysledek switch
            {
                VysledekZapasu.WinA => "A wins",
                VysledekZapasu.WinB => "B wins",
                VysledekZapasu.Remiza => "draw",
                _ => "pending"
            };
            Console.WriteLine(
                $"  {index + 1}. {match.Hrac1.Jmeno} vs {match.Hrac2?.Jmeno ?? "BYE"} - {result}");
        }
    }

    private static void PrintStandings(Turnaj tournament)
    {
        Console.WriteLine($"Standings: {tournament.Nazev}");
        Console.WriteLine("Rank  Player                         Points  Games");
        var rankedPlayers = tournament.HraciVsichni
            .OrderByDescending(player => player.Body)
            .ThenBy(player => player.Jmeno, StringComparer.OrdinalIgnoreCase);
        var rank = 1;
        foreach (var player in rankedPlayers)
        {
            Console.WriteLine(
                $"{rank++,4}  {player.Jmeno,-29} {player.Body,6:0.##}  {player.HryCelkemHrane,5}");
        }
    }

    private static Kolo GetRound(Turnaj tournament, int roundNumber)
    {
        if (roundNumber > tournament.Kola.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), "Round number does not exist.");
        }

        return tournament.Kola[roundNumber - 1];
    }

    private static int ParsePositiveInteger(string value, string description)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result < 1)
        {
            throw new ArgumentException($"The {description} must be a positive whole number.");
        }

        return result;
    }

    private static void RequireCount(string[] args, int expected, string usage)
    {
        if (args.Length != expected)
        {
            throw new ArgumentException($"Usage: {usage}");
        }
    }

    private static (string[]? Args, string FilePath) ExtractFilePath(string[] args)
    {
        var commandArgs = new List<string>();
        string? filePath = null;
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] != "--file")
            {
                commandArgs.Add(args[index]);
                continue;
            }

            if (filePath is not null || index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
            {
                return (null, TournamentFileService.DefaultFilePath);
            }

            filePath = args[++index];
        }

        if (filePath is null
            && commandArgs.Count == 2
            && commandArgs[0] is "save" or "load")
        {
            filePath = commandArgs[1];
            commandArgs.RemoveAt(1);
        }

        return (commandArgs.ToArray(), filePath ?? TournamentFileService.DefaultFilePath);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Tournament Manager CLI");
        Console.WriteLine("Usage: dotnet run -- <command> [arguments] [--file <path>]");
        Console.WriteLine();
        Console.WriteLine("  create <name> <date>       Create a tournament (does not overwrite an existing file)");
        Console.WriteLine("  info                        Show tournament details");
        Console.WriteLine("  players                     List players");
        Console.WriteLine("  player add <name>           Add a player before the first round");
        Console.WriteLine("  player remove <name>        Remove a player before the first round");
        Console.WriteLine("  round create                Create the next round-robin round");
        Console.WriteLine("  round list                  List rounds and matches");
        Console.WriteLine("  result <round> <match> <a|b|draw>");
        Console.WriteLine("  round close <number>        Close a round after all results are entered");
        Console.WriteLine("  standings                   Show points and games played");
        Console.WriteLine("  save | load                 Save or show the default tournament file");
        Console.WriteLine("  help");
        Console.WriteLine();
        Console.WriteLine($"Default file: {TournamentFileService.DefaultFilePath}");
    }
}
