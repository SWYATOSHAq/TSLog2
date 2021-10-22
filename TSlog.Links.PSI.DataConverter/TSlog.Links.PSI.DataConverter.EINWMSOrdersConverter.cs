using System;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSOrdersConverter : EINWMSBaseConverter
    {
        static string NAME = "GP";
        public override string MessageName()
        {
            return $"{base.MessageName()}.{NAME}";
        }
    }
}
