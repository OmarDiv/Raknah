namespace Raknah.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MqttService : IHostedService
{
    private readonly ILogger<MqttService> _logger;
    private IMqttClient _mqttClient ;
    private IMqttClientOptions _mqttOptions; 

    public MqttService(ILogger<MqttService> logger)
    {
        _logger = logger;
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("307928259a4445039f803d5eb4f6d3b1.s1.eu.hivemq.cloud", 8883)
            .WithCredentials("raknahapp", "Raknah1@app")
            .WithTls()
            .WithCleanSession(false)
            .Build();

        _mqttClient.UseDisconnectedHandler(async e =>
        {
            _logger.LogWarning("MQTT disconnected. Reconnecting in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await ConnectAsync();
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MQTT service...");
        await ConnectAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT service...");
        return _mqttClient.DisconnectAsync();
    }

    private async Task ConnectAsync()
    {
        try
        {
            var result = await _mqttClient.ConnectAsync(_mqttOptions);
            _logger.LogInformation("MQTT connected successfully: {Result}", result.ResultCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker");
        }
    }

    public IMqttClient GetClient() => _mqttClient;
}
