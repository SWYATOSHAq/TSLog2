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
            var roleCode = reader.GetAttribute("code") ?? string.Empty;
            var roleName = reader.GetAttribute("name") ?? string.Empty;
            var roleGroupName = reader.GetAttribute("groupName") ?? string.Empty;
            if (!_roles.ContainsKey(roleCode))
                _roles.Add(roleCode, new Role(roleCode, roleName, roleGroupName));
            var role = _roles[roleCode];
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "right")
                    {
                        var rightCode = reader.GetAttribute("code") ?? string.Empty;
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
            var name = reader.GetAttribute("name") ?? string.Empty;
            var login = reader.GetAttribute("login") ?? string.Empty;
            if (!_users.ContainsKey(login))
                _users.Add(login, new User(name, login));
            var user = _users[login];
            while (reader.ReadState == ReadState.Initial || reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "partner")
                    {
                        var partnerCode = reader.GetAttribute("code") ?? string.Empty;
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
        public void GetRights(string fileName)
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
        public void GetUserPartners(string fileName)
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

        public string GetUserName(ClaimsPrincipal user)
        {
            var userName = "m.burov"; // Для теста!
            if (user != null && user.Identity != null && !string.IsNullOrWhiteSpace(user.Identity.Name))
                userName = user.Identity.Name.Split('\\').Last();
            return userName;
        }
        public bool CheckRight(string right, ClaimsPrincipal user)
        {
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
        /// Возвращает идентификатор партнёра (клиента) по имени пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>Идентификатор партнёра (клиента)</returns>
        /// <exception cref="ArgumentException"></exception>
        public string GetPartnerIdByUser(string userName) 
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name is empty!");
            userName = userName.Split('\\').Last();
            userName = userName.ToLower();
            if (!_users.ContainsKey(userName))
                throw new ArgumentException($"User {userName} not found!");
            var user = _users[userName];
            var partners = user.GetPartners();
            return partners[0];
        }
        public HashSet<string> GetPartnersIdsByUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name is empty!");
            if (!_users.ContainsKey(userName))
                return new HashSet<string>();
            var user = _users[userName];
            var partners = user.GetPartners();
            return partners.ToHashSet();
        }
    }
}