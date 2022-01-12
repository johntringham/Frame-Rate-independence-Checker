using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameRateIndependenceChecker.Pages
{
    public class CompiledSnippet
    {
        private Action<float> update;
        private Func<float> getValue;
        private Action reset;

        public CompiledSnippet(Action<float> update, Func<float> getValue, Action reset)
        {
            this.update = update;
            this.getValue = getValue;
            this.reset = reset;
        }

        public void Run(float deltaTime)
        {
            update.Invoke(deltaTime);
        }

        public float GetValue()
        {
            return this.getValue.Invoke();
        }

        public void ResetValue()
        {
            this.reset.Invoke();
        }
    }
}
