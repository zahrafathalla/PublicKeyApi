namespace PublicKeyApi.Entities
{
    public class IntegrationClient : BaseEntity
    {
        public int Id { get; set; }
        public string ClientIdentifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ICollection<IntegrationClientKey> IntegrationClientKeys = new List<IntegrationClientKey>();
    }
}
