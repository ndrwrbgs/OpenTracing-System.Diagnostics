namespace OpenTracing.Contrib.SystemDiagnostics.ToOpenTracing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Remoting;
    using System.Text;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using OpenTracing.Util;

    /// <summary>
    /// Send Console.Out events to OT
    /// </summary>
    [PublicAPI]
    public sealed class OpenTracingTextWriter : TextWriter
    {
        private readonly TextWriter textWriterImplementation;

        public OpenTracingTextWriter(TextWriter textWriterImplementation)
        {
            this.textWriterImplementation = textWriterImplementation;
        }

        public override void Close()
        {
            this.textWriterImplementation.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.textWriterImplementation.Dispose();
            }
        }

        public override void Flush()
        {
            this.textWriterImplementation.Flush();
        }

        public override void Write(char value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(char[] buffer)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, new string(buffer)),
                });

            this.textWriterImplementation.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, new string(buffer, index, count)),
                });

            this.textWriterImplementation.Write(buffer, index, count);
        }

        public override void Write(bool value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(int value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(uint value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(long value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(ulong value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(float value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(double value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(decimal value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(string value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(object value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    // TODO: object as EventLogKey
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            this.textWriterImplementation.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg0)),
                });

            this.textWriterImplementation.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg0, arg1)),
                });

            this.textWriterImplementation.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg0, arg1, arg2)),
                });

            this.textWriterImplementation.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] arg)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg)),
                });

            this.textWriterImplementation.Write(format, arg);
        }

        public override void WriteLine()
        {
            this.textWriterImplementation.WriteLine();
            
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Empty),
                });
        }

        public override void WriteLine(char value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(char[] buffer)
        {
            this.textWriterImplementation.WriteLine(buffer);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, new string(buffer)),
                });
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            this.textWriterImplementation.WriteLine(buffer, index, count);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, new string(buffer, index, count)),
                });
        }

        public override void WriteLine(bool value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(int value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(uint value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(long value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(ulong value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(float value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(double value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(decimal value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(string value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(object value)
        {
            this.textWriterImplementation.WriteLine(value);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });
        }

        public override void WriteLine(string format, object arg0)
        {
            this.textWriterImplementation.WriteLine(format, arg0);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg0)),
                });
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            this.textWriterImplementation.WriteLine(format, arg0, arg1);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg0, arg1)),
                });
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            this.textWriterImplementation.WriteLine(format, arg0, arg1, arg2);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg0, arg1, arg2)),
                });
        }

        public override void WriteLine(string format, params object[] arg)
        {
            this.textWriterImplementation.WriteLine(format, arg);

            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Format(format, arg)),
                });
        }

        public override Task WriteAsync(char value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            return this.textWriterImplementation.WriteAsync(value);
        }

        public override Task WriteAsync(string value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            return this.textWriterImplementation.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.IsWriteWithoutNewline, null), 
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, new string(buffer, index, count)),
                });

            return this.textWriterImplementation.WriteAsync(buffer, index, count);
        }

        public override Task WriteLineAsync(char value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            return this.textWriterImplementation.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(string value)
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, value),
                });

            return this.textWriterImplementation.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            // TODO: Various perf concerns. One of them: given you're ToString-ing it anyway, should we just delegate to WriteLineAsync(string) after tracing?
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, new string(buffer, index, count)),
                });

            return this.textWriterImplementation.WriteLineAsync(buffer, index, count);
        }

        public override Task WriteLineAsync()
        {
            GlobalTracer.Instance.ActiveSpan
                .Log(new[]
                {
                    new KeyValuePair<string, object>(OpenTracingTraceListener.Constants.EventLogKey, string.Empty),
                });

            return this.textWriterImplementation.WriteLineAsync();
        }

        public override Task FlushAsync()
        {
            return this.textWriterImplementation.FlushAsync();
        }

        public override IFormatProvider FormatProvider => this.textWriterImplementation.FormatProvider;
        public override Encoding Encoding => this.textWriterImplementation.Encoding;
        public override string NewLine => this.textWriterImplementation.NewLine;
        public override object InitializeLifetimeService()
        {
            return this.textWriterImplementation.InitializeLifetimeService();
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return this.textWriterImplementation.CreateObjRef(requestedType);
        }

        public override string ToString()
        {
            return this.textWriterImplementation.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.textWriterImplementation.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.textWriterImplementation.GetHashCode();
        }
    }
}