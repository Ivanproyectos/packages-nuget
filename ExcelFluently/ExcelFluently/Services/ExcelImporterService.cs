using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using ExcelFluently.Models;
using ExcelFluently.Settings;

namespace ExcelFluently.Services
{
    public class ExcelImporterService<T>
        where T : new()
    {
        private readonly Stream _excelStream;
        private ImporterSettings _settings = new ImporterSettings();
        private readonly Dictionary<string, PropertyInfo> _columnMappings = new();
        private readonly Dictionary<int, PropertyInfo> _indexMappings = new();

        public ExcelImporterService(Stream stream, Action<ImporterSettings> configure = null)
        {
            configure(_settings);
            _excelStream = stream;
        }

        public ExcelImporterService<T> MapColumn<TProperty>(
            Expression<Func<T, TProperty>> propertySelector,
            string columnName = null
        )
        {
            var property = GetPropertyInfo(propertySelector);
            if (property == null)
            {
                throw new ArgumentException("Expression must be a property");
            }
            _columnMappings[columnName ?? property.Name] = property;
            return this;
        }

        public ExcelImporterService<T> MapColumn<TProperty>(
            Expression<Func<T, TProperty>> propertySelector,
            int columnIndex
        )
        {
            var property = GetPropertyInfo(propertySelector);
            if (property == null)
            {
                throw new ArgumentException("Expression must be a property");
            }
            _indexMappings[columnIndex] = property;
            return this;
        }

        public List<T> ToList()
        {
            return ToList(out _);
        }

        public List<T> ToList(out List<ImportError> errors)
        {
            errors = new List<ImportError>();
            var result = new List<T>();

            using (var workbook = new XLWorkbook(_excelStream))
            {
                var worksheet = workbook.Worksheet(_settings.SheetName);
                var firstRow = worksheet.FirstRowUsed();

                if (firstRow == null)
                    return result;

                var headers = GetHeaders(firstRow);
                var dataRows = worksheet.RowsUsed().Skip(1);

                foreach (var row in dataRows)
                {
                    var item = new T();
                    bool hasErrors = false;

                    for (int col = 1; col <= headers.Count; col++)
                    {
                        var cell = row.Cell(col);
                        var header = headers.ContainsKey(col) ? headers[col] : null;

                        if (TryMapCellToProperty(item, col, header, cell, out var error))
                        {
                            if (error != null)
                                errors.Add(error);
                            hasErrors = true;
                        }
                    }

                    if (!hasErrors)
                        result.Add(item);
                }
            }

            return result;
        }

        private bool TryMapCellToProperty(
            T item,
            int columnIndex,
            string columnName,
            IXLCell cell,
            out ImportError error
        )
        {
            error = null;

            PropertyInfo property = null;
            if (
                _indexMappings.TryGetValue(columnIndex, out property)
                || (columnName != null && _columnMappings.TryGetValue(columnName, out property))
            )
            {
                if (property != null)
                {
                    try
                    {
                        var value = ConvertValue(cell.Value, property.PropertyType);
                        property.SetValue(item, value);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        error = new ImportError
                        {
                            RowNumber = cell.Address.RowNumber,
                            ColumnName = columnName ?? $"Columna {columnIndex}",
                            ErrorMessage = ex.Message,
                            InvalidValue = cell.Value,
                        };
                        return true;
                    }
                }
            }

            return false;
        }

        private Dictionary<int, string> GetHeaders(IXLRow headerRow)
        {
            var headers = new Dictionary<int, string>();
            foreach (var cell in headerRow.CellsUsed())
            {
                headers[cell.Address.ColumnNumber] = cell.Value.ToString();
            }
            return headers;
        }

        private object ConvertValue(object value, Type targetType)
        {
            if (value == null)
                return null;

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(int) || targetType == typeof(int?))
                return Convert.ToInt32(value?.ToString()?.Trim());

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return Convert.ToDecimal(value?.ToString()?.Trim());

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return ConvertToDateTime(value);

            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return Convert.ToBoolean(value);

            return Convert.ChangeType(value, targetType);
        }

        private DateTime ConvertToDateTime(object value)
        {
            if (value is DateTime datetime)
            {
                return datetime;
            }

            return DateTime.TryParseExact(
                value.ToString(),
                _settings.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate
            )
                ? parsedDate
                : DateTime.MinValue;
        }

        private PropertyInfo? GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Expression must be a property");

            return typeof(T).GetProperty(member.Member.Name);
        }
    }
}
