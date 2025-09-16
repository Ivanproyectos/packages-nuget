// See https://aka.ms/new-console-template for more information
using ExcelFluently.Extensions;

new Excel().ExportToExcelWithCustomColumns();
Console.WriteLine("Excel file created successfully.");

//var users = new Excel().ExportToExcelWithCustomStyle();
//Console.WriteLine($"total users: {users.Count}");

public class Excel
{
    private List<User> users = new List<User>()
    {
        new User()
        {
            Id = 1,
            Name = "A",
            Email = "a@a.com",
            DateOfBirth = DateTime.Now,
        },
        new User()
        {
            Id = 2,
            Name = "B",
            Email = "b@b.com",
            DateOfBirth = DateTime.Now,
        },
        new User()
        {
            Id = 3,
            Name = "C",
            Email = "c@c.com",
            DateOfBirth = DateTime.Now,
        },
        new User()
        {
            Id = 4,
            Name = "D",
            Email = "d@d.com",
            DateOfBirth = DateTime.Now,
        },
        new User()
        {
            Id = 5,
            Name = "E",
            Email = "e@e.com",
            DateOfBirth = DateTime.Now,
        },
    };

    // Export users to an Excel file with a custom table style
    public void ExportToExcelWithCustomStyle()
    {
        users
            .ToExcel()
            .WithTableStyle(configure =>
            {
                configure.Theme = ClosedXML.Excel.XLTableTheme.TableStyleMedium9;
                configure.ShowRowStripes = true;
                configure.HeaderFontColor = ClosedXML.Excel.XLColor.Black;
                //configure.ShowTotalsRow = true;
                configure.Title = "Report of Users";
                configure.SheetName = "Users";
            })
            .ToFile("C:\\Users\\ISP2\\Desktop\\users.xlsx");
    }

    public void ExportToExcelWithCustomColumns()
    {
        users
            .ToExcel()
            //.WithTableStyle(configure =>
            //{
            //    configure.Theme = ClosedXML.Excel.XLTableTheme.TableStyleMedium9;
            //    configure.ShowRowStripes = true;
            //    configure.HeaderFontColor = ClosedXML.Excel.XLColor.Black;
            //    configure.Title = "Report of Users";
            //    configure.SheetName = "Users";
            //})
            .WithColumn(x => x.Id, "Codigo")
            .WithColumn(x => x.Name + " " + x.Email, "Name")
            .WithColumn(x => x.DateOfBirth, "Fecha", "yyyy/MM/dd")
            .ToFile("C:\\Users\\ISP2\\Desktop\\users.xlsx");
    }

    public List<User> MapusersFromExcelByNameColumn()
    {
        using var steam = File.OpenRead("C:\\Users\\ISP2\\Desktop\\import users.xlsx");
        var users = steam
            .ImportExcel<User>(configure =>
            {
                configure.SheetName = "Users";
                // configure.DateFormat = "M/d/yyyy";
            })
            .MapColumn(x => x.Name)
            .MapColumn(x => x.Email)
            .MapColumn(x => x.DateOfBirth, "Date Of Birth")
            .MapColumn(x => x.Age)
            .MapColumn(x => x.Salary)
            .ToList();

        return users;
    }

    public List<User> MapusersFromExcelByIndexColumn()
    {
        using var steam = File.OpenRead("C:\\Users\\ISP2\\Desktop\\import users.xlsx");
        var users = steam
            .ImportExcel<User>(configure =>
            {
                configure.SheetName = "Users";
            })
            .MapColumn(x => x.Name, 1)
            .MapColumn(x => x.Email, 2)
            .MapColumn(x => x.DateOfBirth, 3)
            .MapColumn(x => x.Age, 4)
            .MapColumn(x => x.Salary, 5)
            .ToList();

        return users;
    }
};

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public decimal Salary { get; set; }
}
