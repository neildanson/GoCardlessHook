using GoCardless;

namespace YourNamespace;

public class Program
{
    public static async Task Main(string[] args)
    {

        var ALL_ENVS = Environment.GetEnvironmentVariables();
        foreach (var key in ALL_ENVS.Keys)
        {
            Console.WriteLine($"{key} : {ALL_ENVS[key]}");
        }
        Console.WriteLine("#####################################");
        var API_KEY = (string)ALL_ENVS["GO_CARDLESS_APIKEY"];
        var SECRET = (string)ALL_ENVS["GO_CARDLESS_WEBHOOKSECRET"];
        Console.WriteLine($"API_KEY : {API_KEY}, SECRET {SECRET}");

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.WebHost.UseUrls("http://0.0.0.0:8080/");
        
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
        app.UseHttpsRedirection();

        app.MapControllers();

        await app.RunAsync();
    }
}