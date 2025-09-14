using ExcelFluently.Services;
using ExcelFluently.Settings;

namespace ExcelFluently.Extensions
{
    public static class ExcelImporterExtensions
    {
        public static ExcelImporterService<T> ImportExcel<T>(
            this Stream stream,
            Action<ImporterSettings> configure = null
        )
            where T : new()
        {
            return new ExcelImporterService<T>(stream, configure);
        }

        public static ExcelImporterService<T> ImportExcel<T>(
            this byte[] bytes,
            Action<ImporterSettings> configure = null
        )
            where T : new()
        {
            return new ExcelImporterService<T>(new MemoryStream(bytes), configure);
        }
    }
}
