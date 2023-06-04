using System.Security.Claims;

namespace TSLog.Authorization
{
    public interface IRightsChecker
    {
        /// <summary>
        /// Загружает права из *.xml файла
        /// </summary>
        /// <param name="fileName">Имя *.xml файла</param>
        public void LoadRights(string fileName);
        /// <summary>
        /// Загружает информацию о партнёрский (клиентских) пользователях из *.xml файла
        /// </summary>
        /// <param name="fileName">Имя *.xml файла</param>
        public void LoadUserPartners(string fileName);

        /// <summary>
        /// Проверяет доступность указанного права (по коду) пользователю
        /// </summary>
        /// <param name="right">Код права</param>
        /// <param name="user">Пользователь</param>
        /// <returns>Индикатор доступности права пользователю</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool CheckRight(string right, ClaimsPrincipal user);
        /// <summary>
        /// Проверяет доступность нескольких прав (по кодам прав) пользователю
        /// </summary>
        /// <param name="rights">Массив кодов прав</param>
        /// <param name="user">Пользователь</param>
        /// <returns>Индикатор доступности ХОТЯ БЫ ОДНОГО права (из массива) пользователю</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool CheckRights(string[] rights, ClaimsPrincipal user);
        /// <summary>
        /// Возвращает идентификатор партнёра (клиента) по имени пользователя
        /// </summary>
        /// <param name="userLogin">Имя пользователя</param>
        /// <returns>Идентификатор партнёра (клиента)</returns>
        /// <exception cref="ArgumentException"></exception>
        public string GetPartnerIdByUser(string userLogin);
        /// <summary>
        /// Возвращает идентификаторы партнёров (мандантов) по имени пользователя (тех мандантов, с которыми может работать указанный пользователь)
        /// </summary>
        /// <param name="userLogin">Имя пользователя</param>
        /// <returns>Идентификаторы мандантов</returns>
        /// <exception cref="ArgumentException"></exception>
        public HashSet<string> GetPartnersIdsByUser(string userLogin);
    }
}
