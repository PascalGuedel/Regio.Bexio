using Regio.Bexio.Domain.Model;
using Regio.Bexio.Exception;
using System.Diagnostics;
using OfficeOpenXml;

namespace Regio.Bexio.Domain;

internal interface IExcelService
{
    IEnumerable<InputInvoice> GetInputInvoices();
}

internal class ExcelService : IExcelService
{
    public IEnumerable<InputInvoice> GetInputInvoices()
    {
        var inputInvoices = new List<InputInvoice>();
        var excelFilePath = GetExcelFilePath();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo(excelFilePath));
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows + 1;

        for (var row = 1; row <= rowCount; row++)
        {
            // Skip first 3 rows
            if (row <= 3)
            {
                continue;
            }

            // Skip empty rows
            if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Value?.ToString()))
            {
                break;
            }

            // Skip rows with irrelevant states
            if (IsInvoiceStateIsIrrelevant(worksheet.Cells[row, 8].Value?.ToString()))
            {
                continue;
            }

            var invoice = new InputInvoice
            {
                Nr = worksheet.Cells[row, 1].Value?.ToString(),
                Datum = worksheet.Cells[row, 2].Value?.ToString(),
                ArEmpfName = worksheet.Cells[row, 3].Value?.ToString(),
                ArEmpfAdr1 = worksheet.Cells[row, 4].Value?.ToString(),
                ArEmpfAdr3 = worksheet.Cells[row, 5].Value?.ToString(),
                ArEmpfAdr4 = worksheet.Cells[row, 6].Value?.ToString(),
                PRNr = worksheet.Cells[row, 7].Value?.ToString(),
                Pos = worksheet.Cells[row, 8].Value?.ToString(),
                GesamtInklMwst = decimal.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out var gesamtMwst) ? gesamtMwst : null,
                KDNr = worksheet.Cells[row, 10].Value?.ToString(),
                StornoVonNr = worksheet.Cells[row, 11].Value?.ToString(),
                Brutto = decimal.TryParse(worksheet.Cells[row, 12].Value?.ToString(), out var brutto) ? brutto : null,
                MwstPercent = double.TryParse(worksheet.Cells[row, 13].Value?.ToString(), out var mwstPercent) ? mwstPercent : null,
                Mwst = decimal.TryParse(worksheet.Cells[row, 14].Value?.ToString(), out var mwst) ? mwst : null,
                Kundenpreis = decimal.TryParse(worksheet.Cells[row, 15].Value?.ToString(), out var kundenpreis) ? kundenpreis : null,
                Netto = decimal.TryParse(worksheet.Cells[row, 16].Value?.ToString(), out var netto) ? netto : null,
                Waehrung = worksheet.Cells[row, 17].Value?.ToString(),
                ProduktName = worksheet.Cells[row, 18].Value?.ToString(),
                Kundencode = worksheet.Cells[row, 19].Value?.ToString()
            };

            inputInvoices.Add(invoice);
        }

        return inputInvoices;
    }

    private static bool IsInvoiceStateIsIrrelevant(string? invoiceState)
    {
        if (invoiceState == null)
        {
            return true;
        }

        var irrelevantStates = new List<string>
        {
            "SRE",
            "STO",
            "0"
        };

        return irrelevantStates.Contains(invoiceState);
    }


    //public IEnumerable<InputInvoice> GetInputInvoices()
    //{
    //    //return new List<InputInvoice>
    //    //{
    //    //    new()
    //    //    {
    //    //        Nr = "2024062",
    //    //        Datum = "01.02.2024",
    //    //        ArEmpfName = "Theater Schlosskeller Fraubrunnen",
    //    //        ArEmpfAdr1 = null,
    //    //        ArEmpfAdr3 = "3303",
    //    //        ArEmpfAdr4 = "Jegensdorf",
    //    //        PRNr = "1",
    //    //        Pos = "SRE",
    //    //        GesamtInklMwst = -268.5m,
    //    //        KDNr = "20943",
    //    //        StornoVonNr = "2024031",
    //    //        Brutto = -248.4m,
    //    //        MwstPercent = 8.1,
    //    //        Mwst = -20.1m,
    //    //        Kundenpreis = -248.4m,
    //    //        Netto = -248.4m,
    //    //        Waehrung = "CHF",
    //    //        ProduktName = "Theater Schlosskeller Fraubrun",
    //    //        Kundencode = "SCHLOSSKELLER"
    //    //    }
    //    //};

    //    var inputInvoices = new List<InputInvoice>();
    //    var excelFilePath = GetExcelFilePath();

    //    using var doc = SpreadsheetDocument.Open(excelFilePath, false);

    //    var workbookPart = doc.WorkbookPart;
    //    var sheet = workbookPart?.Workbook.Descendants<Sheet>().First();

    //    if (workbookPart == null || sheet == null)
    //    {
    //        throw new BexioException("No sheet found in the workbook");
    //    }

    //    var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
    //    var worksheet = worksheetPart.Worksheet;

    //    foreach (var row in worksheet.Descendants<Row>())
    //    {
    //        var orderedCells = row.Elements<Cell>().ToList();

    //        if (orderedCells.Count() < 10)
    //        {
    //            continue;
    //        }

    //        if (GetCellValue(doc, orderedCells[0]) == "Nr.")
    //        {
    //            continue;
    //        }

    //        var invoice = new InputInvoice
    //        {
    //            Nr = GetCellValue(doc, orderedCells[0]),
    //            Datum = GetCellValue(doc, orderedCells[1]),
    //            ArEmpfName = GetCellValue(doc, orderedCells[2]),
    //            ArEmpfAdr1 = GetCellValue(doc, orderedCells[3]),
    //            ArEmpfAdr3 = GetCellValue(doc, orderedCells[4]),
    //            ArEmpfAdr4 = GetCellValue(doc, orderedCells[5]),
    //            PRNr = GetCellValue(doc, orderedCells[6]),
    //            Pos = GetCellValue(doc, orderedCells[7]),
    //            GesamtInklMwst = decimal.TryParse(GetCellValue(doc, orderedCells[8]), out var gesamtMwst) ? gesamtMwst : null,
    //            KDNr = GetCellValue(doc, orderedCells[9]),
    //            StornoVonNr = GetCellValue(doc, orderedCells[10]),
    //            Brutto = decimal.TryParse(GetCellValue(doc, orderedCells[11]), out var brutto) ? brutto : null,
    //            MwstPercent = double.TryParse(GetCellValue(doc, orderedCells[12]), out var mwstPercent) ? mwstPercent : null,
    //            Mwst = decimal.TryParse(GetCellValue(doc, orderedCells[13]), out var mwst) ? mwst : null,
    //            Kundenpreis = decimal.TryParse(GetCellValue(doc, orderedCells[14]), out var kundenpreis) ? kundenpreis : null,
    //            Netto = decimal.TryParse(GetCellValue(doc, orderedCells[15]), out var netto) ? netto : null,
    //            Waehrung = GetCellValue(doc, orderedCells[16]),
    //            ProduktName = GetCellValue(doc, orderedCells[17]),
    //            Kundencode = GetCellValue(doc, orderedCells[18])
    //        };

    //        inputInvoices.Add(invoice);
    //    }

    //    return inputInvoices;
    //}

    private static string GetExcelFilePath()
    {
        var excelBasePath = $"{Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName)}\\Input";

        if (excelBasePath == null)
        {
            throw new BexioException("Could not determine the base path of the excel file");
        }

        var excelFiles = Directory.EnumerateFiles(excelBasePath, "*.xlsx")
            .Where(f => !Path.GetFileName(f).StartsWith("~$")).ToList();
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
}