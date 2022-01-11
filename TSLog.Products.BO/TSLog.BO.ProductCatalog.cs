using TSLog.Common.BO;

namespace TSLog.Products.BO
{
    /// <summary>
    /// Каталог товаров
    /// </summary>
    /// <typeparam name="T">Товар</typeparam>
    public class ProductCatalog<T> : Catalog<T> where T : Product
    {
        private readonly Dictionary<string, T> _entitiesInternalCodes = new();

        public ProductCatalog(string code, string name, string note = "") : base(code, name, note)
        { }

        public new void Add(T product)
        {
            if (!string.IsNullOrWhiteSpace(product.Code) && !_entitiesInternalCodes.ContainsKey(product.Code))
                _entitiesInternalCodes.Add(product.Code, product);

            //
            base.Add(product);
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