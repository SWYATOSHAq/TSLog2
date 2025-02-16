using System;
using System.Collections.Generic;
using TSLog.LegalEntities.BO;

namespace TSLog.Links;

/// <summary>
/// Справочник клиентов.
/// </summary>
public class ClientsList : Message
{
    private readonly List<LegalEntity> _clients = null;

    public ClientsList(long id, string number, byte version, DateTime date,
        LegalEntity sender, LegalEntity recipient, LegalEntity dataSource, LegalEntity dataRecipient,
        string note = "")
        : base(id, number, 'I', "CLIENTS_LIST", version, date, sender, recipient, note)
    {
        DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        DataRecipient = dataRecipient ?? throw new ArgumentNullException(nameof(dataRecipient)); ;
        _clients = new List<LegalEntity>();
    }

    /// <summary>
    /// Отправитель данных.
    /// </summary>
    public LegalEntity DataSource { get; private set; }

    /// <summary>
    /// Получатель данных.
    /// </summary>
    public LegalEntity DataRecipient { get; private set; }

    /// <summary>
    /// Добавить клиента.
    /// </summary>
    /// <param name="client">Клиент.</param>
    public void AddClient(LegalEntity client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        if (client != LegalEntity.Null)
            _clients.Add(client);
    }

    /// <summary>
    /// Добавить клиентов.
    /// </summary>
    /// <param name="clients">Клиенты.</param>
    public void AddClients(LegalEntity[] clients)
    {
        ArgumentNullException.ThrowIfNull(clients);
        foreach (var client in clients)
            AddClient(client);
    }

    /// <summary>
    /// Клиенты.
    /// </summary>
    public LegalEntity[] Clients => _clients.ToArray();
}
