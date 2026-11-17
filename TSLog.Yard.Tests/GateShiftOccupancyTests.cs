using TSLog.Yard.Reporting;

namespace TSLog.Yard.Tests;

[TestClass]
public class GateShiftOccupancyTests
{
    [TestMethod]
    public void FullShiftUses640WorkingMinutes()
    {
        Assert.AreEqual(100d, GateShiftOccupancy.Calculate([(-60, 800)]));
        Assert.AreEqual(50d, GateShiftOccupancy.Calculate([(0, 390)]));
    }

    [TestMethod]
    public void OverlappingOperationsCountOnce()
    {
        var occupancy = GateShiftOccupancy.Calculate([(70, 150), (60, 110), (90, 100)]);

        Assert.AreEqual(12.5d, occupancy);
    }

    [TestMethod]
    public void PausesAndIntervalsOutsideShiftDoNotCount()
    {
        var occupancy = GateShiftOccupancy.Calculate([
            (-40, 0), (120, 130), (240, 300), (450, 460), (720, 780)
        ]);

        Assert.AreEqual(0d, occupancy);
        Assert.AreEqual(0d, GateShiftOccupancy.Calculate([]));
    }
}
