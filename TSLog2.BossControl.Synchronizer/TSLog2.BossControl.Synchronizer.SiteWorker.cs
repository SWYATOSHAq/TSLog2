using System;

namespace TSLog2.BossControl.Synchronizer
{
    internal class SiteWorker
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Kind { get; set; }
        public int WorkerId { get; set; }
        public int SiteId { get; set; }
        public string PartnerId { get; set; }
        public string Note { get; set; }

        private string _key = "";
        public string Key
        {
            get 
            {
                if (string.IsNullOrWhiteSpace(_key))
                    _key = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}", 
                        Id, DateTime, Kind, WorkerId, SiteId, PartnerId, Note);
                return _key; 
            }
        }
    }
}
