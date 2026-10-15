namespace TurnajManager;

public static class RozdelovaciSystem
{
    public static List<Zapas> Losovani(List<Hrac> hraci)
    {
        var random = new Random();
        var zapasy = new List<Zapas>();
        var zbyvajici = new List<Hrac>();
        foreach (Hrac h in hraci)
        {
            if (h.JeAktvivni)
            {
                zbyvajici.Add(h);
            }
        }
        while (zbyvajici.Count >= 2)
        {
            int h1 = random.Next(zbyvajici.Count);
            Hrac hrac1 = zbyvajici[h1];
            zbyvajici.RemoveAt(h1);
            
            int h2 = random.Next(zbyvajici.Count);
            Hrac? hrac2 = zbyvajici[h2];
            zbyvajici.RemoveAt(h2);
            zapasy.Add(new Zapas(hrac1, hrac2));
        }

        if (zbyvajici.Count == 1)
        {
            var volny = new Zapas(zbyvajici[0], null);
            volny.Vyherce = zbyvajici[0];
            zapasy.Add(volny);
        }
        return zapasy;
    }
}