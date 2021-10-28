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
        public LegalEntity Partner { get; set; }
        public ClientsList(long id, byte version, string body = null) 
            : base(id, 'I', "CLIENTS_LIST", version, body)
        {
            _clients = new List<LegalEntity>();
        }

        private List<LegalEntity> _clients = null;
        public void AddClient(LegalEntity client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (client != LegalEntity.Null)
                _clients.Add(client);
        }
        public void AddClients(LegalEntity[] clients)
        {
            if (clients == null)
                throw new ArgumentNullException("clients");
            foreach (var client in clients)
                AddClient(client);
        }
        public LegalEntity[] Clients
        {
            get
            {
                return _clients.ToArray();
            }
        }
    }
}
