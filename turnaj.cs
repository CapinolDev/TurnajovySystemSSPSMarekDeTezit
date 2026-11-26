using System;
using System.Collections.Generic;
using System.Globalization;

namespace TurnajManager;

public class Turnaj
{
    public string Nazev { get; set; }
    public DateTime DatumKonani { get; set; }
    public List<Hrac> HraciVsichni { get; set; }
    public List<Kolo> Kola { get; } = [];

    public Turnaj(string nazev, string datumKonaniVeStringu)// priklady vyvolani: "15.10.1919" NEBO: "U32", "2026-10-15 14:30:00"; zminit jak vyvolat v consolovy komunikaci s poradatelem
    {
        Nazev = nazev;
        if (!DateTime.TryParseExact(
                datumKonaniVeStringu,
                ["d.M.yyyy", "dd.MM.yyyy"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date)
            && !DateTime.TryParse(
                datumKonaniVeStringu,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date)
            && !DateTime.TryParse(
                datumKonaniVeStringu,
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out date))
        {
            throw new FormatException($"Invalid tournament date: '{datumKonaniVeStringu}'.");
        }

        DatumKonani = date;
        HraciVsichni = new List<Hrac>();
    }

    public bool PridatHrace(Hrac hrac)
    {
        ArgumentNullException.ThrowIfNull(hrac);
        if (Kola.Count > 0)
        {
            throw new InvalidOperationException("Players cannot be changed after the first round has been created.");
        }

        if (string.IsNullOrWhiteSpace(hrac.Jmeno))
        {
            return false;
        } 
        if (HraciVsichni.Any(h => h.Jmeno.Equals(hrac.Jmeno, StringComparison.OrdinalIgnoreCase) && h.JeAktvivni))
        {
            return false;
        }
        HraciVsichni.Add(hrac);
        return true;
    }

    public void OdebratHrace(string jmeno)
    {
        if (Kola.Count > 0)
        {
            throw new InvalidOperationException("Players cannot be changed after the first round has been created.");
        }

        var hrac = HraciVsichni.FirstOrDefault(
            player => player.Jmeno.Equals(jmeno, StringComparison.OrdinalIgnoreCase));
        if (hrac is null)
        {
            throw new ArgumentException($"Player '{jmeno}' was not found.", nameof(jmeno));
        }

        if (hrac.HryCelkemHrane > 0)
        {
            throw new InvalidOperationException("A player with played games cannot be removed.");
        }

        HraciVsichni.Remove(hrac);
    }

    public Kolo VytvoritDalsiKolo()
    {
        if (Kola.Count > 0 && !Kola[^1].JeUzavrene)
        {
            throw new InvalidOperationException("Close the current round before creating the next one.");
        }

        var hraci = HraciVsichni.Where(hrac => hrac.JeAktvivni).ToList();
        if (hraci.Count < 2)
        {
            throw new InvalidOperationException("At least two active players are required to create a round.");
        }

        var numberOfRounds = hraci.Count % 2 == 0 ? hraci.Count - 1 : hraci.Count;
        if (Kola.Count >= numberOfRounds)
        {
            throw new InvalidOperationException("The round-robin tournament is complete.");
        }

        var rotatingPlayers = new List<Hrac?>(hraci);
        if (rotatingPlayers.Count % 2 != 0)
        {
            rotatingPlayers.Add(null);
        }

        for (var roundIndex = 0; roundIndex < Kola.Count; roundIndex++)
        {
            var lastPlayer = rotatingPlayers[^1];
            for (var index = rotatingPlayers.Count - 1; index > 1; index--)
            {
                rotatingPlayers[index] = rotatingPlayers[index - 1];
            }

            rotatingPlayers[1] = lastPlayer;
        }

        var matches = new List<Zapas>(rotatingPlayers.Count / 2);
        for (var index = 0; index < rotatingPlayers.Count / 2; index++)
        {
            var playerA = rotatingPlayers[index];
            var playerB = rotatingPlayers[rotatingPlayers.Count - 1 - index];
            if (playerA is null)
            {
                matches.Add(new Zapas(playerB!, null));
            }
            else
            {
                matches.Add(new Zapas(playerA, playerB));
            }
        }

        var round = new Kolo(matches);
        Kola.Add(round);
        return round;
    }

    public void VypsatHrace()
    {
        Console.WriteLine($"Turnaj: {Nazev} ({DatumKonani.ToShortDateString()})");
        foreach (var h in HraciVsichni)
        {
            Console.WriteLine($" - {h.Jmeno}");
        }
    }
}
