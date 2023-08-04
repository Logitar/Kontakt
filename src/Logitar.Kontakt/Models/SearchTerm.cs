namespace Logitar.Kontakt.Models;

public record SearchTerm
{
  public SearchTerm() : this(string.Empty)
  {
  }
  public SearchTerm(string value)
  {
    Value = value;
  }

  public string Value { get; set; }
}
