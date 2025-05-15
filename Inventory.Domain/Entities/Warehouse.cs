namespace Inventory.Domain.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();

        // Business logic methods
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string location, string address,
            string contactPerson, string contactEmail, string contactPhone)
        {
            Name = name;
            Location = location;
            Address = address;
            ContactPerson = contactPerson;
            ContactEmail = contactEmail;
            ContactPhone = contactPhone;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
