namespace Products.Domain.Entities
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();

        // Business logic methods
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public void Update(string name, string contactName, string email, string phone, string address)
        {
            Name = name;
            ContactName = contactName;
            Email = email;
            Phone = phone;
            Address = address;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
