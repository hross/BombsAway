using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Analysis
{
    public class ProcessResult
    {
        public ProcessResult()
        {
            Reset = false;
            WasProcessed = false;
        }

        public ProcessResult(bool reset, bool anyProcessed)
        {
            this.Reset = reset;
            this.WasProcessed = anyProcessed;
        }

        public bool Reset { get; set; }

        public bool WasProcessed { get; set; }
    }
}
