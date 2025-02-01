namespace TSLog.Links.WebApiForBoxScan.Models;

/// <summary>
/// Настройки подключения (и не только) к БД Oracle.
/// </summary>
public class OracleConnectionSettings
{
    public string? DataSource { get; set; }
    /// <summary>
    /// Имя пользователя БД.
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// Пароль для подключения к БД.
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// Схема с общими объектами БД.
    /// </summary>
    public string? CommonSchema { get; set; }
    /// <summary>
    /// Схема БД с объектами WMS.
    /// </summary>
    public string? WmsSchema { get; set; }
    /// <summary>
    /// Схема БД с объектами обслуживания SSCC и кросс-докинга.
    /// </summary>
    public string? SsccSchema { get; set; }
    /// <summary>
    /// Схема БД с объектами учёта персонала.
    /// </summary>
    public string? StaffSchema { get; set; }
    /// <summary>
    /// Схема БД с объектами юр. лиц.
    /// </summary>
    public string? LegalEntitiesSchema { get; set; }
    /// <summary>
    /// Схема БД с объектами партнёров (клиентов).
    /// </summary>
    public string? PartnersSchema { get; set; }
}
