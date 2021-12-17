using TSLog.Common.BO;

namespace TSLog.Products.BO
{
    public class ProductCatalog<T> : Catalog<T> where T : Entity
    {
        private readonly Dictionary<string, T> _entitiesInternalCodes = new Dictionary<string, T>();

        public ProductCatalog(string code, string name, string note = "") : base(code, name, note)
        { }

        public new void Add(T entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.Code) && !_entitiesInternalCodes.ContainsKey(entity.Code))
                _entitiesInternalCodes.Add(entity.Code, entity);

            //
            base.Add(entity);
        }

        public T? GetByInternalCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            code = code.Trim();
            return _entitiesInternalCodes.ContainsKey(code) ? _entitiesInternalCodes[code] : null;
        }
    }
}