namespace TurnajManager;

public class Zapas
{
    public Hrac Hrac1 { get; set;}
    public Hrac? Hrac2 {get; set;}
    public Hrac Vyherce { get; set;}
    public Zapas(Hrac hrac1, Hrac? hrac2)
    {
        this.Hrac1 = hrac1;
        this.Hrac2 = hrac2;
    }
}