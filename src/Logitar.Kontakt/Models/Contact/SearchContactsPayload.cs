namespace Logitar.Kontakt.Models.Contact;

public record SearchContactsPayload : SearchPayload
{
  public new IEnumerable<Guid> Id { get; set; } = Enumerable.Empty<Guid>();

  public DateTime? BornAfter { get; set; }
  public DateTime? BornBefore { get; set; }
  public bool? HasBirthdate { get; set; }

  public string? Gender { get; set; }
  public bool NotGender { get; set; }

  public new IEnumerable<ContactSortOption> Sort { get; set; } = Enumerable.Empty<ContactSortOption>();
}
