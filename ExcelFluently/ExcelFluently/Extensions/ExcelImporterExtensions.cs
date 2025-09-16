using ExcelFluently.Services;
using ExcelFluently.Settings;
using Microsoft.AspNetCore.Http;

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

        public static ExcelImporterService<T> ImportExcel<T>(
            this IFormFile file,
            Action<ImporterSettings> configure = null
        )
            where T : new()
        {
            var stream = new MemoryStream();
            file.CopyToAsync(stream);

            return new ExcelImporterService<T>(stream, configure);
        }
    }
}
