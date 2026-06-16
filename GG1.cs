// GG1.cs — G/G/1  (Marchal's approximation)
// Ca² = σ²_a / (E[A])²  where E[A]=1/λ  →  Ca² = varArrival * λ²
// Cs² = varService * μ²
namespace QueueingModels
{
    public class GG1
    {
        private double lambda;
        private double mu;
        private double varArrival;
        private double varService;
        private TimeUnit baseUnit;

        public GG1(double lambda, double mu, double varArrival, double varService,
                   TimeUnit baseUnit = TimeUnit.Minutes)
        {
            this.lambda     = lambda;
            this.mu         = mu;
            this.varArrival = varArrival;
            this.varService = varService;
            this.baseUnit   = baseUnit;
        }

        public double Rho()      => lambda / mu;
        public bool   IsStable() => Rho() < 1.0;
        public double Ca2()      => varArrival * lambda * lambda;
        public double Cs2()      => varService * mu * mu;

        // Marchal approximation
        public double Lq()
        {
            double rho = Rho();
            double ca2 = Ca2();
            double cs2 = Cs2();
            return (rho * rho * (1.0 + cs2) * (ca2 + rho * rho * cs2))
                   / (2.0 * (1.0 - rho) * (1.0 + rho * rho * cs2));
        }

        public double Wq()       => Lq() / lambda;
        public double W()        => Wq() + (1.0 / mu);
        public double L()        => lambda * W();
        public double IdleTime() => 1.0 - Rho();

        public QueueResults BuildResults() => new QueueResults
        {
            ModelName  = "G/G/1",
            Rho        = Rho(),
            Lq         = Lq(),
            L          = L(),
            Wq         = Wq(),
            W          = W(),
            IdleTime   = IdleTime(),
            BaseUnit   = baseUnit,
            ExtraLines = new List<string>
            {
                $"  Ca² (arrival CoV²) = {Ca2():F4}",
                $"  Cs² (service CoV²) = {Cs2():F4}"
            }
        };

        public void PrintResults()
        {
            if (!IsStable()) { Console.WriteLine("\n  [ERROR] G/G/1 UNSTABLE."); return; }
            BuildResults().Print();
        }
    }
}
