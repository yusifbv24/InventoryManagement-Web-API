namespace InventoryManagement.WebApp.Services
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}
