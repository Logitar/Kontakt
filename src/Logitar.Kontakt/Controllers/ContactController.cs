using Logitar.Kontakt.Entities;
using Logitar.Kontakt.Models;
using Logitar.Kontakt.Models.Contact;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Text;

namespace Logitar.Kontakt.Controllers;

[ApiController]
[Route("contacts")]
public class ContactController : ControllerBase
{
  private readonly IMongoCollection<ContactEntity> _contacts;

  public ContactController(IMongoDatabase mongoDatabase)
  {
    _contacts = mongoDatabase.GetCollection<ContactEntity>("contacts");
  }

  [HttpPost]
  public async Task<ActionResult<Contact>> CreateAsync([FromBody] SaveContactPayload payload, CancellationToken cancellationToken)
  {
    ContactEntity contact = new()
    {
      EmailAddress = payload.EmailAddress,
      PhoneNumber = payload.PhoneNumber,
      FirstName = payload.FirstName,
      LastName = payload.LastName,
      Birthdate = payload.Birthdate?.ToUniversalTime(),
      Gender = payload.Gender,
      Website = payload.Website
    };

    await _contacts.InsertOneAsync(contact, new InsertOneOptions(), cancellationToken);

    Contact result = new(contact);
    Uri uri = new($"{Request.Scheme}://{Request.Host}/contacts/{result.Id}");

    return Created(uri, result);
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<Contact>> DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    ContactEntity? contact = await _contacts.AsQueryable()
      .SingleOrDefaultAsync(x => x.ContactId == id, cancellationToken);
    if (contact == null)
    {
      return NotFound();
    }

    FilterDefinition<ContactEntity> filter = Builders<ContactEntity>.Filter.Eq(x => x.Id, contact.Id);
    _ = await _contacts.DeleteOneAsync(filter, cancellationToken);

    Contact result = new(contact);

    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Contact>> ReadAsync(Guid id, CancellationToken cancellationToken)
  {
    ContactEntity? contact = await _contacts.AsQueryable()
      .SingleOrDefaultAsync(x => x.ContactId == id, cancellationToken);
    if (contact == null)
    {
      return NotFound();
    }

    Contact result = new(contact);

    return Ok(result);
  }

  [HttpPost("search")]
  public async Task<ActionResult<SearchResults<Contact>>> SearchAsync([FromBody] SearchContactsPayload payload, CancellationToken cancellationToken)
  {
    List<FilterDefinition<ContactEntity>> filters = new(capacity: 6);

    if (payload.Id.Any())
    {
      filters.Add(Builders<ContactEntity>.Filter.In(x => x.ContactId, payload.Id));
    }

    if (payload.BornAfter.HasValue)
    {
      filters.Add(Builders<ContactEntity>.Filter.Gt(x => x.Birthdate, payload.BornAfter.Value));
    }
    if (payload.BornBefore.HasValue)
    {
      filters.Add(Builders<ContactEntity>.Filter.Lt(x => x.Birthdate, payload.BornBefore.Value));
    }
    if (payload.HasBirthdate.HasValue)
    {
      FilterDefinition<ContactEntity> hasNullBirthdateFilter = Builders<ContactEntity>.Filter.Eq(x => x.Birthdate, null);
      filters.Add(payload.HasBirthdate.Value ? Builders<ContactEntity>.Filter.Not(hasNullBirthdateFilter) : hasNullBirthdateFilter);
    }

    if (payload.Gender != null)
    {
      FilterDefinition<ContactEntity> genderFilter = Builders<ContactEntity>.Filter.Eq(x => x.Gender, payload.Gender);
      filters.Add(payload.NotGender ? Builders<ContactEntity>.Filter.Not(genderFilter) : genderFilter);
    }

    int termCount = payload.Search.Terms.Count();
    if (termCount > 0)
    {
      List<FilterDefinition<ContactEntity>> searchFilters = new(capacity: termCount);
      foreach (SearchTerm term in payload.Search.Terms)
      {
        if (!string.IsNullOrWhiteSpace(term.Value))
        {
          StringBuilder pattern = new();
          if (!term.Value.StartsWith('%'))
          {
            pattern.Append('^');
          }
          pattern.Append(term.Value.Trim('%').Replace('_', '.'));
          if (!term.Value.EndsWith('%'))
          {
            pattern.Append('$');
          }
          BsonRegularExpression regex = new(pattern.ToString(), options: "i");

          searchFilters.Add(Builders<ContactEntity>.Filter.Or(
            Builders<ContactEntity>.Filter.Regex(x => x.EmailAddress, regex),
            Builders<ContactEntity>.Filter.Regex(x => x.PhoneNumber, regex),
            Builders<ContactEntity>.Filter.Regex(x => x.FirstName, regex),
            Builders<ContactEntity>.Filter.Regex(x => x.LastName, regex)
          ));
        }
      }

      if (searchFilters.Any())
      {
        switch (payload.Search.Operator)
        {
          case QueryOperator.And:
            filters.Add(Builders<ContactEntity>.Filter.And(searchFilters.ToArray()));
            break;
          case QueryOperator.Or:
            filters.Add(Builders<ContactEntity>.Filter.Or(searchFilters.ToArray()));
            break;
        }
      }
    }

    FilterDefinition<ContactEntity> filter = filters.Any() ? Builders<ContactEntity>.Filter.And(filters.ToArray())
      : Builders<ContactEntity>.Filter.Empty;
    IFindFluent<ContactEntity, ContactEntity> query = _contacts.Find(filter);

    long total = await query.CountDocumentsAsync(cancellationToken);

    int sortCount = payload.Sort.Count();
    if (sortCount > 0)
    {
      List<SortDefinition<ContactEntity>> sortList = new(capacity: sortCount);
      foreach (ContactSortOption sort in payload.Sort)
      {
        switch (sort.Field)
        {
          case ContactSort.Birthdate:
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.Birthdate)
              : Builders<ContactEntity>.Sort.Ascending(x => x.Birthdate));
            break;
          case ContactSort.EmailAddress:
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.EmailAddress)
              : Builders<ContactEntity>.Sort.Ascending(x => x.EmailAddress));
            break;
          case ContactSort.FullName:
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.FirstName)
              : Builders<ContactEntity>.Sort.Ascending(x => x.FirstName));
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.LastName)
              : Builders<ContactEntity>.Sort.Ascending(x => x.LastName));
            break;
          case ContactSort.LastNameThenFirstName:
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.LastName)
              : Builders<ContactEntity>.Sort.Ascending(x => x.LastName));
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.FirstName)
              : Builders<ContactEntity>.Sort.Ascending(x => x.FirstName));
            break;
          case ContactSort.PhoneNumber:
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.PhoneNumber)
              : Builders<ContactEntity>.Sort.Ascending(x => x.PhoneNumber));
            break;
          case ContactSort.UpdatedOn:
            sortList.Add(sort.IsDescending ? Builders<ContactEntity>.Sort.Descending(x => x.UpdatedOn)
              : Builders<ContactEntity>.Sort.Ascending(x => x.UpdatedOn));
            break;
        }
      }

      if (sortList.Any())
      {
        query = query.Sort(Builders<ContactEntity>.Sort.Combine(sortList.ToArray()));
      }
    }

    if (payload.Skip > 0)
    {
      query = query.Skip(payload.Skip);
    }
    if (payload.Limit > 0)
    {
      query = query.Limit(payload.Limit);
    }

    List<ContactEntity> contacts = await query.ToListAsync(cancellationToken);

    IEnumerable<Contact> results = contacts.Select(contact => new Contact(contact));

    return Ok(new SearchResults<Contact>(results, total));
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<Contact>> UpdateAsync(Guid id, [FromBody] SaveContactPayload payload, CancellationToken cancellationToken)
  {
    ContactEntity? contact = await _contacts.AsQueryable()
      .SingleOrDefaultAsync(x => x.ContactId == id, cancellationToken);
    if (contact == null)
    {
      return NotFound();
    }

    contact.UpdatedOn = DateTime.UtcNow;
    contact.Version++;

    contact.EmailAddress = payload.EmailAddress;
    contact.PhoneNumber = payload.PhoneNumber;
    contact.FirstName = payload.FirstName;
    contact.LastName = payload.LastName;
    contact.Birthdate = payload.Birthdate?.ToUniversalTime();
    contact.Gender = payload.Gender;
    contact.Website = payload.Website;

    FilterDefinition<ContactEntity> filter = Builders<ContactEntity>.Filter.Eq(x => x.Id, contact.Id);
    _ = await _contacts.ReplaceOneAsync(filter, contact, new ReplaceOptions(), cancellationToken);

    Contact result = new(contact);

    return Ok(result);
  }
}
