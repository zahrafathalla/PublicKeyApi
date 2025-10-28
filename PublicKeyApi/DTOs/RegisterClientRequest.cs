using System.ComponentModel.DataAnnotations;

namespace PublicKeyApi.DTOs
{
    public class RegisterClientRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string ClientIdentifier { get; set; } = string.Empty;
    }
}
