namespace Products.Application.DTOs
{
    public record BatchOperationResult
    {
        public List<int> SuccessfulIds { get; set; } = new List<int>();
        public Dictionary<int, string> FailedIds { get; set; } = new Dictionary<int, string>();
        public int TotalCount => SuccessfulIds.Count + FailedIds.Count;
        public int SuccessCount => SuccessfulIds.Count;
        public int FailCount => FailedIds.Count;
    }
}
