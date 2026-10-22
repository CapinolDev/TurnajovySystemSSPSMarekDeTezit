using System.Globalization;
using System.Text;

namespace TurnajManager;

public static class TournamentFileService
{
    public const string DefaultFilePath = "tournament.sspst";

    private const string FileHeader = "SSPST";
    private const string FormatVersion = "1";
    private const string TournamentRecord = "TOURNAMENT";
    private const string PlayerRecord = "PLAYER";

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
            || records[0][1] != FormatVersion)
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
        for (var i = 2; i < records.Count; i++)
        {
            var record = records[i];
            if (record.Count != 5 || record[0] != PlayerRecord)
            {
                throw new InvalidDataException($"Player record on line {i + 1} is invalid.");
            }

            if (string.IsNullOrWhiteSpace(record[1]))
            {
                throw new InvalidDataException($"Player name on line {i + 1} is missing.");
            }

            if (!float.TryParse(record[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var points))
            {
                throw new InvalidDataException($"Player points on line {i + 1} are invalid.");
            }

            if (!int.TryParse(record[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var gamesPlayed))
            {
                throw new InvalidDataException($"Games played on line {i + 1} are invalid.");
            }

            if (!Enum.TryParse<Tag>(record[4], ignoreCase: false, out var status)
                || !Enum.IsDefined(status))
            {
                throw new InvalidDataException($"Player status on line {i + 1} is invalid.");
            }

            tournament.HraciVsichni.Add(new Hrac(record[1], points, gamesPlayed)
            {
                Tag = status
            });
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
