namespace TSLog.Common.BO
{
    /// <summary>
    /// ТЕ (транспортная единица)
    /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={996AA0CB-D177-4668-AFB4-1593D0882705}
    /// </summary>
    public class TransportUnit
    {
        /// <summary>
        /// Товарная позиция ТЕ
        /// </summary>
        public class Line
        {
            /// <summary>
            /// Идентификатор
            /// </summary>
            public string Id { get; private set; }
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public string Id { get; private set; }
    }
}