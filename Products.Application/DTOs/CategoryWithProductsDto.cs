namespace Products.Application.DTOs
{
    public record CategoryWithProductsDto : CategoryDto
    {
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}
