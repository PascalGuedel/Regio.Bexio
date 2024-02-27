using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Regio.Bexio.Domain.Model;
using Regio.Bexio.Exception;
using System.Diagnostics;

namespace Regio.Bexio.Domain;

internal interface IExcelService
{
    IEnumerable<InputInvoice> GetInputInvoices();
}

internal class ExcelService : IExcelService
{
    public IEnumerable<InputInvoice> GetInputInvoices()
    {
        return new List<InputInvoice>
        {
            new()
            {
                Nr = "2024062",
                Datum = "01.02.2024",
                ArEmpfName = "Theater Schlosskeller Fraubrunnen",
                ArEmpfAdr1 = null,
                ArEmpfAdr3 = "3303",
                ArEmpfAdr4 = "Jegensdorf",
                PRNr = "1",
                Pos = "SRE",
                GesamtInklMwst = -268.5m,
                KDNr = "20943",
                StornoVonNr = "2024031",
                Brutto = -248.4m,
                MwstPercent = 8.1,
                Mwst = -20.1m,
                Kundenpreis = -248.4m,
                Netto = -248.4m,
                Waehrung = "CHF",
                ProduktName = "Theater Schlosskeller Fraubrun",
                Kundencode = "SCHLOSSKELLER"
            }
        };

        var inputInvoices = new List<InputInvoice>();
        var excelFilePath = GetExcelFilePath();

        using var doc = SpreadsheetDocument.Open(excelFilePath, false);

        var workbookPart = doc.WorkbookPart;
        var sheet = workbookPart?.Workbook.Descendants<Sheet>().First();

        if (workbookPart == null || sheet == null)
        {
            throw new BexioException("No sheet found in the workbook");
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
        var worksheet = worksheetPart.Worksheet;

        foreach (var row in worksheet.Descendants<Row>())
        {
            var orderedCells = row.Descendants<Cell>().ToList();

            if (orderedCells.Count < 10)
            {
                continue;
            }

            if (GetCellValue(doc, orderedCells[0]) == "Nr.")
            {
                continue;
            }

            var invoice = new InputInvoice
            {
                //Nr = GetCellValue(doc, row[0].Descendants<Cell>()[0])
            };
            foreach (var cell in row.Descendants<Cell>())
            {
                var cellValue = GetCellValue(doc, cell);
                Console.WriteLine(cellValue);
            }

            inputInvoices.Add(invoice);
        }

        return inputInvoices;
    }

    private static string GetExcelFilePath()
    {
        var excelBasePath = $"{Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName)}\\Input";

        if (excelBasePath == null)
        {
            throw new BexioException("Could not determine the base path of the excel file");
        }

        var excelFiles = Directory.EnumerateFiles(excelBasePath, "*.xlsx").ToList();
        if (excelFiles.Count == 0)
        {
            throw new BexioException("No excel files found in the base path");
        }

        if (excelFiles.Count > 1)
        {
            throw new BexioException("More than one excel file found in the base path");
        }

        return excelFiles[0];
    }

    private static string? GetCellValue(SpreadsheetDocument document, CellType cell)
    {
        var stringTablePart = document.WorkbookPart?.SharedStringTablePart;
        var value = cell.CellValue?.InnerXml;

        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            return stringTablePart?.SharedStringTable.ChildElements[int.Parse(value)].InnerText;
        }

        return value;
    }
}


