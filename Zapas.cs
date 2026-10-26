namespace TurnajManager;

public enum VysledekZapasu
{
    WinA,
    WinB,
    Remiza
}

public class Zapas
{
    private Kolo? _kolo;

    public Hrac Hrac1 { get; set;}
    public Hrac? Hrac2 {get; set;}
    public VysledekZapasu? Vysledek { get; private set; }
    public Hrac? Vyherce => Vysledek switch
    {
        VysledekZapasu.WinA => Hrac1,
        VysledekZapasu.WinB => Hrac2,
        _ => null
    };

    public Zapas(Hrac hrac1, Hrac? hrac2)
    {
        this.Hrac1 = hrac1;
        this.Hrac2 = hrac2;
    }

    internal bool JePrirazenoDoKola => _kolo is not null;

    internal void PriraditDoKola(Kolo kolo)
    {
        _kolo = kolo;
    }

    internal void NastavitVysledek(Kolo kolo, VysledekZapasu vysledek)
    {
        if (!ReferenceEquals(_kolo, kolo))
        {
            throw new InvalidOperationException("Only the match's round can change its result.");
        }

        Vysledek = vysledek;
    }
}