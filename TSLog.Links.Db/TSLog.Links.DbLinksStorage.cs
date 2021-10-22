using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using TSLog.BO;
using TSLog.Db;

namespace TSLog.Links.Db
{
    public class DbLinksStorage : DbStorage, ILinksStorage
    {
        public DbLinksStorage(string dataSource) : base(dataSource) { }

        public void PostProductsList(ProductsList message, LegalEntity client)
        {
            //
        }

        public void PostClientsList(ClientsList message, LegalEntity client)
        {
            //
        }

        public void PostInboundNotification(InboundInstruction message)
        {
            //
        }

        public InboundNotification[] GetInboundResponses()
        {
            return null;
        }

        public InboundNotification GetInboundResponse(string partnerId, string partnerInboundId)
        {
            // TODO: cache data then find info in it
            return null;
        }

        public void PostOrderNotification(OutboundInstruction message)
        { }

        public OutboundNotification[] GetOrderResponses()
        {
            /*
             
             
             */


            List<OutboundNotification> result = new List<OutboundNotification>();
            using (var cmd = base._connection.CreateCommand())
            {
                cmd.CommandText = "select * from v_partner";
                using (var r = cmd.ExecuteReader()) 
                {
                    while (r.Read())
                    {

                        var or = new OutboundNotification(-1, 'O', "", 1);
                        //or.Data = Convert.ToString(r["id"]) + " - " + Convert.ToString(r["name"]);
                        //
                        or.OrderId = Convert.ToString(r["id"]);
                        or.OrderNumber = Convert.ToString(r["name"]);
                        //
                        result.Add(or);
                    }
                    r.Close();
                }
            }
            return result.ToArray();
        }

        public OutboundNotification GetOrderResponse(string partnerId, string partnerOrderId)
        {
            // TODO: cache data then find info in it
            return null;
        }

        public OutboundNotification[] GetPickingStartNotifications()
        {
            return null;
        }

        public OutboundNotification[] GetPickingFinishNotifications()
        {
            return null;
        }

        public OutboundNotification[] GetShippingNotifications()
        {
            return null;
        }


        public ProductsList GetProductsList(LegalEntity client)
        {
            ProductsList products = new ProductsList(-1, 'O', "", 1);

            using (var cmd = base._connection.CreateCommand())
            {
                cmd.CommandText = "select * from ECCO2_S1_PDBA.art_t a where a.mndnr = :client_id"; // TODO check if there  is more efficient and not breakable way
                cmd.Parameters.Add(new OracleParameter("client_id", client.Id));

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {



                        var id = Convert.ToString(r["artid"]);
                        var tmpProduct = new Product(-1, id);

                        tmpProduct.Name = Convert.ToString(r["artbez"]);

                        tmpProduct.Group = Convert.ToString(r["ArtGrpNam"]);
                        tmpProduct.Gtin = Convert.ToString(r["ArtEan"]);

                        tmpProduct.Netto = Convert.ToInt32(r["artgewnet"]);
                        tmpProduct.Brutto = Convert.ToInt32(r["artgewbru"]);

                        tmpProduct.Length = Convert.ToInt32(r["ArtLng"]);
                        tmpProduct.Width = Convert.ToInt32(r["ArtBre"]);
                        tmpProduct.Heigth = Convert.ToInt32(r["ArtHoe"]);
                        tmpProduct.Volume = Convert.ToInt32(r["ArtVol"]);

                        tmpProduct.UnitType = Convert.ToString(r["ArtMgEh"]);
                        tmpProduct.UnitLinerial = Convert.ToString(r["ARTABMEH"]);
                        tmpProduct.UnitVolume = Convert.ToString(r["ArtVolEh"]);
                        tmpProduct.UnitWeight = Convert.ToString(r["ARTGEWEH"]);

                        //product.MaxAmountPerPalet = Convert.ToInt32(r["ArtLttMax"]); // ArtLtt
                        //product.AmountPerPackage = Convert.ToInt32(r["ArtLttMgPack"]); // ArtLtt
                        tmpProduct.ExpirationDateInDays = Convert.ToInt32(r["ARTMHDTAGEWEDEFLT"]);
                        tmpProduct.Price = r["ARTWERTVK"] == DBNull.Value ? 0 : Convert.ToDouble(r["ARTWERTVK"]);
                        //
                        tmpProduct.Info1 = Convert.ToString(r["ArtInfo1"]);
                        tmpProduct.Info2 = Convert.ToString(r["ArtInfo2"]);
                        tmpProduct.Info3 = Convert.ToString(r["ArtInfo3"]);
                        tmpProduct.Info4 = Convert.ToString(r["ArtInfo4"]);
                        tmpProduct.Info5 = Convert.ToString(r["ArtInfo5"]);
                        tmpProduct.Info6 = Convert.ToString(r["ArtInfo6"]);
                        //
                        products.ProductsData.Add(tmpProduct);

                    }
                    r.Close();
                }
            }
            return products;
        }


        public ClientsList GetClientsList(bool loadPartners = false) // TODO to delete (just for debug)
        {
            ClientsList clients = new ClientsList(-1, 'O', "", 1);

            using (var cmd = base._connection.CreateCommand())
            {


                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {

                        var tmpClient = new LegalEntity(-1, "");


                    }
                    r.Close();
                }
            }
            return clients;
        }
    }
}
