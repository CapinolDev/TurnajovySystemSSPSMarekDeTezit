using System;
using System.Collections.Generic;
using System.Globalization;

namespace TurnajManager;

public class Turnaj
{
    public string Nazev { get; set; }
    public DateTime DatumKonani { get; set; }
    public List<Hrac> HraciVsichni { get; set; }

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
        if (string.IsNullOrWhiteSpace(hrac.Jmeno))
        {
            return false;
        } 
        if (HraciVsichni.Any(h => h.Jmeno.Equals(hrac.Jmeno, StringComparison.OrdinalIgnoreCase) && h.JeAktvivni))
        {
            return false;
        }
        HraciVsichni.Add(hrac);
        Console.WriteLine($"Hráč {hrac.Jmeno} byl přidán do turnaje {Nazev}.");
        return true;
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
