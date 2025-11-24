using Hangfire;
using HangfireBasicAuthenticationFilter;
using Raknah;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.RegisterServices(builder);

// Register MqttService as a singleton service
builder.Services.AddSingleton<MqttService>();

// Register MqttService as a Hosted Service
builder.Services.AddHostedService(provider => provider.GetRequiredService<MqttService>());

builder.Services.AddScoped<GateService>();

var app = builder.Build();
if (!builder.Environment.IsDevelopment())
    app.UseExceptionHandler();

app.UseSwagger();
if (app.Environment.IsDevelopment())
    app.UseSwaggerUI();
else
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });


app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization =
    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = app.Configuration.GetValue<string>("HangfireSettings:Username"),
            Pass = app.Configuration.GetValue<string>("HangfireSettings:Password")
        }
    ],
    DashboardTitle = "Raknah Dashboard"
});

app.Run();
