using Server.Hubs;
using Server.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(s => true);
    });
});
builder.Services.AddSingleton<IDictionary<string, UserData>>(opt => 
    new Dictionary<string, UserData>());

var app = builder.Build();

app.UseCors();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chat");
});

app.Run();