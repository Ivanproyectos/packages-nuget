using System.Linq.Expressions;
using System.Reflection;
using ClosedXML.Excel;
using ExcelFluently.Models;

namespace ExcelFluently.Core
{
    public class ExcelExporterService<T>
    {
        private IEnumerable<T> _data;
        private string _sheetName = "Sheet1";
        private List<ColumnConfig<T>> _columns = new();
        private bool _useAutoColumns = true;

        public ExcelExporterService(IEnumerable<T> data)
        {
            _data = data;
        }

        public ExcelExporterService<T> WithData(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        public ExcelExporterService<T> WithSheetName(string sheetName)
        {
            _sheetName = sheetName;
            return this;
        }

        public ExcelExporterService<T> WithColumn(
            Expression<Func<T, object>> expression,
            string? columnName = null
        )
        {
            _useAutoColumns = false;
            var member = expression.Body as MemberExpression;
            if (member != null)
            {
                throw new ArgumentException("Member expression is not supported");
            }

            var propertyName = member?.Member.Name;
            var compileExpression = expression.Compile();

            _columns.Add(
                new ColumnConfig<T>
                {
                    Name = columnName ?? propertyName ?? string.Empty,
                    GetValue = compileExpression,
                }
            );
            return this;
        }

        public byte[] ToBytes()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(_sheetName);
                if (_useAutoColumns)
                {
                    AddAutoColumns(worksheet);
                }
                else
                {
                    AddCustomColumns(worksheet);
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }

        public void ToFile(string filePath)
        {
            File.WriteAllBytes(filePath, ToBytes());
        }

        private void AddAutoColumns(IXLWorksheet worksheet)
        {
            PropertyInfo[] properties = typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            );
            int column = 1;
            foreach (var property in properties)
            {
                worksheet.Cell(1, column++).Value = property.Name;
            }

            int row = 2;
            int columnIndex = 1;
            foreach (var item in _data)
            {
                foreach (var property in properties)
                {
                    worksheet.Cell(row, columnIndex++).Value = property?.GetValue(item)?.ToString();
                }
                row++;
                columnIndex = 1;
            }
            worksheet.Columns().AdjustToContents();
        }

        private void AddCustomColumns(IXLWorksheet worksheet)
        {
            int columnIndex = 1;
            foreach (var column in _columns)
            {
                worksheet.Cell(1, columnIndex++).Value = column.Name;
            }
            int row = 2;
            foreach (var item in _data)
            {
                columnIndex = 1;
                foreach (var column in _columns)
                {
                    worksheet.Cell(row, columnIndex++).Value = column.GetValue(item).ToString();
                }
                row++;
                columnIndex = 1;
            }
            worksheet.Columns().AdjustToContents();
        }
    }
}
