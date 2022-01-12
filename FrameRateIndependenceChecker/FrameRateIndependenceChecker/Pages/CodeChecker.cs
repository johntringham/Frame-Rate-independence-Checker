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
    public class CodeChecker
    {
        private List<MetadataReference> _references;
        private CSharpCompilation compilation;

        public CodeChecker()
        {
        }

        public async Task Check(string input)
        {
            //var scriptOptions = new ScriptOptions() { }
            //var script = CSharpScript.Create(input, );
            Console.WriteLine("eh?: " + input);

            try
            {
                var task = TryCompile(input, out var assembly, out var errors);
                Console.WriteLine("task: " + task);
                if (!task)
                {
                    foreach(var e in errors)
                    {
                        Console.WriteLine(e.GetMessage());
                    }
                }
                else
                {
                    var entryPoint = compilation.GetEntryPoint(CancellationToken.None);
                    //var t = compilation.GetTypeByMetadataName("MainThing");
                    //Console.WriteLine(t?.Name);
                    
                    var type = assembly.GetType("MainThing");
                    var method = type.GetMethod("Main");
                    var dele = (Func<float>)method.CreateDelegate(typeof(Func<float>));

                    var output = dele.Invoke();
                    Console.WriteLine(output);

                    output = dele.Invoke();
                    Console.WriteLine(output);

                    output = dele.Invoke();
                    Console.WriteLine(output);
                    output = dele.Invoke();
                    Console.WriteLine(output);
                    output = dele.Invoke();
                    Console.WriteLine(output);
                    output = dele.Invoke();
                    Console.WriteLine(output);
                    output = dele.Invoke();
                    Console.WriteLine(output);
                    output = dele.Invoke();
                    Console.WriteLine(output);
                    output = dele.Invoke();
                    Console.WriteLine(output);
                    output = dele.Invoke();
                    Console.WriteLine(output);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(e.Message);
                return;
            }
        }

        public async Task OnInitializedAsync(string baseUri)
        {
            var refs = AppDomain.CurrentDomain.GetAssemblies();
            var client = new HttpClient
            {
                BaseAddress = new Uri(/*navigationManager.BaseUri*/ baseUri)
            };

            var references = new List<MetadataReference>();

            foreach (var reference in refs.Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)))
            {
                string requestUri = $"_framework/_bin/{reference.Location}";
                Console.WriteLine("trying to get " + requestUri);
                var stream = await client.GetStreamAsync(requestUri);
                references.Add(MetadataReference.CreateFromStream(stream));
            }

            ////Disabled = false;
            _references = references;
        }

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
                    "System.Threading.Tasks"
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
