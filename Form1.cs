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
        public string State { get; set; } = "moving";
        public float SvcProg { get; set; }
    }

    // ═══════════════════════════════════════════
    //  MAIN FORM
    // ═══════════════════════════════════════════
    public class MainForm : Form
    {
        // Colours
        static readonly Color BG = Color.FromArgb(15, 20, 35);
        static readonly Color PANEL = Color.FromArgb(25, 33, 55);
        static readonly Color CARD = Color.FromArgb(32, 43, 70);
        static readonly Color ACCENT = Color.FromArgb(64, 156, 255);
        static readonly Color ACCENT2 = Color.FromArgb(80, 220, 170);
        static readonly Color TEXT = Color.FromArgb(220, 230, 255);
        static readonly Color DIM = Color.FromArgb(120, 140, 180);
        static readonly Color INPUTBG = Color.FromArgb(18, 25, 45);
        static readonly Color OK = Color.FromArgb(80, 220, 130);
        static readonly Color ERR = Color.FromArgb(255, 90, 90);
        static readonly Color ORANGE = Color.FromArgb(255, 165, 60);

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

        // Canvas layout
        const int QX = 20, QY = 50, QW = 200, QH = 100;
        const int SX = 350, SY = 100, SR = 40;
        const int EX = 470, EY = 100;
        const int BR = 13;

        // ───────────────────────────────────────
        public MainForm()
        {
            Text = "Queueing Theory Simulator — M/M/1";
            Size = new Size(860, 680);
            MinimumSize = new Size(820, 620);
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
                ItemSize = new Size(170, 38),
                SizeMode = TabSizeMode.Fixed,
                Font = new Font("Segoe UI Semibold", 10f)
            };
            mainTab.DrawItem += (s, e) => {
                bool sel = e.Index == mainTab.SelectedIndex;
                using var bg = new SolidBrush(sel ? ACCENT : PANEL);
                using var fg = new SolidBrush(sel ? Color.White : DIM);
                e.Graphics.FillRectangle(bg, e.Bounds);
                if (sel) { using var a2 = new SolidBrush(ACCENT2); e.Graphics.FillRectangle(a2, e.Bounds.X, e.Bounds.Bottom - 3, e.Bounds.Width, 3); }
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

            var ic = Card(p, 0, 60, 350, 290, "  INPUT PARAMETERS");
            int y = 44;
            qArr = Inp(ic, "Mean Inter-Arrival Time  (1/λ):", ref y);
            qSvc = Inp(ic, "Mean Service Time  (1/μ):", ref y);
            var tb = Inp(ic, "Number of Servers:", ref y); tb.Text = "1 (fixed)"; tb.Enabled = false; tb.ForeColor = DIM;

            ic.Controls.Add(new Label { Text = "Distribution:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(14, y) });
            y += 22;
            qNorm = Radio(ic, "Normal", 14, y);
            qUnif = Radio(ic, "Uniform", 110, y);
            qGamma = Radio(ic, "Gamma", 210, y, true);

            var rc = Card(p, 365, 60, 375, 290, "  PERFORMANCE MEASURES");
            int ry = 44;
            qRho = Res(rc, "ρ   Traffic Intensity", ref ry);
            qL = Res(rc, "L   Avg in System", ref ry);
            qLq = Res(rc, "Lq  Avg in Queue", ref ry);
            qW = Res(rc, "W   Avg Time in System", ref ry);
            qWq = Res(rc, "Wq  Avg Time in Queue", ref ry);
            qP0 = Res(rc, "P₀  Idle Probability", ref ry);

            var calc = Btn(p, "▶   Calculate Performance Measures", 0, 358, 305, 42, ACCENT);
            var clear = Btn(p, "⟳  Clear", 311, 358, 95, 42, CARD);
            clear.ForeColor = DIM;
            calc.Click += (s, e) => DoCalc();
            clear.Click += (s, e) => DoClear();

            qStat = new Label { Text = "Enter values and click Calculate.", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = false, Size = new Size(740, 22), Location = new Point(0, 408) };
            p.Controls.Add(qStat);

            var fc = Card(p, 0, 440, 740, 108, "  M/M/1 FORMULAS REFERENCE");
            fc.Controls.Add(new Label { Text = "ρ=λ/μ   P₀=1−ρ   L=ρ/(1−ρ)   Lq=ρ²/(1−ρ)   W=1/(μ−λ)   Wq=λ/(μ(μ−λ))", ForeColor = ACCENT2, Font = new Font("Courier New", 8.5f), AutoSize = false, Size = new Size(700, 20), Location = new Point(14, 38) });
            fc.Controls.Add(new Label { Text = "where  λ=1/MeanArrival   μ=1/MeanService   (valid only when ρ < 1)", ForeColor = DIM, Font = new Font("Segoe UI", 8f), AutoSize = false, Size = new Size(700, 18), Location = new Point(14, 62) });

            tabQ.Controls.Add(p);
        }

        void DoCalc()
        {
            if (!double.TryParse(qArr.Text.Trim(), out double mA) || mA <= 0) { QStat("❌  Invalid Mean Inter-Arrival Time.", ERR); return; }
            if (!double.TryParse(qSvc.Text.Trim(), out double mS) || mS <= 0) { QStat("❌  Invalid Mean Service Time.", ERR); return; }
            double lam = 1 / mA, mu = 1 / mS, rho = lam / mu;
            if (rho >= 1) { QStat($"⚠️  System UNSTABLE (ρ={rho:F4}≥1). Reduce arrival or increase service rate.", ERR); ClearRes(); return; }
            double P0 = 1 - rho, L = rho / (1 - rho), Lq = rho * rho / (1 - rho), W = 1 / (mu - lam), Wq = lam / (mu * (mu - lam));
            Set(qRho, $"{rho:F6}"); Set(qL, $"{L:F6}"); Set(qLq, $"{Lq:F6}"); Set(qW, $"{W:F6}"); Set(qWq, $"{Wq:F6}"); Set(qP0, $"{P0:F6}");
            string d = qNorm.Checked ? "Normal" : qUnif.Checked ? "Uniform" : "Gamma";
            QStat($"✔  Calculated  |  λ={lam:F4}  μ={mu:F4}  ρ={rho:F4}  |  Dist: {d}", OK);
        }
        void DoClear() { qArr.Clear(); qSvc.Clear(); qGamma.Checked = true; ClearRes(); QStat("Cleared.", DIM); }
        void ClearRes() { foreach (var l in new[] { qRho, qL, qLq, qW, qWq, qP0 }) { l.Text = "—"; l.ForeColor = DIM; } }
        void QStat(string m, Color c) { qStat.Text = "  " + m; qStat.ForeColor = c; }
        void Set(Label l, string v) { l.Text = v; l.ForeColor = ACCENT2; }

        // ═══════════════════════════════════════
        //  SIMULATOR TAB
        // ═══════════════════════════════════════
        void BuildSTab()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = BG, Padding = new Padding(18, 14, 18, 14) };

            Lbl(p, "M/M/1  ·  Arrival Simulator", new Font("Segoe UI", 16f, FontStyle.Bold), ACCENT, 0, 0);
            Lbl(p, "Watch customers arrive, wait, get served and leave in real time", new Font("Segoe UI", 9f), DIM, 2, 30);

            // Input card
            var ic = Card(p, 0, 56, 490, 140, "  SIMULATION PARAMETERS");
            ic.Controls.Add(new Label { Text = "Mean Inter-Arrival Time:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(12, 40) });
            sArr = new TextBox { Location = new Point(12, 58), Size = new Size(130, 24), BackColor = INPUTBG, ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f), Text = "4" };
            ic.Controls.Add(sArr);

            ic.Controls.Add(new Label { Text = "Mean Service Time:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(160, 40) });
            sSvc = new TextBox { Location = new Point(160, 58), Size = new Size(130, 24), BackColor = INPUTBG, ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f), Text = "3" };
            ic.Controls.Add(sSvc);

            ic.Controls.Add(new Label { Text = "Customers:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(310, 40) });
            sCnt = new TextBox { Location = new Point(310, 58), Size = new Size(60, 24), BackColor = INPUTBG, ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f), Text = "15" };
            ic.Controls.Add(sCnt);

            ic.Controls.Add(new Label { Text = "Distribution:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(12, 96) });
            sExp = Radio(ic, "Exponential", 100, 96, true);
            sNorm = Radio(ic, "Normal", 205, 96);
            sUnif = Radio(ic, "Uniform", 278, 96);

            // Speed
            ic.Controls.Add(new Label { Text = "Speed:", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(375, 40) });
            sSpd = new TrackBar { Minimum = 1, Maximum = 12, Value = 4, TickFrequency = 1, Location = new Point(375, 56), Size = new Size(100, 30), BackColor = CARD };
            sSpdLbl = new Label { Text = "4x", ForeColor = ACCENT, Font = new Font("Segoe UI Semibold", 9f), AutoSize = true, Location = new Point(375, 90) };
            sSpd.ValueChanged += (s, e) => sSpdLbl.Text = sSpd.Value + "x";
            ic.Controls.Add(sSpd); ic.Controls.Add(sSpdLbl);

            // Buttons
            sBtnRun = Btn(p, "▶   Run Simulation", 504, 56, 220, 42, ACCENT);
            sBtnRun.Click += (s, e) => SimRun();
            var rst = Btn(p, "⟳  Reset", 504, 106, 220, 42, CARD); rst.ForeColor = DIM;
            rst.Click += (s, e) => SimReset();

            // Log box
            sLog = new ListBox { Location = new Point(504, 156), Size = new Size(220, 100), BackColor = INPUTBG, ForeColor = DIM, BorderStyle = BorderStyle.None, Font = new Font("Consolas", 7.5f) };
            p.Controls.Add(sLog);

            // Animation canvas
            sCanvas = new Panel { Location = new Point(0, 205), Size = new Size(724, 190), BackColor = CARD };
            sCanvas.Paint += DrawCanvas;
            p.Controls.Add(sCanvas);

            // Stats card
            var sc = Card(p, 0, 404, 724, 68, "  SIMULATION RESULTS");
            sWq = StatLbl(sc, "Avg Wait Wq:", 10);
            sW = StatLbl(sc, "Avg Time W:", 190);
            sRho = StatLbl(sc, "Utilization ρ:", 370);
            sSvd = StatLbl(sc, "Served:", 550);

            // Status
            sStat = new Label { Text = "Press Run to start.", ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = false, Size = new Size(724, 22), Location = new Point(0, 478) };
            p.Controls.Add(sStat);

            tabS.Controls.Add(p);
        }

        // ─── Simulation ────────────────────────
        double Rand(double mean, string d)
        {
            double v;
            if (d == "exp") v = -mean * Math.Log(rng.NextDouble() + 1e-9);
            else if (d == "normal") { double u = rng.NextDouble() + 1e-9, v2 = rng.NextDouble(); v = mean + mean * 0.3 * Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v2); }
            else v = mean * 0.5 + rng.NextDouble() * mean;
            return Math.Max(0.05, v);
        }

        void SimRun()
        {
            if (!double.TryParse(sArr.Text.Trim(), out double mA) || mA <= 0) { SStat("❌  Invalid arrival time.", ERR); return; }
            if (!double.TryParse(sSvc.Text.Trim(), out double mS) || mS <= 0) { SStat("❌  Invalid service time.", ERR); return; }
            if (!int.TryParse(sCnt.Text.Trim(), out int n) || n < 2 || n > 50) { SStat("❌  Customers: 2–50.", ERR); return; }

            string d = sNorm.Checked ? "normal" : sUnif.Checked ? "uniform" : "exp";
            simData.Clear();
            double clk = 0, free = 0;
            for (int i = 0; i < n; i++)
            {
                clk += Rand(mA, d);
                double a = clk, ss = Math.Max(a, free), st = Rand(mS, d), dp = ss + st;
                free = dp;
                simData.Add(new Customer { Id = i + 1, Arrival = a, SvcStart = ss, SvcTime = st, Depart = dp });
            }
            simEnd = simData[simData.Count - 1].Depart + 1.0;

            sLog.Items.Clear();
            foreach (var c in simData)
            {
                sLog.Items.Add($"t={c.Arrival:F2} C{c.Id} arrives" + (c.WaitTime > 0.01 ? $" waits {c.WaitTime:F2}" : " → immediate"));
                sLog.Items.Add($"t={c.SvcStart:F2} C{c.Id} starts svc={c.SvcTime:F2}");
                sLog.Items.Add($"t={c.Depart:F2} C{c.Id} departs");
            }

            simT = 0; visuals.Clear(); spawnIdx = 0; running = true;
            foreach (var l in new[] { sWq, sW, sRho, sSvd }) { l.Text = "—"; l.ForeColor = DIM; }
            SStat("Simulation running...", ACCENT);
            sBtnRun.Enabled = false;
            timer.Start();
        }

        void SimReset()
        {
            timer.Stop(); running = false;
            simData.Clear(); visuals.Clear(); sLog.Items.Clear();
            foreach (var l in new[] { sWq, sW, sRho, sSvd }) { l.Text = "—"; l.ForeColor = DIM; }
            SStat("Press Run to start.", DIM); sBtnRun.Enabled = true;
            sCanvas.Invalidate();
        }

        void Tick(object sender, EventArgs e)
        {
            if (!running) return;
            simT += (timer.Interval / 1000.0) * sSpd.Value;

            // Spawn arrivals
            while (spawnIdx < simData.Count && simData[spawnIdx].Arrival <= simT)
            {
                var c = simData[spawnIdx++];
                visuals.Add(new VisCust { Id = c.Id, X = -BR, Y = QY + QH / 2, Col = ORANGE, State = "moving" });
            }

            // Update positions
            foreach (var v in visuals)
            {
                var c = simData[v.Id - 1];
                if (simT >= c.Depart + 0.4) { v.State = "done"; v.Alpha = Math.Max(0f, v.Alpha - 0.07f); v.X += 2.5f; }
                else if (simT >= c.SvcStart) { v.State = "serving"; v.Col = OK; v.X = Ease(v.X, SX, 7); v.Y = Ease(v.Y, SY - SR - BR - 2, 7); v.SvcProg = Math.Min(1f, (float)((simT - c.SvcStart) / c.SvcTime)); }
                else if (simT >= c.Arrival)
                {
                    v.State = "queuing"; v.Col = ORANGE;
                    int slot = 0;
                    foreach (var o in visuals) { if (o == v) break; if (o.State == "queuing") slot++; }
                    float tx = QX + QW - BR * 2 - 8 - slot * (BR * 2 + 4); float ty = QY + QH / 2;
                    v.X = Ease(v.X, tx, 5); v.Y = Ease(v.Y, ty, 5);
                }
            }
            visuals.RemoveAll(v => v.State == "done" && v.Alpha <= 0);

            if (simT >= simEnd && spawnIdx >= simData.Count && visuals.Count == 0)
            {
                timer.Stop(); running = false;
                ShowStats(); sBtnRun.Enabled = true; SStat("✔  Simulation complete.", OK);
            }
            sCanvas.Invalidate();
        }

        float Ease(float cur, float tgt, float spd) => Math.Abs(cur - tgt) < spd ? tgt : cur + Math.Sign(tgt - cur) * spd;

        // ─── Canvas draw ───────────────────────
        void DrawCanvas(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int cw = sCanvas.Width, ch = sCanvas.Height;
            g.Clear(CARD);

            // Queue box
            FillRR(g, QX, QY, QW, QH, Color.FromArgb(42, 55, 88), 8);
            OutRR(g, QX, QY, QW, QH, Color.FromArgb(70, 95, 150), 0.5f, 8);
            DText(g, "QUEUE", QX + 10, QY + 14, DIM, 8.5f);

            // Arrow q→s
            DArrow(g, QX + QW + 2, QY + QH / 2, SX - SR - 4, SY, DIM);

            // Server circle
            g.FillEllipse(new SolidBrush(Color.FromArgb(42, 55, 88)), SX - SR, SY - SR, SR * 2, SR * 2);
            g.DrawEllipse(new Pen(Color.FromArgb(70, 95, 150), 0.5f), SX - SR, SY - SR, SR * 2, SR * 2);
            DTextC(g, "SERVER", SX, SY + 5, DIM, 8.5f);

            // Service progress ring
            var sv = visuals.Find(v => v.State == "serving");
            if (sv != null)
            {
                using var rp = new Pen(ACCENT2, 3f);
                g.DrawArc(rp, SX - SR - 6, SY - SR - 6, (SR + 6) * 2, (SR + 6) * 2, -90f, 360f * sv.SvcProg);
            }

            // Arrow s→exit
            DArrow(g, SX + SR + 4, SY, EX - 8, EY, DIM);
            DText(g, "EXIT", EX, EY + 4, DIM, 8.5f);

            // Time bar
            if (simData.Count > 0)
            {
                float prog = (float)Math.Min(1.0, simT / simEnd);
                int bx = QX, by = ch - 20, bw = EX + 40, bh = 5;
                FillRR(g, bx, by, bw, bh, Color.FromArgb(50, 70, 110), 3);
                if (prog > 0) FillRR(g, bx, by, (int)(bw * prog), bh, ACCENT, 3);
                DText(g, $"t={simT:F1}", bx + bw + 8, by + 9, DIM, 8f);
            }



            // Balls
            foreach (var v in visuals)
            {
                if (v.Alpha <= 0) continue;
                int a = (int)(255 * v.Alpha);
                DBall(g, (int)v.X, (int)v.Y, BR, Color.FromArgb(a, v.Col), v.Id.ToString());
            }

            // Queue count
            int qc = visuals.FindAll(v => v.State == "queuing").Count;
            DText(g, $"Waiting: {qc}", QX + 6, QY + QH + 16, DIM, 8.5f);
        }

        void DBall(Graphics g, int cx, int cy, int r, Color c, string id)
        {
            g.FillEllipse(new SolidBrush(c), cx - r, cy - r, r * 2, r * 2);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(id, new Font("Segoe UI Semibold", 7.5f), Brushes.White, new RectangleF(cx - r, cy - r, r * 2, r * 2), sf);
        }

        void FillRR(Graphics g, int x, int y, int w, int h, Color c, int r)
        {
            if (w <= 0 || h <= 0) return;

            r = Math.Min(r, Math.Min(w / 2, h / 2));

            // 🔴 CRITICAL FIX
            if (r <= 0)
            {
                using var brush = new SolidBrush(c);
                g.FillRectangle(brush, x, y, w, h);
                return;
            }

            using var path = new GraphicsPath();
            int d = r * 2;

            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);

            path.CloseFigure();

            using var brush2 = new SolidBrush(c);
            g.FillPath(brush2, path);
        }

        void OutRR(Graphics g, int x, int y, int w, int h, Color c, float pw, int r)
        {
            r = Math.Min(r, Math.Min(w / 2, h / 2));
            using var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90); path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90); path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure(); g.DrawPath(new Pen(c, pw), path);
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

        void ShowStats()
        {
            if (simData.Count == 0) return;
            int n = simData.Count; double wq = 0, w = 0, busy = 0;
            foreach (var c in simData) { wq += c.WaitTime; w += c.SystemTime; busy += c.SvcTime; }
            double rho = busy / simEnd;
            void S(Label l, string v) { l.Text = v; l.ForeColor = ACCENT2; }
            S(sWq, $"{wq / n:F3}"); S(sW, $"{w / n:F3}"); S(sRho, $"{rho:F3}"); S(sSvd, $"{n}");
        }

        void SStat(string m, Color c) { sStat.Text = "  " + m; sStat.ForeColor = c; }

        // ═══════════════════════════════════════
        //  UI helpers
        // ═══════════════════════════════════════
        Panel Card(Panel parent, int x, int y, int w, int h, string title)
        {
            var c = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = CARD };
            var hdr = new Label { Text = title, ForeColor = ACCENT, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), AutoSize = false, Size = new Size(w, 26), Location = new Point(0, 0), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(20, 28, 50), Padding = new Padding(10, 0, 0, 0) };
            c.Controls.Add(hdr); parent.Controls.Add(c); return c;
        }

        TextBox Inp(Panel parent, string lbl, ref int y)
        {
            parent.Controls.Add(new Label { Text = lbl, ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(14, y) }); y += 20;
            var tb = new TextBox { Location = new Point(14, y), Size = new Size(316, 26), BackColor = INPUTBG, ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f) };
            parent.Controls.Add(tb); y += 36; return tb;
        }

        RadioButton Radio(Panel p, string t, int x, int y, bool chk = false)
        {
            var rb = new RadioButton { Text = t, ForeColor = TEXT, BackColor = Color.Transparent, Location = new Point(x, y), AutoSize = true, Checked = chk };
            p.Controls.Add(rb); return rb;
        }

        Label Res(Panel parent, string name, ref int y)
        {
            parent.Controls.Add(new Label { Text = name, ForeColor = DIM, Font = new Font("Segoe UI", 8.5f), AutoSize = true, Location = new Point(14, y + 2) });
            var v = new Label { Text = "—", ForeColor = DIM, Font = new Font("Courier New", 10f, FontStyle.Bold), AutoSize = true, Location = new Point(260, y + 2) };
            parent.Controls.Add(v); y += 34; return v;
        }

        Label StatLbl(Panel parent, string name, int x)
        {
            parent.Controls.Add(new Label { Text = name, ForeColor = DIM, Font = new Font("Segoe UI", 8f), AutoSize = true, Location = new Point(x, 34) });
            var v = new Label { Text = "—", ForeColor = DIM, Font = new Font("Courier New", 10f, FontStyle.Bold), AutoSize = true, Location = new Point(x, 50) };
            parent.Controls.Add(v); return v;
        }

        Button Btn(Panel parent, string text, int x, int y, int w, int h, Color bg)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(w, h), FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9.5f), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; parent.Controls.Add(b); return b;
        }

        void Lbl(Panel parent, string text, Font font, Color col, int x, int y)
        {
            var l = new Label { Text = text, Font = font, ForeColor = col, Location = new Point(x, y), AutoSize = false, Size = new Size(700, 28) };
            parent.Controls.Add(l);
        }

    }
}