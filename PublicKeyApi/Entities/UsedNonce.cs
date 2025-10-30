using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicKeyApi.Entities
{
    [PrimaryKey(nameof(ClientId), nameof(Value))]
    public class UsedNonce : BaseEntity
    {
        public int ClientId { get; set; }
        [ForeignKey(nameof(ClientId))]
        public IntegrationClient IntegrationClient { get; set; } = null!;
        public string Value { get; set; } = string.Empty;
    }
}
