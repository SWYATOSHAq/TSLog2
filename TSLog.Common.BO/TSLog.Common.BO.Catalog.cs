using System;
using System.Collections.Generic;
using System.Linq;

namespace TSLog.Common.BO
{
    /// <summary>
    /// Каталог справочных объектов
    /// </summary>
    /// <typeparam name="T">Тип справочнико объекта</typeparam>
    public class Catalog<T> where T : Entity
    {
        private readonly IDictionary<long, T> _entitiesIds = new Dictionary<long, T>();
        private readonly IDictionary<string, T> _entitiesCodes = new Dictionary<string, T>();

        /// <summary>
        /// Код
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Примечания и комментарии на естественном языке
        /// </summary>
        public string Note { get; private set; }

        public Catalog(string code, string name, string note = "")
        {
            Code = !string.IsNullOrWhiteSpace(code) ? code.Trim().ToUpper() : string.Empty;
            Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : string.Empty;
            Note = !string.IsNullOrWhiteSpace(note) ? note.Trim() : string.Empty;
        }

        /// <summary>
        /// Добавление сущности
        /// </summary>
        /// <param name="entity">Сущность</param>
        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.Id >= 0 && !_entitiesIds.ContainsKey(entity.Id))
                _entitiesIds.Add(entity.Id, entity);
            if (!string.IsNullOrWhiteSpace(entity.Code) && !_entitiesCodes.ContainsKey(entity.Code))
                _entitiesCodes.Add(entity.Code, entity);
        }

        /// <summary>
        /// Добавление сущностей
        /// </summary>
        /// <param name="entities">Сущности</param>
        public void Add(T[] entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Получение сущности по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Сущность</returns>
        public T this[long id]
        {
            get
            {
                if (id < 0)
                    id = -1;
                return _entitiesIds.ContainsKey(id) ? _entitiesIds[id] : null;
            }
        }

        /// <summary>
        /// Получение сущности по коду
        /// </summary>
        /// <param name="code">Код</param>
        /// <returns>Сущность</returns>
        public T this[string code]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;
                code = code.Trim();
                return _entitiesCodes.ContainsKey(code) ? _entitiesCodes[code] : null;
            }
        }
        /// <summary>
        /// Массив всех объектов каталога
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
                return entitiesCodes.ToArray();
            }
        }
    }
}