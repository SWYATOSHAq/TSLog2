using System;
using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Справочник клиентов
    /// </summary>
    public class ClientsList : Message
    {
        public ClientsList(long id, byte version) 
            : base(id, 'I', "CLIENTS_LIST", version)
        {
            _clients = new List<LegalEntity>();
        }

        private readonly List<LegalEntity> _clients = null;
        public void AddClient(LegalEntity client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (client != LegalEntity.Null)
                _clients.Add(client);
        }
        public void AddClients(LegalEntity[] clients)
        {
            if (clients == null)
                throw new ArgumentNullException(nameof(clients));
            foreach (var client in clients)
                AddClient(client);
        }
        public LegalEntity[] Clients => _clients.ToArray();
    }
}
