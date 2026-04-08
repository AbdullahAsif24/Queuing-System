using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QueueingSystem
{
    // ═══════════════════════════════════════════
    //  Customer data
    // ═══════════════════════════════════════════
    public class Customer
    {
        public int Id { get; set; }
        public double Arrival { get; set; }
        public double SvcStart { get; set; }
        public double SvcTime { get; set; }
        public double Depart { get; set; }
        public double WaitTime => SvcStart - Arrival;
        public double SystemTime => Depart - Arrival;
    }

    // ═══════════════════════════════════════════
    //  Visual ball for animation
    // ═══════════════════════════════════════════
    public class VisCust
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public Color Col { get; set; }
        public float Alpha { get; set; } = 1f;
        public string State { get; set; } = "moving";   // moving | queuing | serving | done
        public float SvcProg { get; set; }
    }

    // ═══════════════════════════════════════════
    //  Entry point
    // ═══════════════════════════════════════════
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // ═══════════════════════════════════════════
    //  MAIN FORM  –  Light Theme
    // ═══════════════════════════════════════════
    public class MainForm : Form
    {
        // ── Light-theme palette ──────────────────
        static readonly Color BG = Color.FromArgb(245, 247, 252);
        static readonly Color PANEL = Color.FromArgb(235, 238, 248);
        static readonly Color CARD = Color.White;
        static readonly Color ACCENT = Color.FromArgb(37, 99, 235);    // blue-600
        static readonly Color ACCENT2 = Color.FromArgb(5, 150, 105);    // emerald-600
        static readonly Color TEXT = Color.FromArgb(15, 23, 42);     // slate-900
        static readonly Color DIM = Color.FromArgb(100, 116, 139);  // slate-500
        static readonly Color INPUTBG = Color.FromArgb(248, 250, 252);
        static readonly Color BORDER = Color.FromArgb(203, 213, 225);  // slate-300
        static readonly Color OK = Color.FromArgb(5, 150, 105);
        static readonly Color ERR = Color.FromArgb(220, 38, 38);
        static readonly Color ORANGE = Color.FromArgb(234, 88, 12);    // orange-600
        static readonly Color QBOX = Color.FromArgb(219, 234, 254);  // blue-100
        static readonly Color SVCBOX = Color.FromArgb(209, 250, 229);  // emerald-100

        // Tabs
        TabControl mainTab;
        TabPage tabQ, tabS;

        // Queue tab
        TextBox qArr, qSvc;
        RadioButton qNorm, qUnif, qGamma;
        Label qRho, qL, qLq, qW, qWq, qP0, qStat;

        // Sim tab
        TextBox sArr, sSvc, sCnt;
        RadioButton sExp, sNorm, sUnif;
        Label sWq, sW, sRho, sSvd, sStat;
        Button sBtnRun;
        TrackBar sSpd;
        Label sSpdLbl;
        Panel sCanvas;
        ListBox sLog;

        // Sim state
        List<Customer> simData = new List<Customer>();
        List<VisCust> visuals = new List<VisCust>();
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        double simT = 0, simEnd = 0;
        bool running = false;
        int spawnIdx = 0;
        Random rng = new Random();

        // ── Canvas layout constants ──────────────
        // Queue box:  (QX,QY) top-left, QW×QH size
        const int QX = 20, QY = 44, QW = 210, QH = 80;
        // Server circle centre
        const int SX = 390, SY = 84;
        const int SR = 36;               // radius
        // Exit point
        const int EX = 500, EY = 84;
        // Ball radius
        const int BR = 13;

        // ─────────────────────────────────────────
        public MainForm()
        {
            Text = "Queueing Theory Simulator — M/M/1";
            Size = new Size(880, 700);
            MinimumSize = new Size(840, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BG;
            ForeColor = TEXT;
            Font = new Font("Segoe UI", 9.5f);

            BuildTabs();
            timer.Interval = 30;
            timer.Tick += Tick;
        }

        // ═══════════════════════════════════════
        //  TABS
        // ═══════════════════════════════════════
        void BuildTabs()
        {
            mainTab = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(175, 38),
                SizeMode = TabSizeMode.Fixed,
                Font = new Font("Segoe UI Semibold", 10f)
            };
            mainTab.DrawItem += (s, e) =>
            {
                bool sel = e.Index == mainTab.SelectedIndex;
                using var bg = new SolidBrush(sel ? ACCENT : PANEL);
                using var fg = new SolidBrush(sel ? Color.White : DIM);
                e.Graphics.FillRectangle(bg, e.Bounds);
                if (sel)
                {
                    using var a2 = new SolidBrush(ACCENT2);
                    e.Graphics.FillRectangle(a2, e.Bounds.X, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
                }
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(mainTab.TabPages[e.Index].Text, mainTab.Font, fg, e.Bounds, sf);
            };

            tabQ = new TabPage("  Queueing System") { BackColor = BG, BorderStyle = BorderStyle.None };
            tabS = new TabPage("  Simulator") { BackColor = BG, BorderStyle = BorderStyle.None };
            mainTab.TabPages.Add(tabQ);
            mainTab.TabPages.Add(tabS);

            BuildQTab();
            BuildSTab();
            Controls.Add(mainTab);
        }

        // ═══════════════════════════════════════
        //  QUEUEING TAB
        // ═══════════════════════════════════════
        void BuildQTab()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = BG, Padding = new Padding(18, 14, 18, 14) };

            Lbl(p, "M/M/1  ·  Single Server Queue", new Font("Segoe UI", 16f, FontStyle.Bold), ACCENT, 0, 0);
            Lbl(p, "Mean-Value Analysis  |  Enter mean times below", new Font("Segoe UI", 9f), DIM, 2, 30);

            // Input card
            var ic = Card(p, 0, 60, 355, 295, "  INPUT PARAMETERS");
            int y = 44;
            qArr = Inp(ic, "Mean Inter-Arrival Time  (1/λ):", ref y);
            qSvc = Inp(ic, "Mean Service Time  (1/μ):", ref y);
            var locked = Inp(ic, "Number of Servers:", ref y);
            locked.Text = "1 (fixed)"; locked.Enabled = false; locked.ForeColor = DIM;

            ic.Controls.Add(new Label { Text = "Distribution:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(14, y) });
            y += 22;
            qNorm = Radio(ic, "Normal", 14, y);
            qUnif = Radio(ic, "Uniform", 110, y);
            qGamma = Radio(ic, "Gamma", 210, y, true);

            // Results card
            var rc = Card(p, 370, 60, 378, 295, "  PERFORMANCE MEASURES");
            int ry = 44;
            qRho = Res(rc, "ρ   Traffic Intensity", ref ry);
            qL = Res(rc, "L   Avg in System", ref ry);
            qLq = Res(rc, "Lq  Avg in Queue", ref ry);
            qW = Res(rc, "W   Avg Time in System", ref ry);
            qWq = Res(rc, "Wq  Avg Time in Queue", ref ry);
            qP0 = Res(rc, "P₀  Idle Probability", ref ry);

            var calc = Btn(p, "▶   Calculate Performance Measures", 0, 362, 308, 40, ACCENT);
            var clear = Btn(p, "⟳  Clear", 314, 362, 96, 40, PANEL);
            clear.ForeColor = DIM;
            calc.Click += (s, e) => DoCalc();
            clear.Click += (s, e) => DoClear();

            qStat = new Label
            {
                Text = "Enter values and click Calculate.",
                ForeColor = DIM,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false,
                Size = new Size(748, 22),
                Location = new Point(0, 412)
            };
            p.Controls.Add(qStat);

            // Formula reference card
            var fc = Card(p, 0, 444, 748, 108, "  M/M/1 FORMULAS REFERENCE");
            fc.Controls.Add(new Label
            {
                Text = "ρ=λ/μ   P₀=1−ρ   L=ρ/(1−ρ)   Lq=ρ²/(1−ρ)   W=1/(μ−λ)   Wq=λ/(μ(μ−λ))",
                ForeColor = ACCENT2,
                Font = new Font("Courier New", 8.5f),
                AutoSize = false,
                Size = new Size(720, 20),
                Location = new Point(14, 38)
            });
            fc.Controls.Add(new Label
            {
                Text = "where  λ=1/MeanArrival   μ=1/MeanService   (valid only when ρ < 1)",
                ForeColor = DIM,
                Font = new Font("Segoe UI", 8f),
                AutoSize = false,
                Size = new Size(720, 18),
                Location = new Point(14, 62)
            });

            tabQ.Controls.Add(p);
        }

        void DoCalc()
        {
            if (!double.TryParse(qArr.Text.Trim(), out double mA) || mA <= 0)
            { QStat("❌  Invalid Mean Inter-Arrival Time.", ERR); return; }
            if (!double.TryParse(qSvc.Text.Trim(), out double mS) || mS <= 0)
            { QStat("❌  Invalid Mean Service Time.", ERR); return; }

            double lam = 1.0 / mA, mu = 1.0 / mS, rho = lam / mu;
            if (rho >= 1)
            {
                QStat($"⚠️  System UNSTABLE (ρ={rho:F4} ≥ 1). Reduce arrival rate or increase service rate.", ERR);
                ClearRes(); return;
            }
            double P0 = 1 - rho;
            double L = rho / (1 - rho);
            double Lq = rho * rho / (1 - rho);
            double W = 1.0 / (mu - lam);
            double Wq = lam / (mu * (mu - lam));

            Set(qRho, $"{rho:F6}"); Set(qL, $"{L:F6}"); Set(qLq, $"{Lq:F6}");
            Set(qW, $"{W:F6}"); Set(qWq, $"{Wq:F6}"); Set(qP0, $"{P0:F6}");

            string d = qNorm.Checked ? "Normal" : qUnif.Checked ? "Uniform" : "Gamma";
            QStat($"✔  Calculated  |  λ={lam:F4}  μ={mu:F4}  ρ={rho:F4}  |  Dist: {d}", OK);
        }

        void DoClear()
        {
            qArr.Clear(); qSvc.Clear(); qGamma.Checked = true;
            ClearRes(); QStat("Cleared.", DIM);
        }

        void ClearRes()
        {
            foreach (var l in new[] { qRho, qL, qLq, qW, qWq, qP0 })
            { l.Text = "—"; l.ForeColor = DIM; }
        }

        void QStat(string m, Color c) { qStat.Text = "  " + m; qStat.ForeColor = c; }
        void Set(Label l, string v) { l.Text = v; l.ForeColor = ACCENT2; }

        // ═══════════════════════════════════════
        //  SIMULATOR TAB
        // ═══════════════════════════════════════
        void BuildSTab()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = BG, Padding = new Padding(18, 14, 18, 14) };

            Lbl(p, "M/M/1  ·  Arrival Simulator", new Font("Segoe UI", 16f, FontStyle.Bold), ACCENT, 0, 0);
            Lbl(p, "Watch customers arrive, queue, get served and depart in real time", new Font("Segoe UI", 9f), DIM, 2, 30);

            // ── Parameters card ─────────────────
            var ic = Card(p, 0, 56, 500, 140, "  SIMULATION PARAMETERS");

            ic.Controls.Add(new Label { Text = "Mean Inter-Arrival Time:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(12, 40) });
            sArr = new TextBox { Location = new Point(12, 57), Size = new Size(120, 24), BackColor = INPUTBG, ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f), Text = "4" };
            ic.Controls.Add(sArr);

            ic.Controls.Add(new Label { Text = "Mean Service Time:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(148, 40) });
            sSvc = new TextBox { Location = new Point(148, 57), Size = new Size(120, 24), BackColor = INPUTBG, ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f), Text = "3" };
            ic.Controls.Add(sSvc);

            ic.Controls.Add(new Label { Text = "Customers:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(284, 40) });
            sCnt = new TextBox { Location = new Point(284, 57), Size = new Size(55, 24), BackColor = INPUTBG, ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f), Text = "15" };
            ic.Controls.Add(sCnt);

            ic.Controls.Add(new Label { Text = "Speed:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(355, 40) });
            sSpd = new TrackBar { Minimum = 1, Maximum = 12, Value = 4, TickFrequency = 1, Location = new Point(355, 55), Size = new Size(105, 30), BackColor = Color.White };
            sSpdLbl = new Label { Text = "4x", ForeColor = ACCENT, Font = new Font("Segoe UI Semibold", 9f), AutoSize = true, Location = new Point(390, 88) };
            sSpd.ValueChanged += (s, e) => sSpdLbl.Text = sSpd.Value + "x";
            ic.Controls.Add(sSpd); ic.Controls.Add(sSpdLbl);

            ic.Controls.Add(new Label { Text = "Distribution:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(12, 94) });
            sExp = Radio(ic, "Exponential", 100, 94, true);
            sNorm = Radio(ic, "Normal", 210, 94);
            sUnif = Radio(ic, "Uniform", 285, 94);

            // ── Buttons & log ───────────────────
            sBtnRun = Btn(p, "▶   Run Simulation", 515, 56, 225, 42, ACCENT);
            sBtnRun.Click += (s, e) => SimRun();

            var rst = Btn(p, "⟳  Reset", 515, 106, 225, 40, PANEL);
            rst.ForeColor = DIM;
            rst.Click += (s, e) => SimReset();

            sLog = new ListBox
            {
                Location = new Point(515, 156),
                Size = new Size(225, 115),
                BackColor = INPUTBG,
                ForeColor = DIM,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 7.5f)
            };
            p.Controls.Add(sLog);

            // ── Animation canvas ─────────────────
            sCanvas = new Panel
            {
                Location = new Point(0, 205),
                Size = new Size(740, 200),
                BackColor = Color.White
            };
            sCanvas.Paint += DrawCanvas;
            p.Controls.Add(sCanvas);

            // ── Stats card ───────────────────────
            var sc = Card(p, 0, 415, 740, 70, "  SIMULATION RESULTS");
            sWq = StatLbl(sc, "Avg Wait  Wq:", 10);
            sW = StatLbl(sc, "Avg Time  W:", 195);
            sRho = StatLbl(sc, "Utilisation  ρ:", 375);
            sSvd = StatLbl(sc, "Served:", 565);

            // ── Status bar ───────────────────────
            sStat = new Label
            {
                Text = "Press Run to start.",
                ForeColor = DIM,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false,
                Size = new Size(740, 22),
                Location = new Point(0, 493)
            };
            p.Controls.Add(sStat);

            tabS.Controls.Add(p);
        }

        // ─── Random variate generation ─────────
        double Rand(double mean, string dist)
        {
            double v;
            switch (dist)
            {
                case "exp":
                    v = -mean * Math.Log(rng.NextDouble() + 1e-12);
                    break;
                case "normal":
                    {
                        // Box-Muller
                        double u1 = rng.NextDouble() + 1e-12;
                        double u2 = rng.NextDouble();
                        v = mean + mean * 0.3 * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                        break;
                    }
                default: // uniform: [0.5×mean , 1.5×mean]
                    v = mean * 0.5 + rng.NextDouble() * mean;
                    break;
            }
            return Math.Max(0.05, v);
        }

        // ─── Build & start simulation ──────────
        void SimRun()
        {
            if (!double.TryParse(sArr.Text.Trim(), out double mA) || mA <= 0)
            { SStat("❌  Invalid Mean Inter-Arrival Time.", ERR); return; }
            if (!double.TryParse(sSvc.Text.Trim(), out double mS) || mS <= 0)
            { SStat("❌  Invalid Mean Service Time.", ERR); return; }
            if (!int.TryParse(sCnt.Text.Trim(), out int n) || n < 2 || n > 50)
            { SStat("❌  Customers must be 2–50.", ERR); return; }

            string dist = sNorm.Checked ? "normal" : sUnif.Checked ? "uniform" : "exp";

            // ── Generate event schedule ───────────
            simData.Clear();
            double clock = 0, serverFree = 0;
            for (int i = 0; i < n; i++)
            {
                clock += Rand(mA, dist);
                double arrival = clock;
                double svcStart = Math.Max(arrival, serverFree);
                double svcTime = Rand(mS, dist);
                double depart = svcStart + svcTime;
                serverFree = depart;
                simData.Add(new Customer
                {
                    Id = i + 1,
                    Arrival = arrival,
                    SvcStart = svcStart,
                    SvcTime = svcTime,
                    Depart = depart
                });
            }
            simEnd = simData[simData.Count - 1].Depart + 1.5;

            // ── Fill log ──────────────────────────
            sLog.Items.Clear();
            foreach (var c in simData)
            {
                sLog.Items.Add($"t={c.Arrival:F2}  C{c.Id} arrives" +
                               (c.WaitTime > 0.01 ? $" → waits {c.WaitTime:F2}" : " → serves now"));
                sLog.Items.Add($"t={c.SvcStart:F2}  C{c.Id} served ({c.SvcTime:F2})");
                sLog.Items.Add($"t={c.Depart:F2}  C{c.Id} departs");
            }

            // ── Reset animation state ─────────────
            simT = 0; visuals.Clear(); spawnIdx = 0; running = true;
            foreach (var l in new[] { sWq, sW, sRho, sSvd })
            { l.Text = "—"; l.ForeColor = DIM; }

            SStat("Simulation running…", ACCENT);
            sBtnRun.Enabled = false;
            timer.Start();
        }

        void SimReset()
        {
            timer.Stop(); running = false;
            simData.Clear(); visuals.Clear(); sLog.Items.Clear();
            foreach (var l in new[] { sWq, sW, sRho, sSvd })
            { l.Text = "—"; l.ForeColor = DIM; }
            SStat("Press Run to start.", DIM);
            sBtnRun.Enabled = true;
            sCanvas.Invalidate();
        }

        // ─── Animation tick ────────────────────
        void Tick(object sender, EventArgs e)
        {
            if (!running) return;

            // Advance simulated time
            simT += (timer.Interval / 1000.0) * sSpd.Value;

            // Spawn arrivals whose arrival time has passed
            while (spawnIdx < simData.Count && simData[spawnIdx].Arrival <= simT)
            {
                var c = simData[spawnIdx++];
                // Start just off the left edge of the canvas at queue's vertical centre
                visuals.Add(new VisCust
                {
                    Id = c.Id,
                    X = -BR - 2,
                    Y = QY + QH / 2f,
                    Col = ORANGE,
                    State = "moving"
                });
            }

            // ── Update each ball ─────────────────
            foreach (var v in visuals)
            {
                var c = simData[v.Id - 1];

                if (simT >= c.Depart + 0.5)
                {
                    // ── Departing: slide right and fade out ──
                    v.State = "done";
                    v.Alpha = Math.Max(0f, v.Alpha - 0.06f);
                    v.X += 3f;
                }
                else if (simT >= c.SvcStart)
                {
                    // ── Being served: move to server circle centre ──
                    v.State = "serving";
                    v.Col = OK;
                    v.X = Ease(v.X, SX, 8);
                    v.Y = Ease(v.Y, SY, 8);
                    v.SvcProg = (float)Math.Min(1.0, (simT - c.SvcStart) / c.SvcTime);
                }
                else if (simT >= c.Arrival)
                {
                    // ── Waiting in queue ──────────────────────
                    v.State = "queuing";
                    v.Col = ORANGE;

                    // Count how many OTHER balls are already queuing ahead of this one
                    int slot = 0;
                    foreach (var o in visuals)
                    {
                        if (o.Id == v.Id) break;          // stop when we reach ourselves
                        if (o.State == "queuing") slot++;  // count predecessors in queue
                    }

                    // Pack balls from the right end of the queue box leftward
                    float targetX = QX + QW - BR - 4 - slot * (BR * 2 + 4);
                    float targetY = QY + QH / 2f;

                    // Clamp so balls don't overflow the left of the queue box
                    targetX = Math.Max(QX + BR + 4, targetX);

                    v.X = Ease(v.X, targetX, 5);
                    v.Y = Ease(v.Y, targetY, 5);
                }
                // else: still moving toward queue (state = "moving"), let it drift right
                else
                {
                    v.X = Ease(v.X, QX + QW - BR - 4, 5);
                    v.Y = Ease(v.Y, QY + QH / 2f, 5);
                }
            }

            // Remove fully faded balls
            visuals.RemoveAll(v => v.State == "done" && v.Alpha <= 0f);

            // ── Check for completion ──────────────
            if (simT >= simEnd && spawnIdx >= simData.Count && visuals.Count == 0)
            {
                timer.Stop(); running = false;
                ShowStats();
                sBtnRun.Enabled = true;
                SStat("✔  Simulation complete.", OK);
            }

            sCanvas.Invalidate();
        }

        float Ease(float cur, float tgt, float spd)
            => Math.Abs(cur - tgt) < spd ? tgt : cur + Math.Sign(tgt - cur) * spd;

        // ─── Canvas rendering ──────────────────
        void DrawCanvas(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int cw = sCanvas.Width, ch = sCanvas.Height;
            g.Clear(Color.White);

            // ── Shadow / drop-shadow helper via border ──

            // ── Queue box ────────────────────────
            FillRR(g, QX, QY, QW, QH, QBOX, 8);
            OutRR(g, QX, QY, QW, QH, BORDER, 1.2f, 8);
            DText(g, "QUEUE", QX + 10, QY + 8, DIM, 8f);

            // Queue entry arrow (from left into queue)
            DArrow(g, 4, QY + QH / 2, QX - 2, QY + QH / 2, DIM);
            DText(g, "arrive", 0, QY + QH / 2 + 6, DIM, 7.5f);

            // ── Arrow: queue exit → server ────────
            int arrowX1 = QX + QW + 2;
            int arrowX2 = SX - SR - 4;
            int arrowY = SY;
            DArrow(g, arrowX1, arrowY, arrowX2, arrowY, DIM);

            // ── Server circle ─────────────────────
            g.FillEllipse(new SolidBrush(SVCBOX), SX - SR, SY - SR, SR * 2, SR * 2);
            g.DrawEllipse(new Pen(BORDER, 1.2f), SX - SR, SY - SR, SR * 2, SR * 2);
            DTextC(g, "SERVER", SX, SY + SR + 8, DIM, 8f);

            // Service-progress arc (drawn outside the circle)
            var sv = visuals.Find(v => v.State == "serving");
            if (sv != null && sv.SvcProg > 0)
            {
                int margin = 5;
                var arcRect = new Rectangle(SX - SR - margin, SY - SR - margin,
                                            (SR + margin) * 2, (SR + margin) * 2);
                using var arcPen = new Pen(ACCENT2, 3f);
                g.DrawArc(arcPen, arcRect, -90f, 360f * sv.SvcProg);
            }

            // ── Arrow: server → exit ──────────────
            DArrow(g, SX + SR + 4, SY, EX + 10, EY, DIM);
            DText(g, "depart", EX + 12, EY - 14, DIM, 7.5f);

            // ── Progress bar (bottom of canvas) ───
            if (simData.Count > 0 && simEnd > 0)
            {
                float prog = (float)Math.Min(1.0, simT / simEnd);
                int bx = QX, by = ch - 18, bw = EX + 50, bh = 5;
                FillRR(g, bx, by, bw, bh, BORDER, 3);
                if (prog > 0 && (int)(bw * prog) > 0)
                    FillRR(g, bx, by, (int)(bw * prog), bh, ACCENT, 3);
                DText(g, $"t = {simT:F1}", bx + bw + 8, by - 2, DIM, 8f);
            }

            // ── Legend ────────────────────────────
            DBall(g, cw - 55, ch - 30, 9, ORANGE, "");
            DText(g, "Waiting", cw - 42, ch - 36, DIM, 7.5f);
            DBall(g, cw - 55, ch - 14, 9, OK, "");
            DText(g, "Serving", cw - 42, ch - 20, DIM, 7.5f);

            // ── Draw balls ────────────────────────
            foreach (var v in visuals)
            {
                if (v.Alpha <= 0) continue;
                int a = (int)(255 * v.Alpha);
                DBall(g, (int)v.X, (int)v.Y, BR, Color.FromArgb(a, v.Col), v.Id.ToString());
            }

            // ── Queue count label ─────────────────
            int qCount = visuals.FindAll(v => v.State == "queuing").Count;
            DText(g, $"Waiting: {qCount}", QX + 6, QY + QH + 8, DIM, 8f);
        }

        // ─── Drawing helpers ───────────────────
        void DBall(Graphics g, int cx, int cy, int r, Color c, string id)
        {
            // Shadow
            using var shadow = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
            g.FillEllipse(shadow, cx - r + 2, cy - r + 2, r * 2, r * 2);
            // Ball
            g.FillEllipse(new SolidBrush(c), cx - r, cy - r, r * 2, r * 2);
            // Highlight
            g.FillEllipse(new SolidBrush(Color.FromArgb(60, 255, 255, 255)),
                          cx - r + 3, cy - r + 2, r - 2, (r - 2) / 2);
            // Label
            if (!string.IsNullOrEmpty(id))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(id, new Font("Segoe UI Semibold", 7.5f), Brushes.White,
                             new RectangleF(cx - r, cy - r, r * 2, r * 2), sf);
            }
        }

        void FillRR(Graphics g, int x, int y, int w, int h, Color c, int r)
        {
            if (w <= 0 || h <= 0) return;
            r = Math.Min(r, Math.Min(w / 2, h / 2));
            if (r <= 0) { using var b = new SolidBrush(c); g.FillRectangle(b, x, y, w, h); return; }
            using var path = new GraphicsPath();
            int d = r * 2;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            using var br = new SolidBrush(c); g.FillPath(br, path);
        }

        void OutRR(Graphics g, int x, int y, int w, int h, Color c, float pw, int r)
        {
            r = Math.Min(r, Math.Min(w / 2, h / 2));
            using var path = new GraphicsPath();
            int d = r * 2;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            g.DrawPath(new Pen(c, pw), path);
        }

        void DArrow(Graphics g, int x1, int y1, int x2, int y2, Color c)
        {
            using var pen = new Pen(c, 1.5f) { EndCap = LineCap.ArrowAnchor };
            g.DrawLine(pen, x1, y1, x2, y2);
        }

        void DText(Graphics g, string t, int x, int y, Color c, float sz)
            => g.DrawString(t, new Font("Segoe UI", sz), new SolidBrush(c), x, y);

        void DTextC(Graphics g, string t, int cx, int y, Color c, float sz)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(t, new Font("Segoe UI", sz), new SolidBrush(c), cx, y, sf);
        }

        // ─── Show final stats ──────────────────
        void ShowStats()
        {
            if (simData.Count == 0) return;
            int n = simData.Count;
            double totalWq = 0, totalW = 0, totalBusy = 0;
            foreach (var c in simData)
            {
                totalWq += c.WaitTime;
                totalW += c.SystemTime;
                totalBusy += c.SvcTime;
            }
            double rho = totalBusy / simEnd;

            void S(Label l, string v) { l.Text = v; l.ForeColor = ACCENT2; }
            S(sWq, $"{totalWq / n:F3}");
            S(sW, $"{totalW / n:F3}");
            S(sRho, $"{rho:F3}");
            S(sSvd, $"{n}");
        }

        void SStat(string m, Color c) { sStat.Text = "  " + m; sStat.ForeColor = c; }

        // ═══════════════════════════════════════
        //  UI helpers
        // ═══════════════════════════════════════
        Panel Card(Panel parent, int x, int y, int w, int h, string title)
        {
            var card = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = CARD };

            // Thin border
            card.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(BORDER, 1f), 0, 0, card.Width - 1, card.Height - 1);
            };

            var hdr = new Label
            {
                Text = title,
                ForeColor = ACCENT,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(w, 26),
                Location = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(239, 246, 255),   // light blue header tint
                Padding = new Padding(8, 0, 0, 0)
            };
            card.Controls.Add(hdr);
            parent.Controls.Add(card);
            return card;
        }

        TextBox Inp(Panel parent, string lbl, ref int y)
        {
            parent.Controls.Add(new Label
            {
                Text = lbl,
                ForeColor = DIM,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(14, y)
            });
            y += 20;
            var tb = new TextBox
            {
                Location = new Point(14, y),
                Size = new Size(318, 26),
                BackColor = INPUTBG,
                ForeColor = TEXT,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            parent.Controls.Add(tb);
            y += 36;
            return tb;
        }

        RadioButton Radio(Panel p, string t, int x, int y, bool chk = false)
        {
            var rb = new RadioButton
            {
                Text = t,
                ForeColor = TEXT,
                BackColor = Color.Transparent,
                Location = new Point(x, y),
                AutoSize = true,
                Checked = chk
            };
            p.Controls.Add(rb);
            return rb;
        }

        Label Res(Panel parent, string name, ref int y)
        {
            parent.Controls.Add(new Label
            {
                Text = name,
                ForeColor = DIM,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(14, y + 2)
            });
            var v = new Label
            {
                Text = "—",
                ForeColor = DIM,
                Font = new Font("Courier New", 10f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(256, y + 2)
            };
            parent.Controls.Add(v);
            y += 34;
            return v;
        }

        Label StatLbl(Panel parent, string name, int x)
        {
            parent.Controls.Add(new Label
            {
                Text = name,
                ForeColor = DIM,
                Font = new Font("Segoe UI", 8f),
                AutoSize = true,
                Location = new Point(x, 32)
            });
            var v = new Label
            {
                Text = "—",
                ForeColor = DIM,
                Font = new Font("Courier New", 10f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(x, 48)
            };
            parent.Controls.Add(v);
            return v;
        }

        Button Btn(Panel parent, string text, int x, int y, int w, int h, Color bg)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            parent.Controls.Add(b);
            return b;
        }

        void Lbl(Panel parent, string text, Font font, Color col, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Font = font,
                ForeColor = col,
                Location = new Point(x, y),
                AutoSize = false,
                Size = new Size(720, 28)
            });
        }
    }
}