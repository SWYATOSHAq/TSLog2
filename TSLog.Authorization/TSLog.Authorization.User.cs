namespace TSLog.Authorization;

/// <summary>
/// Пользователь.
/// </summary>
public class User
{
    /// <summary>
    /// Имя.
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Login { get; private set; }   
    /// <summary>
    /// Партёры.
    /// </summary>
    private HashSet<string> _partners = [];
    public User(string name, string login)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is empty.");
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Login is empty.");
        Name = name.Trim();                
        Login = login.Trim().ToLower();
    }
    /// <summary>
    /// Добавить партнёра к пользователю.
    /// </summary>
    /// <param name="partner">Партнёр.</param>
    /// <exception cref="ArgumentException"></exception>
    public void AddPartner(string partner) 
    {
        if (string.IsNullOrWhiteSpace(partner))
            throw new ArgumentException("Partner code is empty.");
        partner = partner.Trim().ToUpper();
        if (_partners.Contains(partner))
            throw new ArgumentException($"User {Name} already contains partner {partner}.");
        _partners.Add(partner);
    }
    /// <summary>
    /// Проверка текущего пользователя на доступ к операциям двора ВСЕХ партнёров (клиентов).
    /// </summary>
    /// <returns></returns>
    public bool CheckAllPartners()
    {
        return _partners.Count != 0;
    }
    /// <summary>
    /// Проверка текущего пользователя на доступ к операциям двора указанного партнёра (клиента).
    /// </summary>
    /// <param name="partner">Код партнёра.</param>
    /// <returns></returns>
    public bool CheckPartner(string partner)
    {
        if (string.IsNullOrWhiteSpace(partner))
            throw new ArgumentException("Partner code is empty.");
        partner = partner.Trim().ToUpper();
        return CheckAllPartners() || _partners.Contains(partner);
    }
    /// <summary>
    /// Возвращает список кодов партнёров, операции двора которых может просматривать данный пользователь.
    /// </summary>
    /// <returns></returns>
    public string[] GetPartners()
    {
        return [.. _partners];
    }
}
