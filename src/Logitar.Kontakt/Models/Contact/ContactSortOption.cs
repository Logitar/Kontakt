namespace Logitar.Kontakt.Models.Contact;

public record ContactSortOption : SortOption
{
  public ContactSortOption() : this(ContactSort.FullName)
  {
  }
  public ContactSortOption(ContactSort field, bool isDescending = false)
    : base(field.ToString(), isDescending)
  {
  }

  public new ContactSort Field { get; set; }
}
