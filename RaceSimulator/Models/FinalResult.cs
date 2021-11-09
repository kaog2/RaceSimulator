using SwissTiming.Timing.AcquisitionSimulator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceSimulator.Models
{    
    public class FinalResult
    {
        private int? _rank;

        private IrmKind? _irmKind;

        private Competitor _competitor;

        private DateTime? _netTime;

        private DateTime? _finalTime;

        private DateTime? _startTime;

        private string _comment;

        public int? Rank 
        { 
            get { return _rank; }
            set { _rank = value; } 
        }

        public IrmKind? IrmKind 
        { 
            get { return _irmKind; }
            set { _irmKind = value; } 
        }

        public Competitor Competitor 
        { 
            get { return _competitor; }
            set { _competitor = value; } 
        }

        public DateTime? NetTime
        {
            get { return _netTime; }
            set { _netTime = value; }
        }

        public DateTime? FinalTime
        {
            get { return _finalTime; }
            set { _finalTime = value; }
        }

        public DateTime? StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public FinalResult()
        {
            
        }

        public FinalResult(Competitor competitor, int? rank, IrmKind? irmKind, DateTime? netTime, DateTime? finalTime, DateTime? startTime, string comment)
        {
            Competitor = competitor;
            Rank = rank;
            IrmKind = irmKind;
            NetTime = netTime;
            FinalTime = finalTime;
            StartTime = startTime;
            Comment = comment;
        }
    }
}
