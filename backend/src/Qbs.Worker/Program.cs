using Qbs.Infrastructure.DependencyInjection;
using Qbs.Infrastructure.Processing;
using Qbs.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);
var controlled = builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Development:Controlled");
builder.Services.AddStudio(builder.Configuration, controlled, builder.Environment.EnvironmentName);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JobProcessor>();
builder.Services.AddHostedService<ProcessingService>();
using var host = builder.Build();
await using (var scope = host.Services.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<IStudioDatabase>().Verify();
await host.RunAsync();
