namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing {
    using System;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// TODO: This is super duper inefficient.
    /// </summary>
    [Serializable /* TODO: Implement this properly - just making a test run right now. - would not needed if there's a test teardown signal */]
    internal sealed class VectorClock
    {
        private AsyncLocal<VectorClockValue> current = new AsyncLocal<VectorClockValue>();

        public VectorClock()
        {
        }

        public void Extend()
        {
            lock (this.current)
            {
                this.current.Value = new VectorClockValue(this.current.Value);
            }
        }

        public string Value
        {
            get
            {
                var logValue = this.current.Value.ForDisplay();
                return string.Join(".", logValue.Take(logValue.Length));
            }
        }

        public void Pop()
        {
            lock (this.current)
            {
                this.current.Value = this.current.Value.parent;
            }
        }
        
        [Serializable /* TODO: Implement this properly - just making a test run right now. - would not needed if there's a test teardown signal */]
        private sealed class VectorClockValue
        {
            public VectorClockValue parent;
            public int myValue;
            public int next;

            public VectorClockValue(VectorClockValue parent)
            {
                this.parent = parent;
                this.myValue = parent == null ? 1 : parent.next++;
                this.next = 1;
            }

            public int[] ForDisplay()
            {
                if (this.parent == null)
                {
                    return new int[]{this.myValue};
                }

                var arr = this.parent.ForDisplay();
                var ret = new int[arr.Length + 1];
                arr.CopyTo(ret, 0);
                ret[arr.Length] = this.myValue;
                return ret;
            }
        }
    }
}