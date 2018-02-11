using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSharpApp
{
    public class TriggeringTextWriter : TextWriter
    {
        public Action<char> CharWritten;
        public Action<string> StringWritten;

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
        
        public override void Write(char value)
        {
            OnCharWritten(value);
        }

        public override void Write(string value)
        {
            OnStringWritten(value);
        }

        public override void WriteLine(string value)
        {
            OnStringWritten(value + this.NewLine);
        }

        private void OnCharWritten(char value)
        {
            if (CharWritten != null)
            {
                CharWritten(value);
            }
        }

        private void OnStringWritten(string value)
        {
            if (StringWritten != null)
            {
                StringWritten(value);
            }
        }
    }
}
