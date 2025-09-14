namespace ExcelFluently.Models
{
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public object InvalidValue { get; set; } = string.Empty;
    }
}
