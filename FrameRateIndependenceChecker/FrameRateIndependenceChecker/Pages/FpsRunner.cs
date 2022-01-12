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

        public FpsRunner(CompiledSnippet snippet, float fps, bool erratic = false)
        {
            this.snippet = snippet;
            this.fps = fps;
            this.erratic = erratic;
        }

        private Random random = new Random();

        public bool erratic;

        public Dictionary<float, float> results = new Dictionary<float, float>();

        public async Task<Dictionary<float,float>> Test(float duration)
        {
            results = new Dictionary<float, float>();
            snippet.ResetValue();

            var increment = 1.0f / fps;
            var time = 0f;

            results.Add(0f, 0f);

            try
            {
                while (time <= duration)
                {
                    float inc = increment;
                    if (erratic)
                    {
                        var min = 10f;
                        var max = 60f;
                        inc = 1f / (min + (float)random.NextDouble() * (max - min));
                    }

                    snippet.Run(inc);
                    var value = snippet.GetValue();
                    
                    time += inc;
                    
                    results.Add(time, value);
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
