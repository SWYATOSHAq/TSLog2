namespace TSLog.Common.BO
{
    /// <summary>
    /// ТЕ
    /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={996AA0CB-D177-4668-AFB4-1593D0882705}
    /// </summary>
    public class TransportUnit
    {
        /// <summary>
        /// Товарная позиция ТЕ
        /// </summary>
        public class Line
        {
            public string Id { get; set; }
        }

        public string Id { get; set; }
    }
}