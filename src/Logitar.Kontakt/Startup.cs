using MongoDB.Driver;
using System.Text.Json.Serialization;

namespace Logitar.Kontakt;

internal class Startup : StartupBase
{
  private const string ConnectionStringKey = "MONGODB_URI";
  private const string DatabaseNameKey = "MONGODB_DATABASE";

  private readonly IConfiguration _configuration;
  private readonly bool _enableOpenApi;

  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
    _enableOpenApi = configuration.GetValue<bool>("EnableOpenApi");
  }

  public override void ConfigureServices(IServiceCollection services)
  {
    base.ConfigureServices(services);

    services.AddControllers()
      .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    if (_enableOpenApi)
    {
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen();
    }

    string connectionString = _configuration.GetValue<string>(ConnectionStringKey)
      ?? throw new InvalidOperationException($"The configuration '{ConnectionStringKey}' could not be found.");
    string databaseName = _configuration.GetValue<string>(DatabaseNameKey)
      ?? throw new InvalidOperationException($"The configuration '{DatabaseNameKey}' could not be found.");
    MongoClient client = new(connectionString);
    services.AddSingleton(client.GetDatabase(databaseName));
  }

  public override void Configure(IApplicationBuilder builder)
  {
    if (_enableOpenApi)
    {
      builder.UseSwagger();
      builder.UseSwaggerUI();
    }

    builder.UseHttpsRedirection();

    if (builder is WebApplication application)
    {
      application.MapControllers();
    }
  }
}
