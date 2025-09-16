namespace ExcelFluently.Models
{
    internal class ColumnConfig<T>
    {
        public string Name { get; set; }
        public Func<T, object> GetValue { get; set; } = _ => default!;

        public string Format { get; set; } = default!;
    }
}
