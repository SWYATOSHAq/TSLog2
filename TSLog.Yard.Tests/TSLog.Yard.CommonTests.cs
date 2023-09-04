using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSLog.Common;

namespace TSLog.Yard.Tests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void DateParsing()
        {
            // "M/d/yyyy hh:mm:ss tt".
            DateTime trueDate = new DateTime(1985, 4, 16, 18, 12, 53, 0);

            bool result = Utils.DateFromString("4/16/1985 06:12:53 PM", out DateTime date, out string message);
            Assert.IsTrue(result && string.IsNullOrWhiteSpace(message) && date == trueDate);

            //"dd.MM.yyyy HH:mm:ss".
            result = Utils.DateFromString("16.04.1985 18:12:53", out date, out message);
            Assert.IsTrue(result && string.IsNullOrWhiteSpace(message) && date == trueDate);
        }
    }
}
