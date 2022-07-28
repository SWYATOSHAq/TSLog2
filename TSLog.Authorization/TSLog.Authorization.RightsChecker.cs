using System.Security.Claims;
using System.Xml;

namespace TSLog.Authorization
{
    /// <summary>
    /// Класс для проверки прав пользователя
    /// </summary>
    public class RightsChecker
    {
        private readonly IDictionary<string, Role> _roles = new Dictionary<string, Role>();
        private readonly IDictionary<string, User> _users = new Dictionary<string, User>();
        private void ParseRole(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            var roleCode = reader.GetAttribute("code");
            if (string.IsNullOrWhiteSpace(roleCode))
                throw new Exception("Role code is empty");
            roleCode = roleCode.Trim().ToUpper();
            var roleName = reader.GetAttribute("name");

            if (string.IsNullOrWhiteSpace(roleName))
                throw new Exception("Role name is empty");
            roleName = roleName.Trim();

            var roleGroupName = reader.GetAttribute("groupName");
            if (string.IsNullOrWhiteSpace(roleGroupName))
                throw new Exception("Role group name is empty");
            roleGroupName = roleGroupName.Trim();

            if (!_roles.ContainsKey(roleCode))
                _roles.Add(roleCode, new Role(roleCode, roleName, roleGroupName));
            var role = _roles[roleCode];
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "right")
                    {
                        var rightCode = reader.GetAttribute("code");
                        if (string.IsNullOrWhiteSpace(rightCode))
                            throw new Exception("Right code is empty");
                        rightCode = rightCode.Trim().ToUpper();
                        if (!role.Rights.Contains(rightCode))
                            role.Rights.Add(rightCode);
                        reader.Read();
                    }
                    else
                        reader.Read();
                }
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "role")
                    break;
                else
                    reader.Read();
            }
        }
        private void ParseUser(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            var name = reader.GetAttribute("name");
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Name is empty");
            var login = reader.GetAttribute("login");
            if (string.IsNullOrWhiteSpace(login))
                throw new Exception("Login is empty");
            login = login.Trim().ToLower();
            if (!_users.ContainsKey(login))
                _users.Add(login, new User(name, login));
            var user = _users[login];
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "partner")
                    {
                        var partnerCode = reader.GetAttribute("code");
                        if (string.IsNullOrWhiteSpace(partnerCode))
                            throw new Exception("Partner code is empty");
                        partnerCode = partnerCode.Trim().ToUpper();
                        user.AddPartner(partnerCode);
                        reader.Read();
                    }
                    else
                        reader.Read();
                }
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "user")
                    break;
                else
                    reader.Read();
            }
        }
        /// <summary>
        /// Загружает права из *.xml файла
        /// </summary>
        /// <param name="fileName">Имя *.xml файла</param>
        public void LoadRights(string fileName)
        {
            _roles.Clear();
            using var reader = XmlReader.Create(fileName);
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "role")
                        ParseRole(reader);
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            reader.Close();
        }
        /// <summary>
        /// Загружает информацию о партнёрский (клиентских) пользователях из *.xml файла
        /// </summary>
        /// <param name="fileName">Имя *.xml файла</param>
        public void LoadUserPartners(string fileName)
        {
            _users.Clear();
            using var reader = XmlReader.Create(fileName);
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "user")
                        ParseUser(reader);
                    else
                        reader.Read();
                }
                else
                    reader.Read();
            }
            reader.Close();
        }

        /// <summary>
        /// Возвращает имя текущего пользователя Active directory
        /// </summary>
        /// <param name="user">Пользователь AD</param>
        /// <returns>Имя пользователя AD</returns>
        public static string GetUserName(ClaimsPrincipal user)
        {
            var userName = "m.burov"; // Для теста!
            if (user != null && user.Identity != null && !string.IsNullOrWhiteSpace(user.Identity.Name))
            {
                userName = user.Identity.Name.Split('\\').Last();
                userName = userName.Trim().ToLower();
            }                
            return userName;
        }
        /// <summary>
        /// Проверяет доступность указанного права (по коду) пользователю
        /// </summary>
        /// <param name="right">Код права</param>
        /// <param name="user">Пользователь</param>
        /// <returns>Индикатор доступности права пользователю</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool CheckRight(string right, ClaimsPrincipal user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(right))
                return false;
            right = right.Trim().ToUpper();
            var rights = new HashSet<string>();
            foreach (var role in _roles.Values)
                if (user.IsInRole(role.GroupName))
                    rights.UnionWith(role.Rights);                        

            return rights.Contains(right);
        }
        /// <summary>
        /// Проверяет доступность нескольких прав (по кодам прав) пользователю
        /// </summary>
        /// <param name="rights">Массив кодов прав</param>
        /// <param name="user">Пользователь</param>
        /// <returns>Индикатор доступности ХОТЯ БЫ ОДНОГО права (из массива) пользователю</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool CheckRights(string[] rights, ClaimsPrincipal user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (rights == null)
                throw new ArgumentNullException(nameof(rights));
            foreach (var right in rights)
                if (CheckRight(right, user))
                    return true;
            return false;
        }
        /// <summary>
        /// Возвращает идентификатор партнёра (клиента) по имени пользователя
        /// </summary>
        /// <param name="userLogin">Имя пользователя</param>
        /// <returns>Идентификатор партнёра (клиента)</returns>
        /// <exception cref="ArgumentException"></exception>
        public string GetPartnerIdByUser(string userLogin) 
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                throw new ArgumentException("User name is empty!");
            userLogin = userLogin.Split('\\').Last();
            userLogin = userLogin.Trim().ToLower();
            if (!_users.ContainsKey(userLogin))
                throw new ArgumentException($"User {userLogin} not found!");
            var user = _users[userLogin];
            var partners = user.GetPartners();
            return partners[0];
        }
        /// <summary>
        /// Возвращает идентификаторы партнёров (мандантов) по имени пользователя (тех мандантов, с которыми может работать указанный пользователь)
        /// </summary>
        /// <param name="userLogin">Имя пользователя</param>
        /// <returns>Идентификаторы мандантов</returns>
        /// <exception cref="ArgumentException"></exception>
        public HashSet<string> GetPartnersIdsByUser(string userLogin)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                throw new ArgumentException("User name is empty!");
            userLogin = userLogin.Split('\\').Last();
            userLogin = userLogin.Trim().ToLower();
            if (!_users.ContainsKey(userLogin))
                return new HashSet<string>();
            var user = _users[userLogin];
            var partners = user.GetPartners();
            return partners.ToHashSet();
        }
    }
}