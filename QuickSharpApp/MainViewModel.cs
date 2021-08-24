using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.CodeCompletion;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace QuickSharpApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string defaultCode = @"using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace QuickSharp
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine(""Hello, world!"");
        }
    }
}";
        private static readonly string[] defaultRefAssemblies =
            {
            "System.dll",
            "System.Core.dll",
            "System.Data.dll",
            "System.Linq.dll",
            "System.Xml.dll",
            "System.Xml.Linq.dll"
            };
        private SimpleCommand _runCmd;
        private ObservableStringBuilder _output = new ObservableStringBuilder();
        private TriggeringTextWriter _consoleWriter = new TriggeringTextWriter();
        private ObservableCollection<RefDto> _refAssemblies = new ObservableCollection<RefDto>();
        private CSharpCompletion _completion;
        private TextDocument _doc;

        public MainViewModel()
        {
            _completion = new CSharpCompletion(new ScriptProvider());
            _runCmd = new SimpleCommand(() => Run());
            _doc = new TextDocument(defaultCode);
            defaultRefAssemblies.Select(x => new RefDto { Name = x }).AddToCollection(_refAssemblies);
            _doc.FileName = "code.cs";
            _output.Changed += () => OnOutputChanged();
            _consoleWriter.CharWritten += x => _output.Append(x);
            _consoleWriter.StringWritten += x => _output.Append(x);
            SetConsoleOutput(_consoleWriter);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public CSharpCompletion Completion
        {
            get
            {
                return _completion;
            }
        }

        public TextDocument Document
        {
            get
            {
                return _doc;
            }
            set
            {
                if (_doc != value)
                {
                    _doc = value;
                    OnPropertyChanged("Document");
                }
            }
        }

        public ObservableCollection<RefDto> RefAssemblies
        {
            get
            {
                return _refAssemblies;
            }
        }

        public ICommand RunCommand
        {
            get
            {
                return _runCmd;
            }
        }

        public ObservableStringBuilder Output
        {
            get
            {
                return _output;
            }
        }

        public void Run()
        {
            Output.Clear();

            var results = CompileAssemblyFromText(_doc.Text);

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Output.AppendLine(string.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }
            }
            else
            {
                try
                {
                    InvokeAssemblyMainMethod(results.CompiledAssembly);
                }
                catch (Exception ex)
                {
                    Output.AppendLine(ex.Message);
                }
            }
        }

        private CompilerResults CompileAssemblyFromText(string text)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            if (RefAssemblies != null)
            {
                foreach (var i in RefAssemblies)
                {
                    if (!string.IsNullOrEmpty(i.Name))
                    {
                        parameters.ReferencedAssemblies.Add(i.Name);
                    }
                }
            }

            // True - memory generation, false - external file generation
            parameters.GenerateInMemory = true;
            // True - exe file generation, false - dll file generation
            parameters.GenerateExecutable = true;

            CompilerResults result = provider.CompileAssemblyFromSource(parameters, text);

            return result;
        }

        private void InvokeAssemblyMainMethod(Assembly assembly)
        {
            Type program = assembly.GetType("QuickSharp.Program");
            MethodInfo main = program.GetMethod("Main");
            main.Invoke(null, null);
        }

        private void OnOutputChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Output"));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetConsoleOutput(TextWriter writer)
        {
            Console.SetOut(writer);
        }
    }
}
