namespace Logitar.Kontakt.Models;

public record TextSearch
{
  public IEnumerable<SearchTerm> Terms { get; set; } = Enumerable.Empty<SearchTerm>();
  public QueryOperator Operator { get; set; } = QueryOperator.And;
}
