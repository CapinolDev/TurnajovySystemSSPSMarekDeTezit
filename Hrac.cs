namespace TurnajManager;
public class Hrac
{
    public string Jmeno;
    public float Body = 0;
    public int HryCelkemHrane;
    public Tag Tag { get; set; } = Tag.Aktivni;
    public bool JeAktvivni => Tag == Tag.Aktivni;
    public Hrac(string jmeno, float body, int hryCelkemHrane)
    {
        this.Jmeno = jmeno;
        this.Body = body;
        this.HryCelkemHrane = hryCelkemHrane;
    }   

}