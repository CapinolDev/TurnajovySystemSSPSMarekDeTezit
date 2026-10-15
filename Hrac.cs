namespace TurnajManager;
public class Hrac
{
    public string Jmeno;
    public float Body;
    public int GamesPlayed;
    public Tag Tag { get; set; } = Tag.Aktivni;
    public Hrac(string jmeno, float body, int gamesplayed)
    {
        this.Jmeno = jmeno;
        this.Body = body;
        this.GamesPlayed = gamesplayed;
    }   

}