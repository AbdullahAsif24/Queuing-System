// MM1.cs — M/M/1 Queueing Model
namespace QueueingModels
{
    public class MM1
    {
        private double lambda;
        private double mu;
        private TimeUnit baseUnit;

        public MM1(double lambda, double mu, TimeUnit baseUnit = TimeUnit.Minutes)
        {
            this.lambda   = lambda;
            this.mu       = mu;
            this.baseUnit = baseUnit;
        }

        public double Rho()     => lambda / mu;
        public bool   IsStable()=> Rho() < 1.0;
        public double Lq()      { double r = Rho(); return (r * r) / (1.0 - r); }
        public double L()       { double r = Rho(); return r / (1.0 - r); }
        public double Wq()      => Lq() / lambda;
        public double W()       => Wq() + (1.0 / mu);
        public double IdleTime()=> 1.0 - Rho();

        public QueueResults BuildResults()
        {
            return new QueueResults
            {
                ModelName = "M/M/1",
                Rho       = Rho(),
                Lq        = Lq(),
                L         = L(),
                Wq        = Wq(),
                W         = W(),
                IdleTime  = IdleTime(),
                BaseUnit  = baseUnit
            };
        }

        public void PrintResults()
        {
            if (!IsStable())
            {
                Console.WriteLine("\n  [ERROR] M/M/1 UNSTABLE: λ >= μ. Queue grows infinitely.");
                return;
            }
            BuildResults().Print();
        }
    }
}
