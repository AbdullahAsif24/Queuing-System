// UnitConverter.cs
// KEY DESIGN:
//   - User can enter EITHER a "rate" (e.g. 6 customers/hr) OR a "mean time" (e.g. 10 min/customer)
//   - Everything is converted to the BASE UNIT (lambda's unit) as a RATE internally
//   - Variance is always in (base_time_unit)^2
//   - Results printed in both base unit AND alternate unit

namespace QueueingModels
{
    public enum TimeUnit { Minutes, Hours }

    public static class UnitConverter
    {
        public static string UnitLabel(TimeUnit u)   => u == TimeUnit.Minutes ? "min" : "hr";
        public static string UnitLabelSq(TimeUnit u) => u == TimeUnit.Minutes ? "min²" : "hr²";

        // Convert a RATE: e.g. 10/min → 600/hr  OR  6/hr → 0.1/min
        public static double ConvertRate(double rate, TimeUnit from, TimeUnit to)
        {
            if (from == to) return rate;
            return (from == TimeUnit.Minutes) ? rate * 60.0 : rate / 60.0;
        }

        // Convert a TIME VALUE: e.g. 10 min → 0.1667 hr  OR  0.1667 hr → 10 min
        public static double ConvertTime(double time, TimeUnit from, TimeUnit to)
        {
            if (from == to) return time;
            return (from == TimeUnit.Minutes) ? time / 60.0 : time * 60.0;
        }

        // Convert VARIANCE: var changes by k^2 when unit changes by factor k
        // e.g. var=64 min² → hr²: 64 / 3600
        public static double ConvertVariance(double variance, TimeUnit from, TimeUnit to)
        {
            if (from == to) return variance;
            double factor = (from == TimeUnit.Minutes) ? (1.0 / 60.0) : 60.0;
            return variance * factor * factor;
        }

        public static TimeUnit AskUnit(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                Console.WriteLine("  1. Minutes");
                Console.WriteLine("  2. Hours");
                Console.Write("  Choice: ");
                string? input = Console.ReadLine();
                if (input == "1") return TimeUnit.Minutes;
                if (input == "2") return TimeUnit.Hours;
                Console.WriteLine("  [Invalid] Enter 1 or 2.");
            }
        }
    }

    public class QueueInputs
    {
        public double   Lambda     { get; private set; }
        public double   Mu         { get; private set; }
        public double   VarService { get; private set; }
        public double   VarArrival { get; private set; }
        public int      Servers    { get; private set; }
        public TimeUnit BaseUnit   { get; private set; }

        private QueueInputs() { }

        // ── Main input collector ──────────────────────────────────────
        public static QueueInputs CollectMM(string modelName,
            bool needVarService = false,
            bool needVarArrival = false,
            bool needServers    = false)
        {
            var inp = new QueueInputs();
            Console.WriteLine($"\n  ── {modelName} Input ──");

            // ── LAMBDA ───────────────────────────────────────────────
            // Ask: is input a rate or a mean time?
            inp.BaseUnit = UnitConverter.AskUnit("\n  What unit is your ARRIVAL input in?");
            string bu = UnitConverter.UnitLabel(inp.BaseUnit);

            Console.WriteLine($"\n  Is your arrival input a RATE or a MEAN TIME?");
            Console.WriteLine($"  1. Arrival RATE  (e.g. 10 customers per {bu})");
            Console.WriteLine($"  2. Mean INTER-ARRIVAL TIME  (e.g. 10 {bu} between arrivals)");
            Console.Write("  Choice: ");
            string? lambdaType = Console.ReadLine();

            if (lambdaType == "2")
            {
                double meanArrival = ReadPositiveDouble($"  Mean inter-arrival time [{bu}]: ");
                inp.Lambda = 1.0 / meanArrival;
                Console.WriteLine($"  → λ = 1 / {meanArrival} = {inp.Lambda:F6} per {bu}");
            }
            else
            {
                inp.Lambda = ReadPositiveDouble($"  Arrival Rate (λ) [per {bu}]: ");
            }

            // ── MU ────────────────────────────────────────────────────
            TimeUnit muUnit = UnitConverter.AskUnit($"\n  What unit is your SERVICE input in?");
            string   mu_bu  = UnitConverter.UnitLabel(muUnit);

            Console.WriteLine($"\n  Is your service input a RATE or a MEAN TIME?");
            Console.WriteLine($"  1. Service RATE  (e.g. 6 customers per {mu_bu})");
            Console.WriteLine($"  2. Mean SERVICE TIME  (e.g. 10 {mu_bu} per customer)");
            Console.Write("  Choice: ");
            string? muType = Console.ReadLine();

            double muInOwnUnit;
            if (muType == "2")
            {
                double meanSvc = ReadPositiveDouble($"  Mean service time [{mu_bu}]: ");
                muInOwnUnit = 1.0 / meanSvc;
                Console.WriteLine($"  → μ = 1 / {meanSvc} = {muInOwnUnit:F6} per {mu_bu}");
            }
            else
            {
                muInOwnUnit = ReadPositiveDouble($"  Service Rate (μ) [per {mu_bu}]: ");
            }

            // Convert μ to base unit
            inp.Mu = UnitConverter.ConvertRate(muInOwnUnit, muUnit, inp.BaseUnit);
            if (muUnit != inp.BaseUnit)
                Console.WriteLine($"  → μ converted to {bu}: {inp.Mu:F6} per {bu}");

            // ── SERVICE VARIANCE ──────────────────────────────────────
            if (needVarService)
            {
                Console.WriteLine("\n  Service time variance — choose input method:");
                Console.WriteLine("  1. Enter variance directly (σ²)");
                Console.WriteLine("  2. Enter std deviation (σ)");
                Console.WriteLine("  3. Enter min & max (uniform distribution)");
                Console.Write("  Choice: ");
                string? vc = Console.ReadLine();

                TimeUnit varUnit = UnitConverter.AskUnit("  What unit is this service time value in?");
                string   vu      = UnitConverter.UnitLabel(varUnit);
                double   rawVar;

                if (vc == "2")
                {
                    double sigma = ReadPositiveDouble($"  Std deviation (σ) [{vu}]: ");
                    rawVar = sigma * sigma;
                    Console.WriteLine($"  → Variance = σ² = {rawVar:F6} {vu}²");
                }
                else if (vc == "3")
                {
                    double minT = ReadPositiveDouble($"  Min service time [{vu}]: ");
                    double maxT = ReadPositiveDouble($"  Max service time [{vu}]: ");
                    rawVar = Math.Pow(maxT - minT, 2) / 12.0;
                    Console.WriteLine($"  → Variance = (max-min)²/12 = {rawVar:F6} {vu}²");
                }
                else
                {
                    rawVar = ReadPositiveDouble($"  Variance (σ²s) [{vu}²]: ");
                }

                // Always convert variance to base unit² (even if same unit, no-op)
                inp.VarService = UnitConverter.ConvertVariance(rawVar, varUnit, inp.BaseUnit);
                if (varUnit != inp.BaseUnit)
                    Console.WriteLine($"  → Variance converted to {bu}²: {inp.VarService:F6} {bu}²");
            }

            // ── ARRIVAL VARIANCE (GG1 only) ───────────────────────────
            if (needVarArrival)
            {
                Console.WriteLine("\n  Inter-arrival time variance — choose input method:");
                Console.WriteLine("  1. Enter variance directly (σ²a)");
                Console.WriteLine("  2. Enter min & max (uniform distribution)");
                Console.Write("  Choice: ");
                string? vc2 = Console.ReadLine();

                TimeUnit vaUnit = UnitConverter.AskUnit("  What unit is this inter-arrival time value in?");
                string   vu2    = UnitConverter.UnitLabel(vaUnit);
                double   rawVarA;

                if (vc2 == "2")
                {
                    double minA = ReadPositiveDouble($"  Min inter-arrival time [{vu2}]: ");
                    double maxA = ReadPositiveDouble($"  Max inter-arrival time [{vu2}]: ");
                    rawVarA = Math.Pow(maxA - minA, 2) / 12.0;
                    Console.WriteLine($"  → Arrival Variance = {rawVarA:F6} {vu2}²");
                }
                else
                {
                    rawVarA = ReadPositiveDouble($"  Arrival Variance (σ²a) [{vu2}²]: ");
                }

                inp.VarArrival = UnitConverter.ConvertVariance(rawVarA, vaUnit, inp.BaseUnit);
                if (vaUnit != inp.BaseUnit)
                    Console.WriteLine($"  → Arrival variance converted to {bu}²: {inp.VarArrival:F6} {bu}²");
            }

            // ── SERVERS ───────────────────────────────────────────────
            if (needServers)
                inp.Servers = ReadPositiveInt("  Number of Servers (S): ");
            else
                inp.Servers = 1;

            return inp;
        }

        public static double ReadPositiveDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double v) && v > 0) return v;
                Console.WriteLine("  [Invalid] Enter a positive number.");
            }
        }

        public static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= 1) return v;
                Console.WriteLine("  [Invalid] Enter an integer >= 1.");
            }
        }
    }

    // ── Results printer ───────────────────────────────────────────────
    public class QueueResults
    {
        public double   Lq       { get; set; }
        public double   L        { get; set; }
        public double   Wq       { get; set; }
        public double   W        { get; set; }
        public double   Rho      { get; set; }
        public double   IdleTime { get; set; }
        public double   P0       { get; set; }
        public TimeUnit BaseUnit { get; set; }
        public string   ModelName{ get; set; } = "";
        public List<string> ExtraLines { get; set; } = new();

        public string FormatForDisplay()
        {
            string   bu  = UnitConverter.UnitLabel(BaseUnit);
            TimeUnit alt = (BaseUnit == TimeUnit.Minutes) ? TimeUnit.Hours : TimeUnit.Minutes;
            string   au  = UnitConverter.UnitLabel(alt);

            double WqAlt = UnitConverter.ConvertTime(Wq, BaseUnit, alt);
            double WAlt  = UnitConverter.ConvertTime(W,  BaseUnit, alt);

            var lines = new List<string>
            {
                $"========== {ModelName} Queue Results =========="
            };

            lines.AddRange(ExtraLines);

            if (ExtraLines.Count > 0)
                lines.Add("");

            lines.Add($"  Traffic Intensity      (ρ)   = {Rho:F4}");
            if (P0 > 0)
                lines.Add($"  P0 (system empty)            = {P0:F4}");

            lines.Add("");
            lines.Add($"  Avg. in Queue          (Lq)  = {Lq:F4}  customers");
            lines.Add($"  Avg. in System         (L)   = {L:F4}  customers");
            lines.Add("");
            lines.Add($"  Avg. Wait in Queue     (Wq)  = {Wq:F6} {bu}");
            lines.Add($"                               = {WqAlt:F4} {au}");
            lines.Add($"  Avg. Time in System    (W)   = {W:F6} {bu}");
            lines.Add($"                               = {WAlt:F4} {au}");
            lines.Add("");
            lines.Add($"  Server Idle Time             = {IdleTime:F4}  ({IdleTime * 100:F2}%)");
            lines.Add("==========================================");

            return string.Join(Environment.NewLine, lines);
        }

        public void Print() => Console.WriteLine(FormatForDisplay() + Environment.NewLine);
    }
}
