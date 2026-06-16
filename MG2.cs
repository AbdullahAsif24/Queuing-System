// MG2.cs — M/G/2
// Step 1: Compute Wq using M/M/2 (via MMS class)
// Step 2: Apply G/G/c approximation:
//         Wq^(M/G/2) = Wq^(M/M/2) * (Ca² + Cs²) / 2
//         Since arrivals are Poisson → Ca² = 1
// Cs² = varService * μ²  (variance in base_unit^2, μ in per base_unit)
namespace QueueingModels
{
    public class MG2
    {
        private double lambda;
        private double mu;
        private double varService;
        private TimeUnit baseUnit;

        public MG2(double lambda, double mu, double varService,
                   TimeUnit baseUnit = TimeUnit.Minutes)
        {
            this.lambda     = lambda;
            this.mu         = mu;
            this.varService = varService;
            this.baseUnit   = baseUnit;
        }

        public double Rho()      => lambda / (2.0 * mu);
        public bool   IsStable() => Rho() < 1.0;
        public double Ca2()      => 1.0;  // Poisson arrivals
        public double Cs2()      => varService * mu * mu;

        private MMS MM2()        => new MMS(lambda, mu, 2, baseUnit);
        public double Wq_MM2()   => MM2().Wq();
        public double Lq_MM2()   => MM2().Lq();
        public double P0()       => MM2().P0();

        public double Wq()       => Wq_MM2() * (Ca2() + Cs2()) / 2.0;
        public double Lq()       => lambda * Wq();
        public double W()        => Wq() + (1.0 / mu);
        public double L()        => lambda * W();
        public double IdleTime() => 1.0 - Rho();

        public QueueResults BuildResults() => new QueueResults
        {
            ModelName  = "M/G/2",
            Rho        = Rho(),
            P0         = P0(),
            Lq         = Lq(),
            L          = L(),
            Wq         = Wq(),
            W          = W(),
            IdleTime   = IdleTime(),
            BaseUnit   = baseUnit,
            ExtraLines = new List<string>
            {
                $"  Ca² (Poisson → 1)      = {Ca2():F4}",
                $"  Cs² (service CoV²)     = {Cs2():F4}",
                $"  [M/M/2 intermediate]",
                $"  P0                     = {P0():F4}",
                $"  Wq^(M/M/2)             = {Wq_MM2():F6} {UnitConverter.UnitLabel(baseUnit)}",
                $"  Lq^(M/M/2)             = {Lq_MM2():F4}"
            }
        };

        public void PrintResults()
        {
            if (!IsStable()) { Console.WriteLine("\n  [ERROR] M/G/2 UNSTABLE: λ >= 2μ."); return; }
            BuildResults().Print();
        }
    }
}
