using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicKeyApi.Data;
using PublicKeyApi.DTOs;
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
            var existingClient = _context.IntegrationClients.AsNoTracking()
                .FirstOrDefault(c => c.ClientIdentifier == clientIdentifier && c.IsActive && !c.IsDeleted);

            if(existingClient == null)
            {
                return Unauthorized("Invalid client identifier.");
            }

            var signature = Request.Headers["X-Signature"].ToString();

            if(string.IsNullOrEmpty(signature))
            {
                return BadRequest("Missing signature header.");
            }

            if(!VerifySignature(request, existingClient.PublicKey, signature))
            {
                return Unauthorized("Invalid signature.");
            }

            return Ok("Integration API is running.");
        }

        private bool VerifySignature(IntegrationRequest data, string publicKey, string signature)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            byte[] dataBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

       
        //[HttpPost("SignWithKey")]
        //public IActionResult SignWithKey()
        //{
        //    string privateKeyBase64 = "MIIEpAIBAAKCAQEAyZcFltJ/0eNaviX2vLR9G0GzQ03w7urN8zH9Oxs14Ik2Iz8vlCHK1Sf4ztAq3cHS5xoLkgKefuZa/uTbEcu5lDWQUIsXugKJPqjg+T7s2RfcaC6/NPi2DuSqbK1p4hM6cmUZ2O+Fyirg/ltu6PNEAOCFuws+pWYh5UwMsblxssG3vPzO3YlFepZ2REAMTI4BJtwdHdiRioXlv5BVhq0KctGNuSfge5cndEyZu/P1x+TuTV/z/aPw7KehK/XWHwegQCcPRNz4lF65qSa8L2OtuA1cUemldHlYzwqmXX1BCSyyPvBNI8pB+MTU1lNPWLh2UDr3W1G/t1LUTFw/5bjbkQIDAQABAoIBAD6eF5Fav3NtwLERz8ub8MR3qvw8CJvd+a0SGQu0Dw8478URCnFj8cI2UVXEWZxaaW15rKBlCeB3I0rLwbSMaI+9957dJbiUsxbwlDk3r5Bblg4SfzgwDTUhGEL7tskPmfcQqm+1LwS2Pv8jXZckgToYg9Gu033C9MJp1gOai9OvRYSb4wZEdPpfkIJDWj+3vM2i+zvhNGzlLkVXZapFnq39rUikrROMWoH3HUsskG5xlGdBF7Q6z9R3UZYvTZKc9iF6juqVM5plqOOApbK2WyPILdsLcM0tsm4L6rsVftQgCT/r5xrymIvmOMs0U9kXAH82MSG+0AB2V8o9PqIv47UCgYEA5fiOhoiLVNXJKAOfk5HcdxRvRlqT9bUMQ1KRW3s7QM/WBqUFtkWlW/T3g0XasIf47NeiMMQv9mGSn+MMlPkSiyZVeUDufLtaG0tU40xG21ZyRzYQm2GFLmh2j9HQWpDDkbwzx9I6OoRLGhAHw/GyOV62Zob3sjlSwqACgJDDSF8CgYEA4GgfD7PUnR6GuIFXnXldI3A6jQ8xilcO7dWqkdkZ32wfpB/ELc3zuxhLEwmCxYUcwBgsUIgU0aTpClH9+CxtUfxryDpIEcpwkDeLr5JwnUkhOuuq0EtWDvbQvh1PSgXjFGnfZ6raRNE8SheusHhHpgjPxpDFhr3xRuxcDOE1Ig8CgYA3aTd2RQpFa6mnYZAer4OOkbbqHcMO7gvBYPCzOTMiv7FTMon4zDk2ugS1daxm4qxg7OgglfT0ibgZnEyYzJbiPl9T8whDt6TTdMhEaEmeaerpK6a+ubWsY/FFYAmy+LSWteFIIWh0VxH9eqVUWjVWS3Lpq1WddOBzErjnn3neQwKBgQCprPC3fcCoIFm7DklCD27mCcirua46rMLj/+edqarPbUCrZz19aLj+YUr6lPllAdYQRPbU2V/seCWgoQhH6sep8xNH7RFrKkdcNDORSEeQFahjlaetIRlr7SE+bojyLmtZlwfNqbipyg8s8qUqV3fNSeJYgERqMhpKBxM+xdXX7wKBgQCdpmClgsZYR8mNtoKIy1AbF0wi5uRZVNqeuS2o71Xx21gw+wiu1RJG2AvqSWrzbMZiquLZBpprwFxAN/MfnYnPqBQFQSytM38JCOJnKLNVvfpvz0GauaZ0C/+6m4kJN5maPJTkmxw5USid1TI4uygNEpCbGMdAPaHn66xnj9XQTg==";

        //    string body = "{\"Action\":\"Test\"}"; // JSON string

        //    byte[] dataBytes = Encoding.UTF8.GetBytes(body);
        //    byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

        //    using (var rsa = RSA.Create())
        //    {
        //        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        //        byte[] signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        //        string signatureBase64 = Convert.ToBase64String(signature);

        //        return Ok("Signature: " + signatureBase64);
        //    }
        //}

    }

}

