using System.IO;
using System.Xml.Serialization;
using CountingLibrary.Main;

namespace CountingLibrary.Core
{
    internal class HardDriveManager
    {
        private DirectoryInfo DirectoryInfo { get; set; }
        private XmlSerializer XmlSerializer { get; set; } = new(typeof(Settings));

        internal HardDriveManager(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
        }

        internal Settings LoadSettings()
        {
            try
            {
                using FileStream fs = new(Info.Default.SettingsFilePath, FileMode.Open);
                Settings? settings = XmlSerializer.Deserialize(fs) as Settings;
                return settings ?? new Settings();
            }
            catch (Exception)
            {
                return new();
            }
        }
        internal void SaveSettings()
        {
            try
            {
                using FileStream fs = new(Info.Default.SettingsFilePath, FileMode.OpenOrCreate);
                XmlSerializer.Serialize(fs, Workspace.WorkspaceInstance.Settings);
            }
            catch (Exception)
            {
                //log
            }
        }
        internal string[] GetFiles()
        {
            try
            {
                return Directory.GetFiles(DirectoryInfo.FullName, "*.*", new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true }).Where(x => Info.Default.FileExtensions.Contains(Path.GetExtension(x).ToLower())).Select(x => x).ToArray();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }
    }
}