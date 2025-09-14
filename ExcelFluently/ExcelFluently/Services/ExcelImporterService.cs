using System.Linq.Expressions;
using System.Reflection;
using ClosedXML.Excel;
using ExcelFluently.Models;
using ExcelFluently.Settings;

namespace ExcelFluently.Services
{
    public class ExcelImporterService<T>
        where T : new()
    {
        private readonly Stream _excelStream;
        private string _sheetName = "Sheet1";
        private readonly Dictionary<string, PropertyInfo> _columnMappings = new();
        private readonly Dictionary<int, PropertyInfo> _indexMappings = new();

        public ExcelImporterService(Stream stream, Action<ImporterSettings> configure = null)
        {
            var settings = new ImporterSettings();
            configure(settings);
            _sheetName = settings.SheetName;

            _excelStream = stream;
        }

        public ExcelImporterService<T> MapColumn(
            string columnName,
            Expression<Func<T, object>> propertySelector
        )
        {
            var property = GetPropertyInfo(propertySelector);
            if (property == null)
            {
                throw new ArgumentException("Expression must be a property");
            }
            _columnMappings[columnName] = property;
            return this;
        }

        public ExcelImporterService<T> MapColumn(
            int columnIndex,
            Expression<Func<T, object>> propertySelector
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
                var worksheet = workbook.Worksheet(_sheetName);
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
                return Convert.ToInt32(value);

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return Convert.ToDecimal(value);

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return Convert.ToDateTime(value);

            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return Convert.ToBoolean(value);

            return Convert.ChangeType(value, targetType);
        }

        private PropertyInfo? GetPropertyInfo(Expression<Func<T, object>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Expresión debe ser una propiedad");

            return typeof(T).GetProperty(member.Member.Name);
        }
    }
}
