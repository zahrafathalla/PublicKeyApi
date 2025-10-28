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

        [HttpPost("Register")]
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
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
            };

            await _context.IntegrationClients.AddAsync(client);
            await _context.SaveChangesAsync();

            await _context.IntegrationClientKeys.AddAsync(new IntegrationClientKey
            {
                IntegrationClientId = client.Id,
                PublicKey = publicKey,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new {client.ClientIdentifier, publicKey, privateKey});
        }

        [HttpPost("RegenerateKeys")]
        public async Task<IActionResult> RegenerateKeys([FromBody] RegisterClientRequest request)
        {
            var existingClient = await _context.Set<IntegrationClient>()
                .FirstOrDefaultAsync(c => c.ClientIdentifier == request.ClientIdentifier && c.IsActive && !c.IsDeleted);

            if(existingClient == null)
            {
                return NotFound();
            }

            var (privateKey, publicKey) = CreatKeys();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var integrationClientKeys = await _context.IntegrationClientKeys.Where(x => x.IntegrationClientId == existingClient.Id).ToListAsync();
                foreach (var key in integrationClientKeys)
                {
                    key.IsActive = false;
                    key.LastUpdatedAt = DateTime.UtcNow;
                }

                var newKey = new IntegrationClientKey
                {
                    PublicKey = publicKey,
                    IsActive = true,
                    IntegrationClientId = existingClient.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.AddAsync(newKey);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { existingClient.ClientIdentifier, publicKey, privateKey });
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex);
                throw;
            }

            
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
