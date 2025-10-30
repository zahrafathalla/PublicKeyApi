using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicKeyApi.Data;
using PublicKeyApi.DTOs;
using PublicKeyApi.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PublicKeyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public IntegrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Process")]
        public IActionResult Process([FromBody]IntegrationRequest request)
        {
            var clientIdentifier = Request.Headers["X-Client-Identifier"].ToString();
            var timestamp = Request.Headers["X-Timestamp"].ToString();
            var nonce = Request.Headers["X-Nonce"].ToString();

            if (!long.TryParse(timestamp, out long unixSeconds))
            {
                return BadRequest("Invalid timestamp format.");
            }

            DateTimeOffset requestTime;
            try
            {
                requestTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest("Timestamp value is out of valid range.");
            }

            if (DateTimeOffset.UtcNow - requestTime > TimeSpan.FromMinutes(5))
            {
                return Unauthorized("Timestamp is too old.");
            }

            var existingClient = _context.IntegrationClients.AsNoTracking()
                .FirstOrDefault(c => c.ClientIdentifier == clientIdentifier && c.IsActive && !c.IsDeleted);

            if(existingClient == null)
            {
                return Unauthorized("Invalid client identifier.");
            }

            if (_context.UsedNonces.Any(n => n.Value == nonce && n.ClientId == existingClient.Id))
            {
                return Unauthorized("Nonce already used.");
            }

            _context.UsedNonces.Add(new UsedNonce { ClientId = existingClient.Id, Value = nonce, CreatedAt = DateTime.UtcNow });
            _context.SaveChanges();

            var signature = Request.Headers["X-Signature"].ToString();

            if(string.IsNullOrEmpty(signature))
            {
                return BadRequest("Missing signature header.");
            }

            var clientpublicKey = _context.IntegrationClientKeys.FirstOrDefault(x => x.IsActive && x.IntegrationClientId == existingClient.Id);
            if(clientpublicKey == null)
            {
                return Unauthorized("Client public key not found or inactive.");
            }

            var apiPath = HttpContext.Request.Path.Value;

            var rawString = $"ClientId={clientIdentifier}|Timestamp={timestamp}|Nonce={nonce}|Path={apiPath}";

            if (!VerifySignature(rawString, clientpublicKey.PublicKey, signature))
            {
                return Unauthorized("Invalid signature.");
            }

            return Ok("Integration API is running.");
        }

        private bool VerifySignature(string data, string publicKey, string signature)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }



        [HttpPost("SignWithKey")]
        public IActionResult SignWithKey()
        {
            // مفترض القيم دي يبعتهالك الـ frontend أو تولدها وقت التست
            string clientId = "test-355";

            // نستخدم Unix timestamp (ثواني منذ 1970)
            long unixTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string timestamp = unixTimeSeconds.ToString();

            string nonce = Guid.NewGuid().ToString();
            string apiPath = "/api/Integration/Process";

            // المفتاح الخاص (Base64)
            string privateKeyBase64 = "MIIEowIBAAKCAQEAoZH6QqBB1zMfz71vMwx+5zUNQkhq16yxLQRjTsjhS6QvUrShO71smYjg9lopYANK5vn99Nb3BYLHsDZDAEhVDISctiINR3BT4MbISW4iwKjuPB2+GTUu16A/K61vQ7bnk06T9XqvULexn6IKONxbGQGAEy+dG6bkbwcwfNsQuX7bRNlLpiN5itnriNgjCduhGz6SoYfHiGYL/CUWJOYVgwokx90DKo74/0rpQY/mklENU47QAwq95Fnr6yqXAbPdBgwbKh8Slay+vp44BBgtiWJvgh0+dmXZvkHrZPJt8Ma4xJKVm5GFr4EVK63S56BlArot2YerN3AJaStPfYG2sQIDAQABAoIBAQCfcA4l8T8HvcuGlMlG9u3D+vY6knvcmO2+HrZg6JpqqnqIgvcYnLSuTgSxFgf+V0Fy7P9OvVLGfWfQ27sCbF6BG9wJX1D7Tj13crHyxFxHAm0VpcQR3Al9fwTXdS53A+dB1ijr9UVLwfnmLWEo+0pqQrDAPhwrTWXeqpmOnB7E7HjKyJctCFHwY5RArx1p8WiYDLWxOh/z7iu1jhYHNKTkBhBICBufqS7yHQ3/L9gSNSoMELv+bLpGFrVKs3M1hwO0JLmSTZE7S3X/1ezVLcEXbaKgob/8uCFvoAV79n5WjmouFQf2BbfXn+IYiK1SGF3WnKYCNqNMMO2DtNCgjmiBAoGBAMlDBa3RudIX09tQyf5K3YbdgOk8zw4KantcVCbFfmL2/bWg9EEyLoQcOKNPC0GfTPinCPneEiTr7J7bLXNCI2Km360LFN9XXhrepRQ7lV9zIE0WbbSlvpNnwLAeDrX/SwBPcomjF1rzr4379xc7Tdhp0uAt0wwWrMoEHiHtwudTAoGBAM2DZ48tVkcVE3jU2/1hEkvbvZt3yUAWLx78kCktM5aU/6/TFXUtbrZk3E+3PsHSaMxXKvtzuavlfGSeE/LRcpxWpz7oq5BPjCIewRn8VyR0kbTr275jnyzYtzxuznXlhpx6MfJW6zpzhc3l8wE8Ut27wnSwiEPqFn6l5bQAGv1rAoGASZVYiTGJp9eQXLoP8Ao9LibkD+JsrWx/e/TIy5gfWl2Faxea1g9b1G9hAcxPiEGO6cZgUMKxjZA4ZegqmN5Qg6wRUXogunt964MFhf024rv7zlNp7sc+gzRGzd1fcYkSSd3CObJIQrefsuCxeWv0TTB7qfz3EY9kw0N4JN2CCgMCgYBJrS1IOCGxNA4aCH2hldZgWbPc85GzpwBXpIXuLSVMe7g6iXss2g/R1dDhxzj5dXxanXlsUi2jQY9Z5w4RxCLJh4tH01QGVW5QoSPrM/rtt9iwusm5tK9Q/ZSbVsIvpAyhNnGHhr+n4dh7W0/GBqFSmsh42vzPFDHiT4lNAairZwKBgF7Lo3+h+PzWhnonERGgmtfBNIMziAOlyJES/LUxII/TdGoizYU2g2RsxyZU4AyJ8Lzv1wgq9Kot1Mml5h7I4Uog+cf1t5FQTqJvKZqKS98SyqWeFaGYXNF2P8jY27uaHpOGvXusnEQ4R5+me5pF8GPLnvY9tfwWzb4/yUNlxIgY";

            // 1️⃣ بناء النص اللي هيتوقع عليه
            string rawString = $"ClientId={clientId}|Timestamp={timestamp}|Nonce={nonce}|Path={apiPath}";

            // 2️⃣ توقيع النص بالمفتاح الخاص
            byte[] dataBytes = Encoding.UTF8.GetBytes(rawString);
            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                byte[] signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                string signatureBase64 = Convert.ToBase64String(signature);

                // 3️⃣ نرجّع التفاصيل
                return Ok(new
                {
                    clientId,
                    timestamp,
                    nonce,
                    apiPath,
                    rawString,
                    signature = signatureBase64
                });
            }
        }


    }

}

