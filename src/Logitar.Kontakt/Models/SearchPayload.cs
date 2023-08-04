namespace Logitar.Kontakt.Models;

public record SearchPayload
{
  public TextSearch Id { get; set; } = new();
  public TextSearch Search { get; set; } = new();

  public IEnumerable<SortOption> Sort { get; set; } = Enumerable.Empty<SortOption>();

  public int Skip { get; set; }
  public int Limit { get; set; }
}
