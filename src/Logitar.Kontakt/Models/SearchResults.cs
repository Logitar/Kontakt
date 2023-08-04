namespace Logitar.Kontakt.Models;

public record SearchResults<T>
{
  public SearchResults() : this(Enumerable.Empty<T>())
  {
  }
  public SearchResults(IEnumerable<T> results) : this(results, results.LongCount())
  {
  }
  public SearchResults(long total) : this(Enumerable.Empty<T>(), total)
  {
  }
  public SearchResults(IEnumerable<T> results, long total)
  {
    Results = results;
    Total = total;
  }

  public IEnumerable<T> Results { get; set; }
  public long Total { get; set; }
}
