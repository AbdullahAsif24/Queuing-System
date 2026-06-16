// MMS.cs — M/M/S Queueing Model (Multi-server)
namespace QueueingModels
{
    public class MMS
    {
        protected double lambda;
        protected double mu;
        protected int s;
        protected TimeUnit baseUnit;

        public MMS(double lambda, double mu, int s, TimeUnit baseUnit = TimeUnit.Minutes)
        {
            this.lambda   = lambda;
            this.mu       = mu;
            this.s        = s;
            this.baseUnit = baseUnit;
        }

        public double Rho()     => lambda / ((double)s * mu);
        public bool   IsStable()=> Rho() < 1.0;

        public double P0()
        {
            double r   = lambda / mu;
            double rho = Rho();
            double sum = 0.0;
            for (int n = 0; n <= s - 1; n++)
                sum += Math.Pow(r, n) / Factorial(n);
            double last = Math.Pow(r, s) / (Factorial(s) * (1.0 - rho));
            return 1.0 / (sum + last);
        }

        public double Pn(int n)
        {
            double r  = lambda / mu;
            double p0 = P0();
            if (n < s) return (Math.Pow(r, n) / Factorial(n)) * p0;
            return (Math.Pow(r, n) / (Factorial(s) * Math.Pow(s, n - s))) * p0;
        }

        public double Lq()
        {
            double r   = lambda / mu;
            double rho = Rho();
            return (P0() * Math.Pow(r, s) * rho) / (Factorial(s) * Math.Pow(1.0 - rho, 2));
        }

        public double Wq()      => Lq() / lambda;
        public double W()       => Wq() + (1.0 / mu);
        public double L()       => Lq() + (lambda / mu);
        public double IdleTime()=> 1.0 - Rho();

        protected static double Factorial(int n)
        {
            double r = 1.0;
            for (int i = 2; i <= n; i++) r *= i;
            return r;
        }

        public virtual QueueResults BuildResults()
        {
            return new QueueResults
            {
                ModelName  = $"M/M/{s}",
                Rho        = Rho(),
                P0         = P0(),
                Lq         = Lq(),
                L          = L(),
                Wq         = Wq(),
                W          = W(),
                IdleTime   = IdleTime(),
                BaseUnit   = baseUnit
            };
        }

        public virtual void PrintResults()
        {
            if (!IsStable())
            {
                Console.WriteLine($"\n  [ERROR] M/M/{s} UNSTABLE: λ >= S*μ. Queue grows infinitely.");
                return;
            }
            BuildResults().Print();
        }
    }
}
