// See https://aka.ms/new-console-template for more information
using ExcelFluently.Extensions;

new Excel().ExportToCustomColumns();
Console.WriteLine("Hello, World!");

public class Excel
{
    private List<Producto> products = new List<Producto>()
    {
        new Producto()
        {
            Id = 1,
            Nombre = "A",
            Email = "a@a.com",
            FechaCreacion = DateTime.Now,
        },
        new Producto()
        {
            Id = 2,
            Nombre = "B",
            Email = "b@b.com",
            FechaCreacion = DateTime.Now,
        },
        new Producto()
        {
            Id = 3,
            Nombre = "C",
            Email = "c@c.com",
            FechaCreacion = DateTime.Now,
        },
        new Producto()
        {
            Id = 4,
            Nombre = "D",
            Email = "d@d.com",
            FechaCreacion = DateTime.Now,
        },
        new Producto()
        {
            Id = 5,
            Nombre = "E",
            Email = "e@e.com",
            FechaCreacion = DateTime.Now,
        },
    };

    public void ExporToExcel()
    {
        products.ToExcel().ToFile("C:\\Users\\ISP2\\Desktop\\products.xlsx");
    }

    public void ExportToCustomColumns()
    {
        products
            .ToExcel()
            .WithColumn(x => x.Id, "Codigo")
            .WithColumn(x => x.Nombre + " " + x.Email, "Nombre")
            .WithColumn(x => x.FechaCreacion, "Fecha")
            .ToFile("C:\\Users\\ISP2\\Desktop\\products.xlsx");
    }
};

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public DateTime FechaCreacion { get; set; }
}
