using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;   // << required for [Authorize]
using KycApi.Data;
using KycApi.Models;
using KycApi.DTOs;



namespace KycApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class KycController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KycController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/kyc
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitKyc([FromBody] KycApplication kyc)
        {
            if (kyc == null)
                return BadRequest("KYC data is null.");

            try
            {
                await _context.KycApplications.AddAsync(kyc);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "KYC submitted successfully", KycId = kyc.Id });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.WriteLine("Error saving KYC: " + ex.Message);

                return StatusCode(500, new { Message = "Error saving KYC", Error = ex.Message });
            }
        }

        // GET: api/kyc/{id}
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var kyc = await _context.KycApplications.FindAsync(id);
            if (kyc == null)
                return NotFound(new { Message = "KYC record not found" });

            return Ok(kyc);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("list")]
        public IActionResult GetPending()
        {
            var pending = _context.KycApplications.Where(k => k.Status == "Pending").ToList();
            return Ok(pending);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveKyc(int id)
        {
            var kyc = await _context.KycApplications.FindAsync(id);

            if (kyc == null)
                return NotFound("KYC not found.");

            if (kyc.Status == "Approved")
                return BadRequest("This KYC is already approved.");

            if (kyc.Status == "Rejected")
                return BadRequest("This KYC has already been rejected.");

            kyc.Status = "Approved";
            kyc.ReviewedAt = DateTime.UtcNow;
            kyc.RejectionReason = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "KYC approved successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectKyc(int id, [FromBody] RejectRequest request)
        {
            var kyc = await _context.KycApplications.FindAsync(id);

            if (kyc == null)
                return NotFound("KYC not found.");

            if (kyc.Status == "Approved")
                return BadRequest("This KYC is already approved.");

            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest("Rejection reason is required.");

            kyc.Status = "Rejected";
            kyc.ReviewedAt = DateTime.UtcNow;
            kyc.RejectionReason = request.Reason;

            await _context.SaveChangesAsync();

            return Ok(new { message = "KYC rejected successfully." });
        }
    }
}
