namespace FluentValidationInterceptor.Dtos
{
    public class ProblemResultDto
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public int Status { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }
        public string Instance { get; set; }
        public string Type { get; set; }
    }
}
