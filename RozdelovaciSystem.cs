namespace TurnajManager;

public static class RozdelovaciSystem
{
    public static List<Zapas> Losovani(List<Hrac> hraci)
    {
        var aktivni = hraci.Where(h => h.JeAktvivni).ToList();
        var zapasy = new List<Zapas>();
        for (int i = 0; i < aktivni.Count; i++)
        {
            for (int j = i + 1; j < aktivni.Count; j++)
            {
                zapasy.Add(new Zapas(aktivni[i], aktivni[j]));
            }
        }
        return zapasy;
    }
}