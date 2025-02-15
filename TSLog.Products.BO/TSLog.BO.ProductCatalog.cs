using TSLog.Common.BO;

namespace TSLog.Products.BO;

/// <summary>
/// Каталог товаров.
/// </summary>
public class ProductCatalog() 
    : Catalog<Product>("PRODUCTS", "Справочник товаров", string.Empty)
{
    private readonly Dictionary<string, Product> _entitiesInternalCodes = [];

    public new void Add(Product product)
    {
        if (!string.IsNullOrWhiteSpace(product.Code) && !_entitiesInternalCodes.ContainsKey(product.Code))
            _entitiesInternalCodes.Add(product.Code, product);
        base.Add(product);
    }

    public Product? GetByInternalCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        code = code.Trim();
        return _entitiesInternalCodes.TryGetValue(code, out Product? value) ? value : null;
    }
}
