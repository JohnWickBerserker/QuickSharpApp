using System;
using System.Text;

namespace QuickSharpApp
{
    public class ObservableStringBuilder
    {
        StringBuilder _builder = new StringBuilder();

        public event Action Changed;

        public ObservableStringBuilder Append(char value)
        {
            _builder.Append(value);
            InvokeChanged();
            return this;
        }

        public ObservableStringBuilder Append(string value)
        {
            _builder.Append(value);
            InvokeChanged();
            return this;
        }

        public ObservableStringBuilder AppendLine(string value)
        {
            _builder.AppendLine(value);
            InvokeChanged();
            return this;
        }

        public ObservableStringBuilder Clear()
        {
            _builder.Clear();
            InvokeChanged();
            return this;
        }

        private void InvokeChanged()
        {
            Changed?.Invoke();
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
