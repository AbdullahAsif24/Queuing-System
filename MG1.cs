// MG1.cs — M/G/1  (Pollaczek-Khinchine formula)
// All inputs MUST be in the same unit (base unit) before passing to constructor
// VarService must be in (base_unit)^2
// Cs² = VarService / (mean_service_time)^2 = VarService * mu^2
//   because mean_service_time = 1/mu  →  (1/mu)^2 = 1/mu^2
namespace QueueingModels
{
    public class MG1
    {
        private double lambda;
        private double mu;
        private double varService;   // in base_unit^2
        private TimeUnit baseUnit;

        public MG1(double lambda, double mu, double varService,
                   TimeUnit baseUnit = TimeUnit.Minutes)
        {
            this.lambda     = lambda;
            this.mu         = mu;
            this.varService = varService;
            this.baseUnit   = baseUnit;
        }

        public double Rho()      => lambda / mu;
        public bool   IsStable() => Rho() < 1.0;

        // Cs² = σ²_s / (E[S])²  where E[S] = 1/mu
        // so  Cs² = varService / (1/mu)² = varService * mu²
        public double Cs2()      => varService * mu * mu;

        // P-K formula:  Lq = (λ²σ²_s + ρ²) / (2(1-ρ))
        public double Lq()
        {
            double rho = Rho();
            return (lambda * lambda * varService + rho * rho) / (2.0 * (1.0 - rho));
        }

        public double Wq()       => Lq() / lambda;
        public double W()        => Wq() + (1.0 / mu);
        public double L()        => lambda * W();
        public double IdleTime() => 1.0 - Rho();

        public QueueResults BuildResults() => new QueueResults
        {
            ModelName  = "M/G/1",
            Rho        = Rho(),
            Lq         = Lq(),
            L          = L(),
            Wq         = Wq(),
            W          = W(),
            IdleTime   = IdleTime(),
            BaseUnit   = baseUnit,
            ExtraLines = new List<string>
            {
                $"  Cs² (service CoV²) = {Cs2():F4}"
            }
        };

        public void PrintResults()
        {
            if (!IsStable()) { PrintUnstable("M/G/1"); return; }
            BuildResults().Print();
        }

        private static void PrintUnstable(string m) =>
            Console.WriteLine($"\n  [ERROR] {m} UNSTABLE: λ >= μ. Queue grows infinitely.");
    }
}
