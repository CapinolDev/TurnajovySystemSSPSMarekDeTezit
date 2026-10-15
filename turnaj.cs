using System;
using System.Collections.Generic;
namespace TurnajManager;

public class Turnaj
{
    public string Nazev { get; set; }
    public DateTime DatumKonani { get; set; }
    public List<Hrac> HraciVsichni { get; set; }

    public Turnaj(string nazev, string datumKonaniVeStringu)// priklady vyvolani: "15.10.1919" NEBO: "U32", "2026-10-15 14:30:00"; zminit jak vyvolat v consolovy komunikaci s poradatelem
    {
        Nazev = nazev;
        DatumKonani = DateTime.Parse(datumKonaniVeStringu);
        HraciVsichni = new List<Hrac>();
    }

    public void PridatHrace(Hrac hrac)
    {
        HraciVsichni.Add(hrac);
        Console.WriteLine($"Hráč {hrac.Jmeno} byl přidán do turnaje {Nazev}.");
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
