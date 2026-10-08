using System;
using System.Collections.Generic;
namespace TurnajManager;

public class Turnaj
{
    public string Nazev { get; set; }
    public DateTime Datum { get; set; }
    public List<Hrac> Hraci { get; set; }

    public Turnaj(string nazev, DateTime datum)
    {
        Nazev = nazev;
        Datum = datum;
        Hraci = new List<Hrac>();
    }

    public void PridatHrace(Hrac hrac)
    {
        Hraci.Add(hrac);
        Console.WriteLine($"Hráč {hrac.Jmeno} byl přidán do turnaje {Nazev}.");
    }

    public void VypsatHrace()
    {
        Console.WriteLine($"Turnaj: {Nazev} ({Datum.ToShortDateString()})");
        foreach (var h in Hraci)
        {
            Console.WriteLine($" - {h.Jmeno}");
        }
    }
}
