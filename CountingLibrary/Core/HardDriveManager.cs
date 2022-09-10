using System.IO;
using System.Text;
using System.Xml.Serialization;
using CountingLibrary.Main;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace CountingLibrary.Core
{
    internal class HardDriveManager
    {
        internal string[] Files { get; set; } = Array.Empty<string>();
        private XmlSerializer XmlSerializer { get; set; } = new(typeof(Settings));
        private PdfDocument PdfDocument { get; set; } = new();

        internal HardDriveManager(DirectoryInfo directoryInfo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Files = GetFiles(directoryInfo);
        }
        internal HardDriveManager(string[] files)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Files = files;
        }

        internal void SaveResult(string fileName, string fontFamily, int fontSize, ProcessingType processingType)
        {
            PdfDocument = new();
            PdfPage pdfPage = PdfDocument.AddPage();
            XGraphics xGraphics = XGraphics.FromPdfPage(pdfPage);
            XFont xFont = new(fontFamily, fontSize);

            switch (processingType)
            {
                case ProcessingType.OneSymbol:
                    xGraphics.DrawString($"Обработанных знаков {Workspace.WorkspaceInstance.SymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 50));
                    xGraphics.DrawString($"Необработанных знаков {Workspace.WorkspaceInstance.WrongSymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 75));
                    xGraphics.DrawString("Знак", xFont, XBrushes.Black, new XPoint(100, 150));
                    xGraphics.DrawString("Процент", xFont, XBrushes.Black, new XPoint(400, 150));
                    break;
                case ProcessingType.TwoSymbols:
                    xGraphics.DrawString($"Обработанных пар знаков {Workspace.WorkspaceInstance.SymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 50));
                    xGraphics.DrawString($"Необработанных пар знаков {Workspace.WorkspaceInstance.WrongSymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 75));
                    xGraphics.DrawString("Два знака", xFont, XBrushes.Black, new XPoint(100, 150));
                    xGraphics.DrawString("Процент", xFont, XBrushes.Black, new XPoint(400, 150));
                    break;
                case ProcessingType.ThreeSymbols:
                    xGraphics.DrawString($"Обработанных троек {Workspace.WorkspaceInstance.SymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 50));
                    xGraphics.DrawString($"Необработанных троек {Workspace.WorkspaceInstance.WrongSymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 75));
                    xGraphics.DrawString("Три знака", xFont, XBrushes.Black, new XPoint(100, 150));
                    xGraphics.DrawString("Процент", xFont, XBrushes.Black, new XPoint(400, 150));
                    break;
                case ProcessingType.Word:
                    xGraphics.DrawString($"Обработанных слов {Workspace.WorkspaceInstance.SymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 50));
                    xGraphics.DrawString($"Необработанных знаков {Workspace.WorkspaceInstance.WrongSymbolsCount}", xFont, XBrushes.Black, new XPoint(100, 75));
                    xGraphics.DrawString("Слово", xFont, XBrushes.Black, new XPoint(100, 150));
                    break;
            }
            xGraphics.DrawString($"Времени прошло {Workspace.WorkspaceInstance.TimeSpent}", xFont, XBrushes.Black, new XPoint(100, 100));
            xGraphics.DrawString("Количество", xFont, XBrushes.Black, new XPoint(250, 150));

            int height = 150;

            if (processingType == ProcessingType.Word)
            {
                xGraphics.DrawString(Workspace.WorkspaceInstance.SymbolInfo.SymbolView, xFont, XBrushes.Black, new XPoint(100, height + 20));
                xGraphics.DrawString(Workspace.WorkspaceInstance.SymbolInfo.Count.ToString(), xFont, XBrushes.Black, new XPoint(250, height + 20));
            }
            else
            {
                for (int i = 0; i < Workspace.WorkspaceInstance.SymbolInfos.Count; i++)
                {

                    xGraphics.DrawString(Workspace.WorkspaceInstance.SymbolInfos[i].SymbolView, xFont, XBrushes.Black, new XPoint(100, height + 20));
                    xGraphics.DrawString(Workspace.WorkspaceInstance.SymbolInfos[i].Count.ToString(), xFont, XBrushes.Black, new XPoint(250, height + 20));
                    xGraphics.DrawString(Workspace.WorkspaceInstance.SymbolInfos[i].PercentView, xFont, XBrushes.Black, new XPoint(400, height + 20));
                    height += 20;
                    if (height + 50 > pdfPage.Height)
                    {
                        height = 0;
                        pdfPage = PdfDocument.AddPage();
                        xGraphics = XGraphics.FromPdfPage(pdfPage);
                    }
                }
            }

            PdfDocument.Save(fileName);
        }
        internal Settings LoadSettings(out bool catchException)
        {
            try
            {
                using FileStream fs = new(Info.Default.SettingsFilePath, FileMode.Open);
                Settings? settings = XmlSerializer.Deserialize(fs) as Settings;
                catchException = false;
                return settings ?? new Settings();
            }
            catch (Exception)
            {
                catchException = true;
                return new();
            }
        }
        internal void SaveSettings()
        {
            bool catchException = true;
            while (catchException)
            {
                try
                {
                    if (!File.Exists(Info.Default.SettingsFilePath))
                        File.Create(Info.Default.SettingsFilePath).Close();
                    using FileStream fs = new(Info.Default.SettingsFilePath, FileMode.Truncate);
                    XmlSerializer.Serialize(fs, Workspace.WorkspaceInstance.Settings);
                    fs.Close();
                    LoadSettings(out catchException);
                }
                catch (Exception)
                {
                    catchException = false;
                }
            }
        }
        private string[] GetFiles(DirectoryInfo directoryInfo)
        {
            try
            {
                return Directory.GetFiles(directoryInfo.FullName, "*.*", new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true }).Where(x => Info.Default.FileExtensions.Contains(Path.GetExtension(x).ToLower())).Select(x => x).ToArray();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }
    }
}