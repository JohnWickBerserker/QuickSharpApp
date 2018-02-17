using ICSharpCode.CodeCompletion;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickSharpApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        SimpleCommand _runCmd;
        StringBuilder _output = new StringBuilder();
        TriggeringTextWriter _consoleWriter = new TriggeringTextWriter();
        ObservableCollection<RefDto> _refAssemblies = new ObservableCollection<RefDto>()
        {
            new RefDto { Name = "System.dll" },
            new RefDto { Name = "System.Core.dll" },
            new RefDto { Name = "System.Data.dll" },
            new RefDto { Name = "System.Linq.dll" },
            new RefDto { Name = "System.Xml.dll" },
            new RefDto { Name = "System.Xml.Linq.dll" },
        };
        ICSharpCode.CodeCompletion.CSharpCompletion _completion;
        ICSharpCode.AvalonEdit.Document.TextDocument _doc;

        public MainViewModel()
        {
            _completion = new ICSharpCode.CodeCompletion.CSharpCompletion(new ScriptProvider());
            _runCmd = new SimpleCommand(() => Run());
            var code =
                @"using System;
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
            _doc = new ICSharpCode.AvalonEdit.Document.TextDocument(code);
            _doc.FileName = "code.cs";
            _consoleWriter.CharWritten += x => { _output.Append(x); OnOutputChanged(); };
            _consoleWriter.StringWritten += x => { _output.Append(x); OnOutputChanged(); };
            SetConsoleOutput();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public CSharpCompletion Completion
        {
            get
            {
                return _completion;
            }
        }

        public ICSharpCode.AvalonEdit.Document.TextDocument Document
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
        public StringBuilder Output
        {
            get
            {
                return _output;
            }
        }
        public void Run()
        {
            _output.Clear();

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

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, _doc.Text);

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Output.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }
                OnOutputChanged();
                return;
            }
            else
            {
                OnOutputChanged();
            }

            MethodInfo main = null;
            try
            {
                Assembly assembly = results.CompiledAssembly;
                Type program = assembly.GetType("QuickSharp.Program");
                main = program.GetMethod("Main");
            }
            catch (Exception ex)
            {
                Output.AppendLine(ex.Message);
                OnOutputChanged();
                return;
            }

            OnOutputChanged();

            try
            {
                main.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Unhandled exception: " + ex.ToString());
            }
        }
        private void OnOutputChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Output"));
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void SetConsoleOutput()
        {
            Console.SetOut(_consoleWriter);
        }
    }
}
