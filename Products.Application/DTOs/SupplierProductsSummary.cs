namespace Products.Application.DTOs
{
    public record SupplierProductsSummary
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AveragePrice { get; set; }
        public string HighestPricedProduct { get; set; } = string.Empty;
        public List<CategoryCount> ProductCountByCategory { get; set; } = new List<CategoryCount>();
    }
    public record CategoryCount
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    public record ContactInfoUpdate
    {
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
