using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raknah.Extensions;
using System.Security.Claims;

namespace Raknah.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationServices _reservationServices;
        private readonly GateService _gateService;
        public ReservationController(IReservationServices reservationServices, GateService gateService)
        {
            _reservationServices = reservationServices;
            _gateService = gateService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservationAsync([FromBody] ReservationRequest request)
        {

            var result = await _reservationServices.CreateReservationAsync(request, User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return result.IsSuccess ? CreatedAtAction(nameof(GetPendingReservations), new { id = result.Value.Id }, result.Value) : result.ToProblem();
        }

        [HttpPost("cancel/{reservationId}")]
        public async Task<IActionResult> CancelReservationAsync(int reservationId)
        {
            var result = await _reservationServices.CancelReservationAsync(reservationId);
            return result.IsSuccess ? Ok() : result.ToProblem();

        }
        //[HttpPost("OpenGate")]
        //public async Task<IActionResult> OpenGateAsync()
        //{

        //    var result = await _reservationServices.OpenGateAsync(User.GetUserId());
        //    return result.IsSuccess ? Ok() : result.ToProblem();
        //}

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReservations()
        {
            var result = await _reservationServices.GetPendingReservations(User.GetUserId());
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveReservations()
        {
            var result = await _reservationServices.GetActiveReservations(User.GetUserId());
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }

        [HttpGet("ActiveAndPending")]
        public async Task<IActionResult> GetActiveAndPendingReservations()
        {
            var result = await _reservationServices.GetPendingOrActiveReservations(User.GetUserId());
            return result.IsSuccess ? Ok(result.Value) : result.ToProblem();

        }

        [HttpGet("Completed")]
        public async Task<IActionResult> CompletedReservations()
        {
            var result = await _reservationServices.GetCompletedReservations(User.GetUserId());

            return result.IsFailure ? result.ToProblem() : Ok(result.Value);
        }

        [HttpGet("Canceled")]
        public async Task<IActionResult> CanceledReservations()
        {
            var result = await _reservationServices.GetCanceledReservations(User.GetUserId());
            return result.IsFailure ? result.ToProblem() : Ok(result.Value);

        }

        [HttpGet("CanceledAndCompleted")]
        public async Task<IActionResult> CanceledAndCompletedReservations()
        {
            var result = await _reservationServices.GetCompletedAndCanceledReservations(User.GetUserId());
            return result.IsFailure ? result.ToProblem() : Ok(result.Value);
        }

        [HttpPost("open-gate")]
        public async Task<IActionResult> OpenGate()
        {

            var result = await _gateService.OpenGateAsync(User.GetUserId());

            return result.IsSuccess ? Ok() : result.ToProblem();
        }

    }
}
