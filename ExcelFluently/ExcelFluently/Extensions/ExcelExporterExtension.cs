using ExcelFluently.Core;

namespace ExcelFluently.Extensions
{
    public static class ExcelExporterExtension
    {
        public static ExcelExporterService<T> ToExcel<T>(this IEnumerable<T> source)
        {
            return new ExcelExporterService<T>(source);
        }
    }
}
