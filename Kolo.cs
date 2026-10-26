namespace TurnajManager;

public class Kolo
{
    private readonly List<Zapas> _zapasy;

    public IReadOnlyList<Zapas> Zapasy => _zapasy.AsReadOnly();
    public bool JeUzavrene { get; private set; }

    public Kolo(IEnumerable<Zapas> zapasy)
    {
        ArgumentNullException.ThrowIfNull(zapasy);
        _zapasy = zapasy.ToList();

        if (_zapasy.Any(zapas => zapas is null))
        {
            throw new ArgumentException("A round cannot contain a null match.", nameof(zapasy));
        }

        if (_zapasy.Distinct().Count() != _zapasy.Count)
        {
            throw new ArgumentException("A round cannot contain the same match more than once.", nameof(zapasy));
        }

        if (_zapasy.Any(zapas => zapas.JePrirazenoDoKola))
        {
            throw new ArgumentException("A match can belong to only one round.", nameof(zapasy));
        }

        foreach (var zapas in _zapasy)
        {
            zapas.PriraditDoKola(this);
        }
    }

    public void ZadatVysledek(Zapas zapas, VysledekZapasu vysledek)
    {
        ArgumentNullException.ThrowIfNull(zapas);

        if (JeUzavrene)
        {
            throw new InvalidOperationException("The round is closed; match results can no longer be changed.");
        }

        if (!_zapasy.Contains(zapas))
        {
            throw new ArgumentException("The match does not belong to this round.", nameof(zapas));
        }

        if (!Enum.IsDefined(vysledek))
        {
            throw new ArgumentOutOfRangeException(nameof(vysledek), vysledek, "The match result is not supported.");
        }

        if (zapas.Hrac2 is null && vysledek != VysledekZapasu.WinA)
        {
            throw new ArgumentException("A match without a second player can only be won by player A.", nameof(vysledek));
        }

        zapas.NastavitVysledek(this, vysledek);
    }

    public void Uzavrit()
    {
        if (_zapasy.Any(zapas => zapas.Vysledek is null))
        {
            throw new InvalidOperationException("Every match must have a result before the round can be closed.");
        }

        JeUzavrene = true;
    }
}
