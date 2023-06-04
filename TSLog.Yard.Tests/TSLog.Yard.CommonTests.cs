using TSLog.Common;

namespace TSLog.Yard.Tests
{
    public class CommonTests
    {
        [Test]
        public void DateParsing()
        {
            // "M/d/yyyy hh:mm:ss tt".
            var trueDate = new DateTime(1985, 4, 16, 18, 12, 53, 0);

            var result = Utils.DateFromString("4/16/1985 06:12:53 PM", out DateTime date, out string message);
            Assert.That(result && string.IsNullOrWhiteSpace(message) && date == trueDate, Is.True);

            //"dd.MM.yyyy HH:mm:ss".
            result = Utils.DateFromString("16.04.1985 18:12:53", out date, out message);
            Assert.That(result && string.IsNullOrWhiteSpace(message) && date == trueDate, Is.True);
        }
    }
}
