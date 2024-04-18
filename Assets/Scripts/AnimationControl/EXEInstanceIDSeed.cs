using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{
    class EXEInstanceIDSeed
    {
        private static EXEInstanceIDSeed Instance;
        private int Seed;
        private EXEInstanceIDSeed()
        {
            EXEInstanceIDSeed.Instance = null;
            this.Seed = 1;
        }

        public static EXEInstanceIDSeed GetInstance()
        {
            if (EXEInstanceIDSeed.Instance == null)
            {
                EXEInstanceIDSeed.Instance = new EXEInstanceIDSeed();
            }

            return EXEInstanceIDSeed.Instance;
        }

        public int GenerateID()
        {
            int Result = this.Seed;
            this.Seed++;
            return Result;
        }
    }
}
