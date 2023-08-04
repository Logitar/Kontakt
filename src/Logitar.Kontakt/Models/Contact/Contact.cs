using Logitar.Kontakt.Entities;

namespace Logitar.Kontakt.Models.Contact;

public record Contact
{
  public Contact(ContactEntity? contact = null)
  {
    if (contact != null)
    {
      Id = contact.ContactId;
      CreatedOn = contact.CreatedOn;
      UpdatedOn = contact.UpdatedOn;
      Version = contact.Version;

      EmailAddress = contact.EmailAddress;
      PhoneNumber = contact.PhoneNumber;

      FirstName = contact.FirstName;
      LastName = contact.LastName;

      Birthdate = contact.Birthdate;
      Gender = contact.Gender;

      Website = contact.Website;
    }
  }

  public Guid Id { get; set; }
  public DateTime CreatedOn { get; set; }
  public DateTime UpdatedOn { get; set; }
  public long Version { get; set; }

  public string? EmailAddress { get; set; }
  public string? PhoneNumber { get; set; }

  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;

  public DateTime? Birthdate { get; set; }
  public string? Gender { get; set; }

  public string? Website { get; set; }
}
