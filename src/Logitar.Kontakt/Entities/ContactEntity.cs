using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Logitar.Kontakt.Entities;

public record ContactEntity
{
  public ObjectId Id { get; set; }

  [BsonElement("contact_id")]
  public Guid ContactId { get; set; } = Guid.NewGuid();
  public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
  public long Version { get; set; } = 1;

  public string? EmailAddress { get; set; }
  public string? PhoneNumber { get; set; }

  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;

  public DateTime? Birthdate { get; set; }
  public string? Gender { get; set; }

  public string? Website { get; set; }
}
