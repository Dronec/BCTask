using BCTestTask;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace BCTestTaskTests
{
    [TestClass]
    public class BCTests
    {
        private DB SetupDB()
        {
            return new DB("Server=localhost;Database=BCTestTask;Trusted_Connection=True;");
        }
        private LogFile SetupLogFile()
        {
            return new LogFile(@"C:\DEV\", "dbo.SomeTable");
        }
        [TestMethod]
        public void TestTableExsts()
        {
            DB MyDatabase = SetupDB();
            bool ShouldNotExist = MyDatabase.CheckIfTableExists("ShouldNotExist");
            Assert.IsFalse(ShouldNotExist, "This table should not exist");
        }
        [TestMethod]
        public void TestCDCExsts()
        {
            DB MyDatabase = SetupDB();
            string ShouldNotExist = MyDatabase.GetFirstLSN("ShouldNotExist");
            Assert.AreEqual(ShouldNotExist, string.Empty, "This CDC instance should not exist");
        }
        [TestMethod]
        public void TestLastLSN()
        {
            DB MyDatabase = SetupDB();
            string ShouldExist = MyDatabase.GetLastLSN();
            Assert.AreNotEqual(ShouldExist, string.Empty, "This last LSN should exist");
        }
        [TestMethod]
        public void TestFileInit()
        {
            LogFile logFile = SetupLogFile();
            Assert.AreEqual(logFile.LogFileName, @"C:\DEV\BCTestTaskLog_dbo_SomeTable_.json", "The file name should be generated correctly");
            Assert.AreEqual(logFile.LSN, string.Empty, "LSN for the new file should be empty");
        }
    }
}
