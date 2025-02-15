using TSLog.Links.Sscc;

namespace SsccUnitTests;

[TestClass]
public sealed class SsccTest
{
    [TestMethod]
    public void CheckSscc()
    {
        // 18 цифр (стандартный SSCC).
        Assert.IsTrue(Utils.CheckSscc("123456789123456789", out string? sscc));
        Assert.AreEqual("123456789123456789", sscc);

        // 18 цифр с пробелами на концах.
        Assert.IsTrue(Utils.CheckSscc("    987654321987654321    ", out sscc));
        Assert.AreEqual("987654321987654321", sscc);

        // 20 цифр.
        Assert.IsTrue(Utils.CheckSscc("00123456789123456789", out sscc));
        Assert.AreEqual("00123456789123456789", sscc);

        // 20 цифр с пробелами на концах.
        Assert.IsTrue(Utils.CheckSscc("    00123456789123456789    ", out sscc));
        Assert.AreEqual("00123456789123456789", sscc);

        // 20 цифр, две лидирующие цифры в круглых скобках.
        Assert.IsTrue(Utils.CheckSscc("(00)123456789123456789", out sscc));
        Assert.AreEqual("(00)123456789123456789", sscc);

        // 20 цифр, две лидирующие цифры в круглых скобках. Плюс пробелы на концах.
        Assert.IsTrue(Utils.CheckSscc("    (00)123456789123456789    ", out sscc));
        Assert.AreEqual("(00)123456789123456789", sscc);

        // Длина 17 символов.
        Assert.IsFalse(Utils.CheckSscc("12345678912345678", out sscc));
        Assert.AreEqual(string.Empty, sscc);

        // Длина 19 символов.
        Assert.IsFalse(Utils.CheckSscc("1234567890123456789", out sscc));
        Assert.AreEqual(string.Empty, sscc);

        // Длина 21 символ.
        Assert.IsFalse(Utils.CheckSscc("123456789012345678901", out sscc));
        Assert.AreEqual(string.Empty, sscc);

        // Недопустимый символ в середине.
        Assert.IsFalse(Utils.CheckSscc("123456789.12345678", out sscc));
        Assert.AreEqual(string.Empty, sscc);

        // Пробелы в середине.
        Assert.IsFalse(Utils.CheckSscc("123456789    123456789", out sscc));
        Assert.AreEqual(string.Empty, sscc);

        // Пустая строка.
        Assert.IsFalse(Utils.CheckSscc(string.Empty, out sscc));
        Assert.AreEqual(string.Empty, sscc);

        // Пробелы.
        Assert.IsFalse(Utils.CheckSscc("    ", out sscc));
        Assert.AreEqual(string.Empty, sscc);
    }
}
