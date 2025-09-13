using ClosedXML.Excel;

namespace ExcelFluently.Settings
{
    public class TableStyleSettings
    {
        public XLTableTheme Theme { get; set; } = XLTableTheme.TableStyleLight1;
        public bool ShowRowStripes { get; set; } = false;
        public string SheetName { get; set; } = default!;
        public string Title { get; set; } = default!;
        public XLColor HeaderFontColor { get; set; } = XLColor.Black;
        public bool ShowColumnStripes { get; set; } = false;
        public bool ShowTotalsRow { get; set; } = false;
    }
}
