using System.Globalization;
using QueueingModels;

namespace Queuing_System
{
    public partial class MainForm : Form
    {
        private enum ModelKind { MM1, MG1, GG1, MG2, MMS }

        private static readonly (ModelKind Kind, string Label, string Description)[] Models =
        {
            (ModelKind.MM1, "M/M/1", "Markovian · Single server"),
            (ModelKind.MG1, "M/G/1", "General service · 1 server"),
            (ModelKind.GG1, "G/G/1", "General both · 1 server"),
            (ModelKind.MG2, "M/G/2", "General service · 2 servers"),
            (ModelKind.MMS, "M/M/S", "Markovian · S servers"),
        };

        public MainForm()
        {
            InitializeComponent();
            WireEvents();
            LoadModels();
            LayoutInputGroups();
        }

        private void WireEvents()
        {
            lstModels.SelectedIndexChanged += (_, _) => OnModelChanged();
            btnCalculate.Click += (_, _) => Calculate();
            btnCalculate.MouseEnter += (_, _) => btnCalculate.BackColor = Color.FromArgb(29, 78, 216);
            btnCalculate.MouseLeave += (_, _) => btnCalculate.BackColor = Color.FromArgb(37, 99, 235);
            lstModels.DrawItem += LstModels_DrawItem;
            cboSvcVarMethod.SelectedIndexChanged += (_, _) => UpdateVarianceFields();
            cboArrVarMethod.SelectedIndexChanged += (_, _) => UpdateVarianceFields();
            cboArrivalType.SelectedIndexChanged += (_, _) => UpdateValueLabels();
            cboServiceType.SelectedIndexChanged += (_, _) => UpdateValueLabels();
            cboArrivalUnit.SelectedIndexChanged += (_, _) => UpdateValueLabels();
            cboServiceUnit.SelectedIndexChanged += (_, _) => UpdateValueLabels();
            Resize += (_, _) => LayoutResultsPanel();
        }

        private void LoadModels()
        {
            lstModels.Items.Clear();
            foreach (var m in Models)
                lstModels.Items.Add(m.Label);
            lstModels.SelectedIndex = 0;
        }

        private ModelKind SelectedModel => Models[lstModels.SelectedIndex].Kind;

        private void OnModelChanged()
        {
            lblStatus.Text = "";
            var model = SelectedModel;

            grpServiceVariance.Visible = model is ModelKind.MG1 or ModelKind.GG1 or ModelKind.MG2;
            grpArrivalVariance.Visible = model == ModelKind.GG1;
            grpServers.Visible = model == ModelKind.MMS;

            LayoutInputGroups();

            UpdateVarianceFields();
            UpdateValueLabels();
            txtResults.Text = $"Model: {Models[lstModels.SelectedIndex].Label}\r\n{Models[lstModels.SelectedIndex].Description}\r\n\r\nEnter parameters and click Calculate.";
        }

        private void LayoutInputGroups()
        {
            const int gap = 10;
            int width = Math.Max(400, scrollInputs.ClientSize.Width - 4);
            int y = 0;

            foreach (var grp in new[] { grpArrival, grpService, grpServiceVariance, grpArrivalVariance, grpServers })
            {
                grp.Width = width;
                grp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }

            grpArrival.Location = new Point(0, y);
            y += grpArrival.Height + gap;

            grpService.Location = new Point(0, y);
            y += grpService.Height + gap;

            if (grpServiceVariance.Visible)
            {
                grpServiceVariance.Location = new Point(0, y);
                y += grpServiceVariance.Height + gap;
            }

            if (grpArrivalVariance.Visible)
            {
                grpArrivalVariance.Location = new Point(0, y);
                y += grpArrivalVariance.Height + gap;
            }

            if (grpServers.Visible)
            {
                grpServers.Location = new Point(0, y);
                y += grpServers.Height + gap;
            }

            scrollInputs.Height = y + 8;
            btnCalculate.Top = scrollInputs.Bottom + 12;
            lblStatus.Top = btnCalculate.Top + 14;
            LayoutResultsPanel();
        }

        private void LstModels_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var back = selected ? Color.FromArgb(37, 99, 235) : Color.FromArgb(241, 245, 249);
            var fore = selected ? Color.White : Color.FromArgb(51, 65, 85);

            using var backBrush = new SolidBrush(back);
            e.Graphics.FillRectangle(backBrush, e.Bounds);

            string label = Models[e.Index].Label;
            string desc = Models[e.Index].Description;

            using var labelFont = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            using var descFont = new Font("Segoe UI", 8.5F);
            using var foreBrush = new SolidBrush(fore);
            using var descBrush = new SolidBrush(selected ? Color.FromArgb(219, 234, 254) : Color.FromArgb(100, 116, 139));

            e.Graphics.DrawString(label, labelFont, foreBrush, e.Bounds.Left + 12, e.Bounds.Top + 6);
            e.Graphics.DrawString(desc, descFont, descBrush, e.Bounds.Left + 12, e.Bounds.Top + 24);
        }

        private void UpdateValueLabels()
        {
            string arrUnit = UnitLabel(cboArrivalUnit);
            string svcUnit = UnitLabel(cboServiceUnit);

            lblArrivalValue.Text = cboArrivalType.SelectedIndex == 1
                ? $"Mean inter-arrival time ({arrUnit}):"
                : $"Arrival rate λ (per {arrUnit}):";

            lblServiceValue.Text = cboServiceType.SelectedIndex == 1
                ? $"Mean service time ({svcUnit}):"
                : $"Service rate μ (per {svcUnit}):";
        }

        private void UpdateVarianceFields()
        {
            bool svcUniform = cboSvcVarMethod.SelectedIndex == 2;
            lblSvcVarMax.Visible = txtSvcVarMax.Visible = svcUniform;
            lblSvcVarValue.Text = cboSvcVarMethod.SelectedIndex switch
            {
                1 => "Std deviation (σ):",
                2 => "Min:",
                _ => $"Variance ({UnitLabelSq(cboSvcVarUnit)}):"
            };

            bool arrUniform = cboArrVarMethod.SelectedIndex == 1;
            lblArrVarMax.Visible = txtArrVarMax.Visible = arrUniform;
            lblArrVarValue.Text = arrUniform
                ? "Min:"
                : $"Variance ({UnitLabelSq(cboArrVarUnit)}):";
        }

        private void LayoutResultsPanel()
        {
            int top = btnCalculate.Bottom + 16;
            grpResults.Location = new Point(20, top);
            grpResults.Size = new Size(rightPanel.ClientSize.Width - 40, rightPanel.ClientSize.Height - top - 20);
        }

        private void Calculate()
        {
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Text = "";

            if (!TryBuildInputs(out var inp, out string? error))
            {
                lblStatus.Text = error;
                return;
            }

            QueueResults? results = null;
            bool stable = false;
            string unstableMsg = "";

            switch (SelectedModel)
            {
                case ModelKind.MM1:
                    var mm1 = new MM1(inp.Lambda, inp.Mu, inp.BaseUnit);
                    stable = mm1.IsStable();
                    unstableMsg = "M/M/1 is unstable: λ ≥ μ. The queue grows infinitely.";
                    if (stable) results = mm1.BuildResults();
                    break;

                case ModelKind.MG1:
                    var mg1 = new MG1(inp.Lambda, inp.Mu, inp.VarService, inp.BaseUnit);
                    stable = mg1.IsStable();
                    unstableMsg = "M/G/1 is unstable: λ ≥ μ. The queue grows infinitely.";
                    if (stable) results = mg1.BuildResults();
                    break;

                case ModelKind.GG1:
                    var gg1 = new GG1(inp.Lambda, inp.Mu, inp.VarArrival, inp.VarService, inp.BaseUnit);
                    stable = gg1.IsStable();
                    unstableMsg = "G/G/1 is unstable: λ ≥ μ. The queue grows infinitely.";
                    if (stable) results = gg1.BuildResults();
                    break;

                case ModelKind.MG2:
                    var mg2 = new MG2(inp.Lambda, inp.Mu, inp.VarService, inp.BaseUnit);
                    stable = mg2.IsStable();
                    unstableMsg = "M/G/2 is unstable: λ ≥ 2μ. The queue grows infinitely.";
                    if (stable) results = mg2.BuildResults();
                    break;

                case ModelKind.MMS:
                    var mms = new MMS(inp.Lambda, inp.Mu, inp.Servers, inp.BaseUnit);
                    stable = mms.IsStable();
                    unstableMsg = $"M/M/{inp.Servers} is unstable: λ ≥ S·μ. The queue grows infinitely.";
                    if (stable) results = mms.BuildResults();
                    break;
            }

            if (!stable)
            {
                lblStatus.Text = unstableMsg;
                txtResults.Text = $"⚠ SYSTEM UNSTABLE\r\n\r\n{unstableMsg}\r\n\r\nPlease adjust your input values so that traffic intensity ρ < 1.";
                return;
            }

            lblStatus.ForeColor = Color.FromArgb(22, 163, 74);
            lblStatus.Text = "Calculation complete.";
            txtResults.Text = results!.FormatForDisplay();
        }

        private bool TryBuildInputs(out InputData inp, out string? error)
        {
            inp = new InputData();
            error = null;

            var baseUnit = ParseUnit(cboArrivalUnit);
            if (baseUnit == null) { error = "Select a valid arrival unit."; return false; }

            if (!TryParsePositive(txtArrivalValue.Text, out double arrivalRaw))
            {
                error = "Enter a valid positive arrival value.";
                return false;
            }

            inp.Lambda = cboArrivalType.SelectedIndex == 1 ? 1.0 / arrivalRaw : arrivalRaw;
            inp.BaseUnit = baseUnit.Value;

            var svcUnit = ParseUnit(cboServiceUnit);
            if (svcUnit == null) { error = "Select a valid service unit."; return false; }

            if (!TryParsePositive(txtServiceValue.Text, out double serviceRaw))
            {
                error = "Enter a valid positive service value.";
                return false;
            }

            double muInOwnUnit = cboServiceType.SelectedIndex == 1 ? 1.0 / serviceRaw : serviceRaw;
            inp.Mu = UnitConverter.ConvertRate(muInOwnUnit, svcUnit.Value, inp.BaseUnit);

            if (grpServiceVariance.Visible)
            {
                if (!TryParseServiceVariance(inp.BaseUnit, out double varSvc, out error))
                    return false;
                inp.VarService = varSvc;
            }

            if (grpArrivalVariance.Visible)
            {
                if (!TryParseArrivalVariance(inp.BaseUnit, out double varArr, out error))
                    return false;
                inp.VarArrival = varArr;
            }

            if (grpServers.Visible)
                inp.Servers = (int)numServers.Value;
            else
                inp.Servers = 1;

            return true;
        }

        private bool TryParseServiceVariance(TimeUnit baseUnit, out double variance, out string? error)
        {
            variance = 0;
            error = null;
            var varUnit = ParseUnit(cboSvcVarUnit);
            if (varUnit == null) { error = "Select a valid service variance unit."; return false; }

            double rawVar;
            switch (cboSvcVarMethod.SelectedIndex)
            {
                case 1:
                    if (!TryParsePositive(txtSvcVarValue.Text, out double sigma))
                    { error = "Enter a valid positive std deviation."; return false; }
                    rawVar = sigma * sigma;
                    break;
                case 2:
                    if (!TryParsePositive(txtSvcVarValue.Text, out double minT) ||
                        !TryParsePositive(txtSvcVarMax.Text, out double maxT))
                    { error = "Enter valid positive min and max service times."; return false; }
                    if (maxT <= minT) { error = "Max service time must be greater than min."; return false; }
                    rawVar = Math.Pow(maxT - minT, 2) / 12.0;
                    break;
                default:
                    if (!TryParsePositive(txtSvcVarValue.Text, out rawVar))
                    { error = "Enter a valid positive service variance."; return false; }
                    break;
            }

            variance = UnitConverter.ConvertVariance(rawVar, varUnit.Value, baseUnit);
            return true;
        }

        private bool TryParseArrivalVariance(TimeUnit baseUnit, out double variance, out string? error)
        {
            variance = 0;
            error = null;
            var varUnit = ParseUnit(cboArrVarUnit);
            if (varUnit == null) { error = "Select a valid arrival variance unit."; return false; }

            double rawVar;
            if (cboArrVarMethod.SelectedIndex == 1)
            {
                if (!TryParsePositive(txtArrVarValue.Text, out double minA) ||
                    !TryParsePositive(txtArrVarMax.Text, out double maxA))
                { error = "Enter valid positive min and max inter-arrival times."; return false; }
                if (maxA <= minA) { error = "Max inter-arrival time must be greater than min."; return false; }
                rawVar = Math.Pow(maxA - minA, 2) / 12.0;
            }
            else
            {
                if (!TryParsePositive(txtArrVarValue.Text, out rawVar))
                { error = "Enter a valid positive arrival variance."; return false; }
            }

            variance = UnitConverter.ConvertVariance(rawVar, varUnit.Value, baseUnit);
            return true;
        }

        private static bool TryParsePositive(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();
            return (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                    double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                   && value > 0;
        }

        private static TimeUnit? ParseUnit(ComboBox cbo) =>
            cbo.SelectedIndex switch
            {
                0 => TimeUnit.Minutes,
                1 => TimeUnit.Hours,
                _ => null
            };

        private static string UnitLabel(ComboBox cbo) =>
            cbo.SelectedIndex == 1 ? "hr" : "min";

        private static string UnitLabelSq(ComboBox cbo) =>
            cbo.SelectedIndex == 1 ? "hr²" : "min²";

        private struct InputData
        {
            public double Lambda;
            public double Mu;
            public double VarService;
            public double VarArrival;
            public int Servers;
            public TimeUnit BaseUnit;
        }
    }
}
