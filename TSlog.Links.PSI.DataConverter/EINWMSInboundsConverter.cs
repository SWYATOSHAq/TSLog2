using System;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSInboundsConverter : EINWMSBaseConverter
    {
        static string NAME = "GP";
        public override string MessageName()
        {
            return $"{base.MessageName()}.{NAME}";
        }
    }
}
