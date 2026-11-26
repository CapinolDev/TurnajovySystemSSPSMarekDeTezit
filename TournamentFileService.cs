using System.Globalization;
using System.Text;

namespace TurnajManager;

public static class TournamentFileService
{
    public const string DefaultFilePath = "tournament.sspst";

    private const string FileHeader = "SSPST";
    private const string FormatVersion = "2";
    private const string TournamentRecord = "TOURNAMENT";
    private const string PlayerRecord = "PLAYER";
    private const string RoundRecord = "ROUND";
    private const string MatchRecord = "MATCH";

    public static void Save(Turnaj tournament, string filePath = DefaultFilePath)
    {
        ArgumentNullException.ThrowIfNull(tournament);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var content = new StringBuilder()
            .Append(FileHeader).Append(';').AppendLine(FormatVersion)
            .Append(TournamentRecord).Append(';')
            .Append(EscapeField(tournament.Nazev)).Append(';')
            .AppendLine(tournament.DatumKonani.ToString("O", CultureInfo.InvariantCulture));

        foreach (var player in tournament.HraciVsichni)
        {
            content.Append(PlayerRecord).Append(';')
                .Append(EscapeField(player.Jmeno)).Append(';')
                .Append(player.Body.ToString("R", CultureInfo.InvariantCulture)).Append(';')
                .Append(player.HryCelkemHrane.ToString(CultureInfo.InvariantCulture)).Append(';')
                .AppendLine(player.Tag.ToString());
        }

        var playerIndices = tournament.HraciVsichni
            .Select((player, index) => (player, index))
            .ToDictionary(item => item.player, item => item.index);
        foreach (var round in tournament.Kola)
        {
            content.Append(RoundRecord).Append(';').AppendLine(round.JeUzavrene.ToString());
            foreach (var match in round.Zapasy)
            {
                content.Append(MatchRecord).Append(';')
                    .Append(playerIndices[match.Hrac1].ToString(CultureInfo.InvariantCulture)).Append(';')
                    .Append(match.Hrac2 is null
                        ? string.Empty
                        : playerIndices[match.Hrac2].ToString(CultureInfo.InvariantCulture)).Append(';')
                    .AppendLine(match.Vysledek?.ToString() ?? string.Empty);
            }
        }

        File.WriteAllText(filePath, content.ToString(), new UTF8Encoding(false));
    }

    public static Turnaj Load(string filePath = DefaultFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var content = File.ReadAllText(filePath);
        var records = ParseRecords(content);
        if (records.Count < 2
            || records[0].Count != 2
            || records[0][0] != FileHeader
            || records[0][1] is not ("1" or FormatVersion))
        {
            throw new InvalidDataException("The file header is missing or unsupported.");
        }

        var tournamentRecord = records[1];
        if (tournamentRecord.Count != 3 || tournamentRecord[0] != TournamentRecord)
        {
            throw new InvalidDataException("The tournament record is invalid.");
        }

        if (string.IsNullOrWhiteSpace(tournamentRecord[1]))
        {
            throw new InvalidDataException("The tournament name is missing.");
        }

        if (!DateTime.TryParseExact(
                tournamentRecord[2],
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var date))
        {
            throw new InvalidDataException("The tournament date is invalid.");
        }

        var tournament = new Turnaj(tournamentRecord[1], date.ToString("O", CultureInfo.InvariantCulture));
        var players = new List<Hrac>();
        var playerRecordIndex = 2;
        while (playerRecordIndex < records.Count && records[playerRecordIndex][0] == PlayerRecord)
        {
            var record = records[playerRecordIndex];
            if (record.Count != 5 || record[0] != PlayerRecord)
            {
                throw new InvalidDataException($"Player record on line {playerRecordIndex + 1} is invalid.");
            }

            if (string.IsNullOrWhiteSpace(record[1]))
            {
                throw new InvalidDataException($"Player name on line {playerRecordIndex + 1} is missing.");
            }

            if (!float.TryParse(record[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var points))
            {
                throw new InvalidDataException($"Player points on line {playerRecordIndex + 1} are invalid.");
            }

            if (!int.TryParse(record[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var gamesPlayed))
            {
                throw new InvalidDataException($"Games played on line {playerRecordIndex + 1} are invalid.");
            }

            if (!Enum.TryParse<Tag>(record[4], ignoreCase: false, out var status)
                || !Enum.IsDefined(status))
            {
                throw new InvalidDataException($"Player status on line {playerRecordIndex + 1} is invalid.");
            }

            var player = new Hrac(record[1], points, gamesPlayed) { Tag = status };
            tournament.HraciVsichni.Add(player);
            players.Add(player);
            playerRecordIndex++;
        }

        var savedStatistics = players.Select(player => (player.Body, player.HryCelkemHrane)).ToArray();
        if (records[0][1] == "1")
        {
            if (playerRecordIndex != records.Count)
            {
                throw new InvalidDataException($"Record on line {playerRecordIndex + 1} is unsupported.");
            }

            return tournament;
        }

        while (playerRecordIndex < records.Count)
        {
            var roundRecord = records[playerRecordIndex];
            if (roundRecord.Count != 2 || roundRecord[0] != RoundRecord
                || !bool.TryParse(roundRecord[1], out var isClosed))
            {
                throw new InvalidDataException($"Round record on line {playerRecordIndex + 1} is invalid.");
            }

            playerRecordIndex++;
            var matchRecords = new List<(Zapas Match, VysledekZapasu? Result)>();
            while (playerRecordIndex < records.Count && records[playerRecordIndex][0] == MatchRecord)
            {
                var matchRecord = records[playerRecordIndex];
                if (matchRecord.Count != 4
                    || !int.TryParse(matchRecord[1], NumberStyles.None, CultureInfo.InvariantCulture, out var playerAIndex)
                    || playerAIndex < 0 || playerAIndex >= players.Count)
                {
                    throw new InvalidDataException($"Match record on line {playerRecordIndex + 1} is invalid.");
                }

                int? playerBIndex = null;
                if (matchRecord[2].Length > 0)
                {
                    if (!int.TryParse(matchRecord[2], NumberStyles.None, CultureInfo.InvariantCulture, out var parsedPlayerBIndex)
                        || parsedPlayerBIndex < 0
                        || parsedPlayerBIndex >= players.Count
                        || parsedPlayerBIndex == playerAIndex)
                    {
                        throw new InvalidDataException($"Match record on line {playerRecordIndex + 1} is invalid.");
                    }

                    playerBIndex = parsedPlayerBIndex;
                }

                var playerB = playerBIndex.HasValue ? players[playerBIndex.Value] : null;
                VysledekZapasu? result = null;
                if (matchRecord[3].Length > 0)
                {
                    if (!Enum.TryParse<VysledekZapasu>(matchRecord[3], ignoreCase: false, out var parsedResult)
                        || !Enum.IsDefined(parsedResult))
                    {
                        throw new InvalidDataException($"Match result on line {playerRecordIndex + 1} is invalid.");
                    }

                    result = parsedResult;
                }

                if (playerB is null && result is not null and not VysledekZapasu.WinA)
                {
                    throw new InvalidDataException($"Bye result on line {playerRecordIndex + 1} is invalid.");
                }

                matchRecords.Add((new Zapas(players[playerAIndex], playerB), result));
                playerRecordIndex++;
            }

            if (matchRecords.Count == 0)
            {
                throw new InvalidDataException($"Round record on line {playerRecordIndex} has no matches.");
            }

            var round = new Kolo(matchRecords.Select(match => match.Match));
            tournament.Kola.Add(round);
            foreach (var (match, result) in matchRecords)
            {
                if (result is not null)
                {
                    round.ZadatVysledek(match, result.Value);
                }
            }

            if (isClosed)
            {
                try
                {
                    round.Uzavrit();
                }
                catch (InvalidOperationException exception)
                {
                    throw new InvalidDataException($"Closed round {tournament.Kola.Count} has incomplete results.", exception);
                }
            }

            if (playerRecordIndex < records.Count && records[playerRecordIndex][0] != RoundRecord)
            {
                throw new InvalidDataException($"Record on line {playerRecordIndex + 1} is unsupported.");
            }
        }

        for (var index = 0; index < players.Count; index++)
        {
            players[index].Body = savedStatistics[index].Body;
            players[index].HryCelkemHrane = savedStatistics[index].HryCelkemHrane;
        }

        return tournament;
    }

    private static string EscapeField(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static List<List<string>> ParseRecords(string content)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var afterQuote = false;

        for (var i = 0; i < content.Length; i++)
        {
            var character = content[i];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                        afterQuote = true;
                    }
                }
                else
                {
                    field.Append(character);
                }

                continue;
            }

            if (afterQuote && character != ';' && character != '\r' && character != '\n')
            {
                throw new InvalidDataException("Unexpected character after a quoted field.");
            }

            if (character == '"' && field.Length == 0 && !afterQuote)
            {
                inQuotes = true;
            }
            else if (character == ';')
            {
                record.Add(field.ToString());
                field.Clear();
                afterQuote = false;
            }
            else if (character is '\r' or '\n')
            {
                if (character == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                {
                    i++;
                }

                record.Add(field.ToString());
                field.Clear();
                afterQuote = false;
                if (record.Any(value => value.Length > 0) || record.Count > 1)
                {
                    records.Add(record);
                }

                record = new List<string>();
            }
            else
            {
                field.Append(character);
            }
        }

        if (inQuotes)
        {
            throw new InvalidDataException("A quoted field is not closed.");
        }

        if (field.Length > 0 || record.Count > 0 || afterQuote)
        {
            record.Add(field.ToString());
            if (record.Any(value => value.Length > 0) || record.Count > 1)
            {
                records.Add(record);
            }
        }

        return records;
    }
}
