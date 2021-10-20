using System;

namespace TSLog.Links
{
    public interface ILinksStorage
    {
        public void PostProductsList(ProductsList message);
        public void PostClientsList(ClientsList message);
        public void PostInboundNotification(InboyndNotification message);
        public InboundResponse[] GetInboundResponses();
        public void PostOrderNotification(OrderNotofocation message);
        public OrderResponse[] GetOrderResponses();
        public PickingStartNotification[] GetPickingStartNotifications();
        public PickingFinishNotification[] GetPickingFinishNotifications();
        public ShippingNotification[] GetShippingNotifications();
    }
}
