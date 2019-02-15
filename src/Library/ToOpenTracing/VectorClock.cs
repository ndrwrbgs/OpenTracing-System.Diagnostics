namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing {
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;

    /// <summary>
    /// TODO: This is super duper inefficient.
    /// </summary>
    [Serializable]
    internal sealed class VectorClock : ISerializable
    {
        private AsyncLocal<VectorClockValue> current = new AsyncLocal<VectorClockValue>();

        public VectorClock()
        {
        }
        
        // Implement this method to serialize data. The method is called 
        // on serialization.
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Use the AddValue method to specify serialized values.
            info.AddValue("current", current.Value, typeof(VectorClockValue));
        }

        // The special constructor is used to deserialize values.
        public VectorClock(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            current.Value = (VectorClockValue) info.GetValue("current", typeof(VectorClockValue));
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
        
        [Serializable]
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