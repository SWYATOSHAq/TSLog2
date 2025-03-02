using System;

namespace TSLog.Common.BO;
/// <summary>
/// Позиция услуги в приходе или расходе.
/// </summary>
public class ServiceLine
{
    public int Number { get; private set; } = -1;
    public Service Service { get; private set; }
    public MeasureUnit Measure { get; private set; }
    public double Quantity { get; private set; }
    public double Price { get; private set; }
    public double Sum
    {
        get
        {
            return Price * Quantity;
        }
    }
    public double CalculatedSum { get; private set; }
    public Entity Currency { get; private set; }
    public ServiceLine(int number, Service service, MeasureUnit measure, double quantity, double price, 
        double calculatedSum, Entity currency)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (quantity <= 0)
            throw new ArgumentException("Quantity <= 0!");
        Number = number > 0 ? number : -1;
        Service = service;
        Measure = measure ?? service.DefaultMeasure;
        Quantity = quantity;
        Price = price > 0 ? price  : 0;
        CalculatedSum = calculatedSum > 0 ? calculatedSum : 0;
        Currency = currency;
    }
}
