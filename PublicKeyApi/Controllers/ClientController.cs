using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicKeyApi.Data;
using PublicKeyApi.DTOs;
using PublicKeyApi.Entities;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PublicKeyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterClientRequest request)
        {
            var (privateKey, publicKey) = CreatKeys();

            var existingClient = await _context.IntegrationClients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientIdentifier == request.ClientIdentifier);

            if(existingClient !=null)
            {
                return BadRequest("Client with the same identifier already exists.");
            }

            var client = new IntegrationClient
            {
                ClientIdentifier = request.ClientIdentifier,
                Name = request.Name,
                PublicKey = publicKey,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
            };

            await _context.IntegrationClients.AddAsync(client);
            await _context.SaveChangesAsync();

            return Ok(new {client.ClientIdentifier, publicKey, privateKey});
        }

        private (string, string) CreatKeys()
        {
            using var rsa = RSA.Create(2048);
            var privateKEy = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            return (privateKEy, publicKey);
        }
    }
}
