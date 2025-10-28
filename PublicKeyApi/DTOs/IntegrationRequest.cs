using System.ComponentModel.DataAnnotations;

namespace PublicKeyApi.DTOs
{
    public class IntegrationRequest
    {
        [Required]
        public string Action { get; set; } = null!;
    }
}
