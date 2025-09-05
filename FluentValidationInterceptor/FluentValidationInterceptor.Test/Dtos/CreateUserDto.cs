namespace FluentValidationInterceptor.Test.Dtos
{
    public record class CreateUserDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
    }
}
