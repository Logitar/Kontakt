namespace Logitar.Kontakt.Models.Contact;

public record SaveContactPayload
{
  public string? EmailAddress { get; set; }
  public string? PhoneNumber { get; set; }

  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;

  public DateTime? Birthdate { get; set; }
  public string? Gender { get; set; }

  public string? Website { get; set; }
}
