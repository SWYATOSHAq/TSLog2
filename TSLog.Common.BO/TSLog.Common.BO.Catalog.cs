using System;
using System.Collections.Generic;
using System.Linq;

namespace TSLog.Common.BO;

/// <summary>
/// Каталог справочных объектов.
/// </summary>
/// <typeparam name="T">Тип справочнико объекта.</typeparam>
public class Catalog<T> where T : Entity
{
    private readonly Dictionary<long, T> _entitiesIds = [];
    private readonly Dictionary<string, T> _entitiesCodes = [];

    /// <summary>
    /// Код.
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Название.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Примечания и комментарии на естественном языке.
    /// </summary>
    public string Note { get; private set; }

    public Catalog(string code, string name, string note = "")
    {
        Code = !string.IsNullOrWhiteSpace(code) ? code.Trim().ToUpper() : string.Empty;
        Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : string.Empty;
        Note = !string.IsNullOrWhiteSpace(note) ? note.Trim() : string.Empty;
    }

    /// <summary>
    /// Добавление сущности.
    /// </summary>
    /// <param name="entity">Сущность</param>
    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.Id > 0 && !_entitiesIds.ContainsKey(entity.Id))
            _entitiesIds.Add(entity.Id, entity);
        if (!string.IsNullOrWhiteSpace(entity.Code) && !_entitiesCodes.ContainsKey(entity.Code))
            _entitiesCodes.Add(entity.Code, entity);
    }
    /// <summary>
    /// Добавлеие или правка сущности.
    /// </summary>
    /// <param name="entity">Сущность.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddOrUpdate(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var updated = false;
        if (entity.Id > 0)
        {
            if (!_entitiesIds.TryGetValue(entity.Id, out T value))
                _entitiesIds.Add(entity.Id, entity);
            else
            {
                value.Update(entity);
                updated = true;
            }
        }
        if (!string.IsNullOrWhiteSpace(entity.Code))
        {
            var code = entity.Code.Trim();
            if (!_entitiesCodes.TryGetValue(code, out T value))
                _entitiesCodes.Add(code, entity);
            else if (!updated)
                value.Update(entity);
        }
    }

    /// <summary>
    /// Добавление сущностей.
    /// </summary>
    /// <param name="entities">Сущности</param>
    public void Add(T[] entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        foreach (var entity in entities)
            Add(entity);
    }
    /// <summary>
    /// Проверка наличия сущности в контейнере.
    /// </summary>
    /// <param name="entity">сущность.</param>
    /// <returns>Признак наличия.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Contains(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Id > 0 && _entitiesIds.ContainsKey(entity.Id) 
            || !string.IsNullOrWhiteSpace(entity.Code) && _entitiesCodes.ContainsKey(entity.Code);
    }

    /// <summary>
    /// Получение сущности по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Сущность.</returns>
    public T this[long id]
    {
        get
        {
            if (id < 0)
                id = -1;
            return _entitiesIds.TryGetValue(id, out T value) ? value : null;
        }
    }

    /// <summary>
    /// Получение сущности по коду.
    /// </summary>
    /// <param name="code">Код.</param>
    /// <returns>Сущность.</returns>
    public T this[string code]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            code = code.Trim();
            return _entitiesCodes.TryGetValue(code, out T value) ? value : null;
        }
    }
    /// <summary>
    /// Массив всех объектов каталога.
    /// </summary>
    public T[] Entities
    {
        get
        {
            var entitiesIds = _entitiesIds.Values.ToList();
            var entitiesCodes = _entitiesCodes.Values.ToList();
            foreach (var entity in entitiesIds)
                if (!entitiesCodes.Contains(entity))
                    entitiesCodes.Add(entity);
            return [.. entitiesCodes];
        }
    }
}
