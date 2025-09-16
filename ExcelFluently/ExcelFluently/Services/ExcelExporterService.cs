using System.Linq.Expressions;
using System.Reflection;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelFluently.Models;
using ExcelFluently.Settings;

namespace ExcelFluently.Services
{
    public class ExcelExporterService<T>
    {
        private IEnumerable<T> _data;
        private List<ColumnConfig<T>> _columns = new();
        private bool _useAutoColumns = true;
        private int _titleRowHeight = 3;
        private TableStyleSettings _tableSettings = new();

        public ExcelExporterService(IEnumerable<T> data)
        {
            _data = data;
        }

        public ExcelExporterService<T> WithColumn(
            Expression<Func<T, object>> expression,
            string? columnName = null,
            string? format = null
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
                    Format = format,
                }
            );
            return this;
        }

        public byte[] ToBytes()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(_tableSettings.SheetName ?? "Sheet1");
                if (_useAutoColumns)
                {
                    AddAutoColumns(worksheet);
                }
                else
                {
                    AddCustomColumns(worksheet);
                }
                ApplyTableStyle(worksheet);

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
            int rowIndex = GetStartRow();
            foreach (var property in properties)
            {
                worksheet.Cell(rowIndex, column++).Value = property.Name;
            }

            int row = rowIndex + 1;
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
            int rowIndex = GetStartRow();
            foreach (var column in _columns)
            {
                worksheet.Cell(rowIndex, columnIndex++).Value = column.Name;
            }
            int row = rowIndex + 1;

            foreach (var item in _data)
            {
                columnIndex = 1;
                foreach (var column in _columns)
                {
                    var type = item?.GetType();
                    var value = column.GetValue(item);
                    var cell = worksheet.Cell(row, columnIndex++);
                    if (column.Format != null && value is DateTime datetime)
                    {
                        cell.Value = datetime.ToString(column.Format);
                        continue;
                    }
                    cell.Value = value?.ToString();
                }
                row++;
                columnIndex = 1;
            }
            worksheet.Columns().AdjustToContents();
        }

        private void ApplyTableStyle(IXLWorksheet worksheet)
        {
            if (!string.IsNullOrEmpty(_tableSettings.Title))
            {
                ApplyTitle(worksheet);
            }

            if (_tableSettings.Theme == XLTableTheme.None)
                return;

            var lastRow = worksheet?.LastRowUsed()?.RowNumber();
            var lastColumn = worksheet?.LastColumnUsed()?.ColumnNumber();
            int firstRow = GetStartRow();

            if (lastRow > 1 && lastColumn > 0)
            {
                var range = worksheet?.Range(firstRow, 1, lastRow ?? 0, lastColumn ?? 0);
                var table = range?.CreateTable();

                if (table == null)
                    return;

                table.Theme = _tableSettings.Theme;
                table.ShowRowStripes = _tableSettings.ShowRowStripes;
                table.ShowColumnStripes = _tableSettings.ShowColumnStripes;
                table.ShowTotalsRow = _tableSettings.ShowTotalsRow;
                table.HeadersRow().Style.Font.FontColor = _tableSettings.HeaderFontColor;
            }
        }

        private void ApplyTitle(IXLWorksheet worksheet)
        {
            var lastColumn = GetDataColumnCount();

            var titleRange = worksheet.Range(1, 1, _titleRowHeight, lastColumn);
            titleRange.Merge();
            titleRange.Value = _tableSettings.Title;

            titleRange.Style.Fill.BackgroundColor = XLColor.White;
            titleRange.Style.Font.FontColor = XLColor.Black;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Font.Bold = true;

            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Fila de separación después del título
            worksheet.Row(_titleRowHeight + 1).Height = 5;
        }

        private int GetDataColumnCount()
        {
            if (_useAutoColumns)
                return typeof(T).GetProperties().Length;
            else
                return _columns.Count;
        }

        private int GetStartRow()
        {
            return string.IsNullOrEmpty(_tableSettings.Title) ? 1 : _titleRowHeight + 2;
        }

        internal void ApplyTableStyle(TableStyleSettings settings)
        {
            _tableSettings = settings;
        }
    }
}
