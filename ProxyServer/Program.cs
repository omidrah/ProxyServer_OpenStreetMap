using Polly;
using Polly.Extensions.Http;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient("OSMClient").ConfigureHttpClient(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("ProxyServer/1.0 (com.rhm@gmail.com)");
}).AddPolicyHandler(GetRetryPolicy()).AddPolicyHandler(GetTimeoutPolicy()); 

builder.Services.AddMemoryCache();  

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();


IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(10); // 10-second timeout
}