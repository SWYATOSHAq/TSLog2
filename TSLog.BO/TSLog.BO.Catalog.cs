using System.Collections.Generic;

namespace TSLog.BO
{
    /// <summary>
    /// Каталог справочных объектов
    /// </summary>
    /// <typeparam name="T">Тип справочнико объекта</typeparam>
    public class Catalog<T> where T: Entity
    {
        private IDictionary<long, T> _entitiesIds = null;
        private IDictionary<string, T> _entitiesCodes = null;

        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Note { get; private set; }
        public Catalog(string code, string name, string note = "")
        {
            Code = code;
            Name = name;
            Note = note;
            _entitiesIds = new Dictionary<long, T>();
            _entitiesCodes = new Dictionary<string, T>();
        }

        public void Add(T entity)
        {
            if (entity.Id > 0 && !_entitiesIds.ContainsKey(entity.Id))
                _entitiesIds.Add(entity.Id, entity);
            if (string.IsNullOrWhiteSpace(entity.Code) && !_entitiesCodes.ContainsKey(entity.Code))
                _entitiesCodes.Add(entity.Code, entity);

        }
        public void Add(T[] entities)
        {
            foreach (var entity in entities)
                Add(entity);
        }
        public T this[long id]
        {
            get
            {
                if (id <= 0)
                    id = -1;
                return _entitiesIds.ContainsKey(id) ? _entitiesIds[id] : null;
            }
        }
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
    }
}
