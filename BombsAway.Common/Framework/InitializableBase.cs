using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Framework
{
    public abstract class InitializableBase
    {
        protected DatabaseContext Context { get; private set; }

        public InitializableBase(DatabaseContext context) { Context = context; }

        // how to create this object
        internal abstract void Initialize();

        // how to destory this object
        internal virtual void Destroy() { }
    }
}
