using MQTTnet;
using MQTTnet.Client;
using Raknah.Consts.Errors;
using Raknah.Persistence;
using System.Text;

namespace Raknah.Services
{
    public class GateService
    {
        private readonly MqttService _mqttService;
        private readonly ApplicationDbContext _context;

        public GateService(MqttService mqttService, ApplicationDbContext context)
        {
            _mqttService = mqttService;
            _context = context;
        }

        public async Task<Result> OpenGateAsync(string userId)
        {
            var reservation = await _context.Reservations
                .Where(r => r.UserId == userId && r.Status == ReservationStatus.Pending && !r.IsGateOpened)
                .FirstOrDefaultAsync();

            if (reservation == null)
                return Result.Failure(ReservationError.NotFound);

            var mqttClient = _mqttService.GetClient();

            var tcs = new TaskCompletionSource<string>();

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                var msg = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"📩 Received from ESP: {msg}");

                if (msg.Contains("Gate opened", StringComparison.OrdinalIgnoreCase))
                    tcs.TrySetResult("Gate opened");
                else if (msg.Contains("No car", StringComparison.OrdinalIgnoreCase))
                    tcs.TrySetResult("No car detected");
                else
                    tcs.TrySetResult("Error");
            });

            await mqttClient.SubscribeAsync("parking/gate/status");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("parking/gate/open")
                .WithPayload("{\"message\":\"open\"}")
                .WithAtLeastOnceQoS()
                .WithRetainFlag(true)
                .WithMessageExpiryInterval(180)
                .Build();

            await mqttClient.PublishAsync(message);

            var delayTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(tcs.Task, delayTask);

            if (completedTask == delayTask)
                return Result.Failure(ReservationError.EspFaliure);

            var resultMsg = await tcs.Task;

            if (resultMsg == "Gate opened")
            {
                reservation.IsGateOpened = true;
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            else if (resultMsg == "No car detected")
            {
                return Result.Failure(ReservationError.NoCarDetected);
            }
            else
            {
                return Result.Failure(ReservationError.ErrorFromGate);
            }
        }
    }
}
