namespace PublicKeyApi.Entities
{
    public class IntegrationClient
    {
        public int Id { get; set; }
        public string ClientIdentifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
