using System;
using System.IO;

namespace BCTestTask
{
    public class LogFile
    {
        public string LogFileName
        { get; set; }
        public string LogFilePath
        { get; }
        public string LSN
        { get; set; }
        public LogFile(string _FilePath, string TableName)
        {
            string[] searchingLogFiles = Directory.GetFiles(_FilePath, $"BCTestTaskLog_{DB.GetCDNInstanceFromTable(TableName)}_*.json");
            if (searchingLogFiles.Length == 0)
                LogFileName = Path.Combine(_FilePath, $"BCTestTaskLog_{DB.GetCDNInstanceFromTable(TableName)}_.json");
            else
                LogFileName = searchingLogFiles[0];
            LogFilePath = _FilePath;
            LSN = (Path.GetFileNameWithoutExtension(LogFileName).Split("_")[3]);
        }
        public void Save(string content, string newLSN)
        {
            string[] oldFileName = Path.GetFileNameWithoutExtension(LogFileName).Split("_");
            string oldLSN = oldFileName[3];
            oldFileName[3] = newLSN;
            if (content != "[]")
            {
                if (oldLSN != string.Empty)
                {
                    using (FileStream fileStream = new FileStream(LogFileName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        fileStream.Seek(-1, SeekOrigin.End);
                        if (Convert.ToChar(fileStream.ReadByte()) == ']')
                            fileStream.SetLength(fileStream.Length - 1);
                    }
                    content = content.Replace("[", ",");
                }
                using StreamWriter w = File.AppendText(LogFileName);
                w.Write(content);
                w.Flush();
                w.Close();
            }
            string newFileName = Path.Combine(LogFilePath, String.Concat(String.Join("_", oldFileName), ".json"));
            File.Move(LogFileName, newFileName);
            LSN = newLSN;
            LogFileName = newFileName;
        }
    }
}
