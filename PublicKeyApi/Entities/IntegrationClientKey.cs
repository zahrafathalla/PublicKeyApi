using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicKeyApi.Entities
{
    [PrimaryKey(nameof(IntegrationClientId), nameof(PublicKey))]
    public class IntegrationClientKey
    {
        public int IntegrationClientId { get; set; }
        [ForeignKey(nameof(IntegrationClientId))]
        public IntegrationClient IntegrationClient { get; set; } = null!;
        public string PublicKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
