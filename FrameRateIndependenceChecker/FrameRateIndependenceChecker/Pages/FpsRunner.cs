using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameRateIndependenceChecker.Pages
{
    public class FpsRunner
    {
        public CompiledSnippet snippet;
        public float fps;

        public FpsRunner(CompiledSnippet snippet, float fps)
        {
            this.snippet = snippet;
            this.fps = fps;
        } 

        public Dictionary<float, float> results = new Dictionary<float, float>();

        public async Task<Dictionary<float,float>> Test(float duration)
        {
            results = new Dictionary<float, float>();
            snippet.ResetValue();

            var increment = 1.0f / fps;
            var time = 0f;
            try
            {
                while (time <= duration)
                {
                    var value = snippet.GetValue();
                    results.Add(time, value);

                    Console.WriteLine($"{time}: {value}");
                    snippet.Run(increment);

                    time += increment;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception:" + e.Message);
                return new Dictionary<float, float>();
            }

            return results;
        }
    }
}
