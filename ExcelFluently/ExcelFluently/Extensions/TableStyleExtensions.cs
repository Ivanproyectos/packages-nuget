using ExcelFluently.Services;
using ExcelFluently.Settings;

namespace ExcelFluently.Extensions
{
    public static class TableStyleExtensions
    {
        public static ExcelExporterService<T> WithTableStyle<T>(
            this ExcelExporterService<T> exporter,
            Action<TableStyleSettings> configure
        )
        {
            var config = new TableStyleSettings();
            configure(config);

            exporter.ApplyTableStyle(config);

            return exporter;
        }
    }
}
