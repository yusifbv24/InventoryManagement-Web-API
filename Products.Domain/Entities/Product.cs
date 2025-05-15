namespace Products.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SKU { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string Dimensions { get; set; } = string.Empty;

        // Business logic methods
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public void UpdateDetails(string name, string description, decimal price, string sku,
            int categoryId, int supplierId, string imageUrl, decimal weight, string dimensions)
        {
            Name = name;
            Description = description;
            Price = price;
            SKU = sku;
            CategoryId = categoryId;
            SupplierId = supplierId;
            ImageUrl = imageUrl;
            Weight = weight;
            Dimensions = dimensions;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
