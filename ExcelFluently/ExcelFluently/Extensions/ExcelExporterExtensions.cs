using ExcelFluently.Services;

namespace ExcelFluently.Extensions
{
    public static class ExcelExporterExtensions
    {
        public static ExcelExporterService<T> ToExcel<T>(this IEnumerable<T> source)
            where T : new()
        {
            return new ExcelExporterService<T>(source);
        }
    }
}
