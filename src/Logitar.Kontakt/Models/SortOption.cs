namespace Logitar.Kontakt.Models;

public record SortOption
{
  public SortOption() : this(string.Empty)
  {
  }
  public SortOption(string field, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }

  public string Field { get; set; }
  public bool IsDescending { get; set; }
}
