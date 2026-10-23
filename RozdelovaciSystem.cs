namespace TurnajManager;

public static class RozdelovaciSystem
{
    public static List<Zapas> Losovani(List<Hrac> hraci)
    {
        var random = new Random();
        var aktivni = hraci.Where(h => h.JeAktvivni).ToList();
        var zapasy = new List<Zapas>();
        for (int i = 0; i < aktivni.Count; i++)
        {
            for (int j = i + 1; j < aktivni.Count; j++)
            {
                var a = aktivni[i];
                var b = aktivni[j];
                if (random.Next(2) == 0) (a, b) = (b, a);
                zapasy.Add(new Zapas(a, b));
            }
        }
        return zapasy.OrderBy(_ => random.Next()).ToList();
    }
}