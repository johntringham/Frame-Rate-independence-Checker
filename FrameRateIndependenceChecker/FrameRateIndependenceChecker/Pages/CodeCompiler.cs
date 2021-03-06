using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FrameRateIndependenceChecker.Pages
{
    public class CodeCompiler
    {
        private List<MetadataReference> _references;
        private CSharpCompilation compilation;

        public CodeCompiler()
        {
        }

        public async Task<(CompiledSnippet, IEnumerable<string>)> Compile(string input)
        {
            Console.WriteLine("eh?: " + input);
            input = mathfClassContent + input;
            List<string> errorMessages;

            try
            {
                Assembly assembly = null;
                IEnumerable<Diagnostic> errors = null;
                var task = Task.Run(() => TryCompile(input, out assembly, out errors));
                if (!(await task))
                {
                    foreach(var e in errors)
                    {
                        Console.WriteLine(e.GetMessage());
                    }
                    errorMessages = errors.Select(e => e.GetMessage()).ToList();
                    
                    return (null, errorMessages);
                }
                else
                {
                    var entryPoint = compilation.GetEntryPoint(CancellationToken.None);
                    
                    var type = assembly.GetType("MainThing");
                    var method = type.GetMethod("Update");
                    var updateDelegate = (Action<float>)method.CreateDelegate(typeof(Action<float>));

                    var getValueProperty = type.GetProperty("value");
                    var getValueDelegate = (Func<float>)(() => (float)getValueProperty.GetValue(null));

                    var resetValueDelegate = (Action)(() => getValueProperty.SetValue(null, 0));

                    return (new CompiledSnippet(updateDelegate, getValueDelegate, resetValueDelegate), new List<string>());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(e.Message);
                return (null, new List<string>() { "Unexpected error - could not compile for some reason" });
            }
        }

        public async Task OnInitializedAsync(string baseUri)
        {
            var refs = AppDomain.CurrentDomain.GetAssemblies();
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };

            var references = new List<MetadataReference>();

            foreach (var reference in refs.Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)))
            {
                string requestUri = $"_framework/_bin/{reference.Location}";
                Console.WriteLine("trying to get " + requestUri);
                var stream = await client.GetStreamAsync(requestUri);
                references.Add(MetadataReference.CreateFromStream(stream));
            }

            var mathfUri = "Mathf.txt";
            mathfClassContent = await client.GetStringAsync(mathfUri);
            
            ////Disabled = false;
            _references = references;
        }

        private string mathfClassContent;

        private bool TryCompile(string source, out Assembly assembly, out IEnumerable<Diagnostic> errorDiagnostics)
        {
            assembly = null;
            var scriptCompilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                new[] { CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithKind(SourceCodeKind.Regular).WithLanguageVersion(LanguageVersion.Preview)) },
                _references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary
                , usings: new[]
                {
                    "System",
                    "System.IO",
                    "System.Collections.Generic",
                    "System.Console",
                    "System.Diagnostics",
                    "System.Dynamic",
                    "System.Linq",
                    "System.Linq.Expressions",
                    "System.Net.Http",
                    "System.Text",
                    "System.Threading.Tasks",
                    "System.Math"
                }
                )
                //_previousCompilation
            );

            errorDiagnostics = scriptCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error);
            if (errorDiagnostics.Any())
            {
                return false;
            }

            using (var peStream = new MemoryStream())
            {
                var emitResult = scriptCompilation.Emit(peStream);

                if (emitResult.Success)
                {
                    //_submissionIndex++;
                    compilation = scriptCompilation;
                    assembly = Assembly.Load(peStream.ToArray());
                    return true;
                }
            }

            return false;
        }
    }
}
