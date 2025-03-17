using GoCardless;

namespace YourNamespace;

public class Program
{
    public static async Task Main(string[] args)
    {
        var API_KEY = Environment.GetEnvironmentVariable("GO_CARDLESS_APIKEY");

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.WebHost.UseUrls("http://0.0.0.0:8000/");
        GoCardlessClient client = GoCardlessClient.Create(
                // We recommend storing your access token in an
                // configuration setting for security
                API_KEY,
                // Change me to LIVE when you're ready to go live
                GoCardlessClient.Environment.SANDBOX
            );
        builder.Services.AddSingleton(client);
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}