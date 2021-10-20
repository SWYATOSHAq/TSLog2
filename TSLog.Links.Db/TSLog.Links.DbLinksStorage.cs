using System;
using System.Collections.Generic;
using TSLog.Db;

namespace TSLog.Links.Db
{
    public class DbLinksStorage : DbStorage, ILinksStorage
    {
        public DbLinksStorage(string dataSource) : base(dataSource) { }
        public void PostProductsList(ProductsList message)
        {
            //
        }
        public void PostClientsList(ClientsList message)
        {
            //
        }
        public void PostInboundNotification(InboyndNotification message)
        {
            //
        }
        public InboundResponse[] GetInboundResponses()
        {
            return null;
        }
        public InboundResponse GetInboundResponse(string partnerId, string partnerInboundId)
        {
            return null;
        }
        public void PostOrderNotification(OrderNotofocation message)
        { }
        public OrderResponse[] GetOrderResponses()
        {
            List<OrderResponse> result = new List<OrderResponse>();
            using (var cmd = base._connection.CreateCommand())
            {
                cmd.CommandText = "select * from v_partner";
                using (var r = cmd.ExecuteReader()) 
                {
                    while (r.Read())
                    {
                        var or = new OrderResponse(-1, 'O', "", 1);
                        //or.Data = Convert.ToString(r["id"]) + " - " + Convert.ToString(r["name"]);
                        or.OrderId = Convert.ToString(r["id"]);
                        or.OrderNumber = Convert.ToString(r["name"]);
                        result.Add(or);
                    }
                    r.Close();
                }
            }
            return result.ToArray();
        }
        public OrderResponse GetOrderResponse(string partnerId, string partnerOrderId)
        {
            return null;
        }
        public PickingStartNotification[] GetPickingStartNotifications()
        {
            return null;
        }
        public PickingFinishNotification[] GetPickingFinishNotifications()
        {
            return null;
        }
        public ShippingNotification[] GetShippingNotifications()
        {
            return null;
        }
    }
}
