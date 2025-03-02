using System;

namespace TSLog.Common.BO;

/// <summary>
/// Базовый класс для всех бизнес-оъектов.
/// </summary>
public class Entity : IComparable<Entity>
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public long Id { get; protected set; }

    /// <summary>
    /// Код (альтернативный строковый идентификатор).
    /// </summary>
    public string Code { get; protected set; }

    /// <summary>
    /// Название.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Короткое название.
    /// </summary>
    public string ShortName { get; protected set; }

    /// <summary>
    /// Альтернативное название.
    /// </summary>
    public string AltName { get; protected set; }

    /// <summary>
    /// Название для отображения.
    /// </summary>
    public string DisplayName { get; protected set; }

    private string _note = string.Empty;
    /// <summary>
    /// Примечание.
    /// </summary>
    public string Note
    {
        get
        {
            return _note;
        }
        set
        {
            _note = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    /// <summary>
    /// Признак архивной сущности.
    /// </summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Порядок отображения.
    /// </summary>
    public long DisplayOrder { get; private set; }

    public Entity(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "",
        bool isActive = true, long displayOrder = -1, string note = "")
    {
        Id = id >= 0 ? id : -1;
        Code = !string.IsNullOrWhiteSpace(code) ? code.Trim() : string.Empty;
        Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : string.Empty;
        ShortName = !string.IsNullOrWhiteSpace(shortName) ? shortName.Trim() : string.Empty;
        AltName = !string.IsNullOrWhiteSpace(altName) ? altName.Trim() : string.Empty;
        DisplayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : string.Empty;
        Note = !string.IsNullOrWhiteSpace(note) ? note.Trim() : string.Empty;
        IsActive = isActive;
        DisplayOrder = displayOrder > 0 ? displayOrder : -1;
    }
    protected Entity()
    {
        Id = -1;
        Code = string.Empty;
        Name = string.Empty;
        ShortName = string.Empty;
        AltName = string.Empty;
        DisplayName = string.Empty;
        _note = string.Empty;
        IsActive = true;
        DisplayOrder = -1;
    }
    /// <summary>
    /// Обновляет атрибуты текущей сущности атрибутами другой сущности.
    /// </summary>
    /// <param name="entity">Другая сущность.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Update(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.Id != Id || entity.Code != Code)
            return;
        Name = !string.IsNullOrWhiteSpace(entity.Name) ? entity.Name.Trim() : string.Empty;
        ShortName = !string.IsNullOrWhiteSpace(entity.ShortName) ? entity.ShortName.Trim() : string.Empty;
        AltName = !string.IsNullOrWhiteSpace(entity.AltName) ? entity.AltName.Trim() : string.Empty;
        DisplayName = !string.IsNullOrWhiteSpace(entity.DisplayName) ? entity.DisplayName.Trim() : string.Empty;
        Note = !string.IsNullOrWhiteSpace(entity.Note) ? entity.Note.Trim() : string.Empty;
        IsActive = entity.IsActive;
        DisplayOrder = entity.DisplayOrder > 0 ? entity.DisplayOrder : -1;
    }
    private static Entity _null = null;

    /// <summary>
    /// Null object.
    /// </summary>
    public static Entity Null
    {
        get
        {
            _null ??= new Entity(-1, "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, "NULL");
            return _null;
        }
    }
    private static Entity _empty = null;

    /// <summary>
    /// Empty object.
    /// </summary>
    public static Entity Empty
    {
        get
        {
            _empty ??= new Entity();
            return _empty;
        }
    }

    public int CompareTo(Entity other)
    {
        return DisplayOrder.CompareTo(other.DisplayOrder);
    }
}
