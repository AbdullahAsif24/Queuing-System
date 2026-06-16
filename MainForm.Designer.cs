namespace Queuing_System
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            headerPanel = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            mainSplit = new SplitContainer();
            modelPanel = new Panel();
            lblModels = new Label();
            lstModels = new ListBox();
            rightPanel = new Panel();
            scrollInputs = new Panel();
            grpArrival = new GroupBox();
            lblArrivalUnit = new Label();
            cboArrivalUnit = new ComboBox();
            lblArrivalType = new Label();
            cboArrivalType = new ComboBox();
            lblArrivalValue = new Label();
            txtArrivalValue = new TextBox();
            grpService = new GroupBox();
            lblServiceUnit = new Label();
            cboServiceUnit = new ComboBox();
            lblServiceType = new Label();
            cboServiceType = new ComboBox();
            lblServiceValue = new Label();
            txtServiceValue = new TextBox();
            grpServiceVariance = new GroupBox();
            lblSvcVarMethod = new Label();
            cboSvcVarMethod = new ComboBox();
            lblSvcVarUnit = new Label();
            cboSvcVarUnit = new ComboBox();
            lblSvcVarValue = new Label();
            txtSvcVarValue = new TextBox();
            lblSvcVarMax = new Label();
            txtSvcVarMax = new TextBox();
            grpArrivalVariance = new GroupBox();
            lblArrVarMethod = new Label();
            cboArrVarMethod = new ComboBox();
            lblArrVarUnit = new Label();
            cboArrVarUnit = new ComboBox();
            lblArrVarValue = new Label();
            txtArrVarValue = new TextBox();
            lblArrVarMax = new Label();
            txtArrVarMax = new TextBox();
            grpServers = new GroupBox();
            lblServers = new Label();
            numServers = new NumericUpDown();
            btnCalculate = new Button();
            lblStatus = new Label();
            grpResults = new GroupBox();
            txtResults = new TextBox();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
            mainSplit.Panel1.SuspendLayout();
            mainSplit.Panel2.SuspendLayout();
            mainSplit.SuspendLayout();
            modelPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            scrollInputs.SuspendLayout();
            grpArrival.SuspendLayout();
            grpService.SuspendLayout();
            grpServiceVariance.SuspendLayout();
            grpArrivalVariance.SuspendLayout();
            grpServers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numServers).BeginInit();
            grpResults.SuspendLayout();
            SuspendLayout();

            // headerPanel
            headerPanel.BackColor = Color.FromArgb(30, 58, 95);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 72;
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblTitle);

            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(24, 12);
            lblTitle.Text = "Queueing Models Calculator";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9.5F);
            lblSubtitle.ForeColor = Color.FromArgb(180, 200, 230);
            lblSubtitle.Location = new Point(26, 44);
            lblSubtitle.Text = "DCS-UOK  ·  Dr. Shaista Rais  ·  Software Engineering Fundamentals";

            // mainSplit
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.FixedPanel = FixedPanel.Panel1;
            mainSplit.IsSplitterFixed = true;
            mainSplit.SplitterDistance = 220;
            mainSplit.Panel1.Controls.Add(modelPanel);
            mainSplit.Panel2.Controls.Add(rightPanel);

            // modelPanel
            modelPanel.BackColor = Color.FromArgb(241, 245, 249);
            modelPanel.Dock = DockStyle.Fill;
            modelPanel.Padding = new Padding(16);
            modelPanel.Controls.Add(lstModels);
            modelPanel.Controls.Add(lblModels);

            lblModels.AutoSize = true;
            lblModels.Dock = DockStyle.Top;
            lblModels.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblModels.ForeColor = Color.FromArgb(51, 65, 85);
            lblModels.Padding = new Padding(0, 0, 0, 8);
            lblModels.Text = "Select Model";

            lstModels.BorderStyle = BorderStyle.None;
            lstModels.Dock = DockStyle.Fill;
            lstModels.DrawMode = DrawMode.OwnerDrawFixed;
            lstModels.Font = new Font("Segoe UI", 11F);
            lstModels.ItemHeight = 40;

            // rightPanel
            rightPanel.BackColor = Color.FromArgb(248, 250, 252);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Padding = new Padding(20);
            rightPanel.Controls.Add(grpResults);
            rightPanel.Controls.Add(lblStatus);
            rightPanel.Controls.Add(btnCalculate);
            rightPanel.Controls.Add(scrollInputs);

            scrollInputs.AutoScroll = true;
            scrollInputs.Dock = DockStyle.Top;
            scrollInputs.Height = 420;
            scrollInputs.Controls.Add(grpServers);
            scrollInputs.Controls.Add(grpArrivalVariance);
            scrollInputs.Controls.Add(grpServiceVariance);
            scrollInputs.Controls.Add(grpService);
            scrollInputs.Controls.Add(grpArrival);

            ConfigureInputGroup(grpArrival, "Arrival (λ)", 0,
                lblArrivalUnit, cboArrivalUnit, "Unit:", new[] { "Minutes", "Hours" },
                lblArrivalType, cboArrivalType, "Input type:", new[] { "Arrival rate", "Mean inter-arrival time" },
                lblArrivalValue, txtArrivalValue, "Value:");

            ConfigureInputGroup(grpService, "Service (μ)", 110,
                lblServiceUnit, cboServiceUnit, "Unit:", new[] { "Minutes", "Hours" },
                lblServiceType, cboServiceType, "Input type:", new[] { "Service rate", "Mean service time" },
                lblServiceValue, txtServiceValue, "Value:");

            ConfigureVarianceGroup(grpServiceVariance, "Service Variance (σ²s)", 220,
                lblSvcVarMethod, cboSvcVarMethod, new[] { "Variance directly", "Std deviation (σ)", "Min & Max (uniform)" },
                lblSvcVarUnit, cboSvcVarUnit,
                lblSvcVarValue, txtSvcVarValue, lblSvcVarMax, txtSvcVarMax);

            ConfigureVarianceGroup(grpArrivalVariance, "Arrival Variance (σ²a)", 330,
                lblArrVarMethod, cboArrVarMethod, new[] { "Variance directly", "Min & Max (uniform)" },
                lblArrVarUnit, cboArrVarUnit,
                lblArrVarValue, txtArrVarValue, lblArrVarMax, txtArrVarMax);

            // grpServers
            grpServers.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            grpServers.ForeColor = Color.FromArgb(30, 58, 95);
            grpServers.Location = new Point(0, 440);
            grpServers.Size = new Size(820, 72);
            grpServers.Text = "Servers";
            grpServers.Controls.Add(numServers);
            grpServers.Controls.Add(lblServers);

            lblServers.AutoSize = true;
            lblServers.Font = new Font("Segoe UI", 9F);
            lblServers.ForeColor = Color.FromArgb(71, 85, 105);
            lblServers.Location = new Point(16, 32);
            lblServers.Text = "Number of servers (S):";

            numServers.Font = new Font("Segoe UI", 10F);
            numServers.Location = new Point(180, 28);
            numServers.Minimum = 1;
            numServers.Maximum = 100;
            numServers.Value = 2;
            numServers.Width = 80;

            // btnCalculate
            btnCalculate.BackColor = Color.FromArgb(37, 99, 235);
            btnCalculate.Cursor = Cursors.Hand;
            btnCalculate.FlatAppearance.BorderSize = 0;
            btnCalculate.FlatStyle = FlatStyle.Flat;
            btnCalculate.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            btnCalculate.ForeColor = Color.White;
            btnCalculate.Location = new Point(20, 448);
            btnCalculate.Size = new Size(200, 44);
            btnCalculate.Text = "Calculate";
            btnCalculate.UseVisualStyleBackColor = false;

            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Location = new Point(240, 462);
            lblStatus.MaximumSize = new Size(600, 0);

            // grpResults
            grpResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpResults.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            grpResults.ForeColor = Color.FromArgb(30, 58, 95);
            grpResults.Location = new Point(20, 504);
            grpResults.Size = new Size(820, 200);
            grpResults.Text = "Results";
            grpResults.Controls.Add(txtResults);

            txtResults.BackColor = Color.FromArgb(15, 23, 42);
            txtResults.BorderStyle = BorderStyle.None;
            txtResults.Dock = DockStyle.Fill;
            txtResults.Font = new Font("Consolas", 10F);
            txtResults.ForeColor = Color.FromArgb(226, 232, 240);
            txtResults.Multiline = true;
            txtResults.ReadOnly = true;
            txtResults.ScrollBars = ScrollBars.Vertical;
            txtResults.Text = "Select a model, enter parameters, and click Calculate.";

            // MainForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1080, 740);
            Controls.Add(mainSplit);
            Controls.Add(headerPanel);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(920, 640);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Queueing Models Calculator";

            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            mainSplit.Panel1.ResumeLayout(false);
            mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
            mainSplit.ResumeLayout(false);
            modelPanel.ResumeLayout(false);
            modelPanel.PerformLayout();
            rightPanel.ResumeLayout(false);
            rightPanel.PerformLayout();
            scrollInputs.ResumeLayout(false);
            grpArrival.ResumeLayout(false);
            grpArrival.PerformLayout();
            grpService.ResumeLayout(false);
            grpService.PerformLayout();
            grpServiceVariance.ResumeLayout(false);
            grpServiceVariance.PerformLayout();
            grpArrivalVariance.ResumeLayout(false);
            grpArrivalVariance.PerformLayout();
            grpServers.ResumeLayout(false);
            grpServers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numServers).EndInit();
            grpResults.ResumeLayout(false);
            grpResults.PerformLayout();
            ResumeLayout(false);
        }

        private void ConfigureInputGroup(
            GroupBox grp, string title, int top,
            Label lblUnit, ComboBox cboUnit, string unitCaption, string[] unitItems,
            Label lblType, ComboBox cboType, string typeCaption, string[] typeItems,
            Label lblValue, TextBox txtValue, string valueCaption)
        {
            grp.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            grp.ForeColor = Color.FromArgb(30, 58, 95);
            grp.Location = new Point(0, top);
            grp.Size = new Size(820, 100);
            grp.Text = title;

            lblUnit.AutoSize = true;
            lblUnit.Font = new Font("Segoe UI", 9F);
            lblUnit.ForeColor = Color.FromArgb(71, 85, 105);
            lblUnit.Location = new Point(16, 32);
            lblUnit.Text = unitCaption;

            cboUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnit.Font = new Font("Segoe UI", 9.5F);
            cboUnit.Items.AddRange(unitItems);
            cboUnit.Location = new Point(16, 52);
            cboUnit.Size = new Size(140, 25);
            cboUnit.SelectedIndex = 0;

            lblType.AutoSize = true;
            lblType.Font = new Font("Segoe UI", 9F);
            lblType.ForeColor = Color.FromArgb(71, 85, 105);
            lblType.Location = new Point(180, 32);
            lblType.Text = typeCaption;

            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboType.Font = new Font("Segoe UI", 9.5F);
            cboType.Items.AddRange(typeItems);
            cboType.Location = new Point(180, 52);
            cboType.Size = new Size(240, 25);
            cboType.SelectedIndex = 0;

            lblValue.AutoSize = true;
            lblValue.Font = new Font("Segoe UI", 9F);
            lblValue.ForeColor = Color.FromArgb(71, 85, 105);
            lblValue.Location = new Point(440, 32);
            lblValue.Text = valueCaption;

            txtValue.Font = new Font("Segoe UI", 10F);
            txtValue.Location = new Point(440, 52);
            txtValue.Size = new Size(160, 25);

            grp.Controls.AddRange(new Control[] { lblUnit, cboUnit, lblType, cboType, lblValue, txtValue });
        }

        private void ConfigureVarianceGroup(
            GroupBox grp, string title, int top,
            Label lblMethod, ComboBox cboMethod, string[] methodItems,
            Label lblUnit, ComboBox cboUnit,
            Label lblValue, TextBox txtValue,
            Label lblMax, TextBox txtMax)
        {
            grp.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            grp.ForeColor = Color.FromArgb(30, 58, 95);
            grp.Location = new Point(0, top);
            grp.Size = new Size(820, 100);
            grp.Text = title;

            lblMethod.AutoSize = true;
            lblMethod.Font = new Font("Segoe UI", 9F);
            lblMethod.ForeColor = Color.FromArgb(71, 85, 105);
            lblMethod.Location = new Point(16, 32);
            lblMethod.Text = "Method:";

            cboMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMethod.Font = new Font("Segoe UI", 9.5F);
            cboMethod.Items.AddRange(methodItems);
            cboMethod.Location = new Point(16, 52);
            cboMethod.Size = new Size(200, 25);
            cboMethod.SelectedIndex = 0;

            lblUnit.AutoSize = true;
            lblUnit.Font = new Font("Segoe UI", 9F);
            lblUnit.ForeColor = Color.FromArgb(71, 85, 105);
            lblUnit.Location = new Point(230, 32);
            lblUnit.Text = "Unit:";

            cboUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnit.Font = new Font("Segoe UI", 9.5F);
            cboUnit.Items.AddRange(new[] { "Minutes", "Hours" });
            cboUnit.Location = new Point(230, 52);
            cboUnit.Size = new Size(120, 25);
            cboUnit.SelectedIndex = 0;

            lblValue.AutoSize = true;
            lblValue.Font = new Font("Segoe UI", 9F);
            lblValue.ForeColor = Color.FromArgb(71, 85, 105);
            lblValue.Location = new Point(370, 32);
            lblValue.Text = "Value:";

            txtValue.Font = new Font("Segoe UI", 10F);
            txtValue.Location = new Point(370, 52);
            txtValue.Size = new Size(120, 25);

            lblMax.AutoSize = true;
            lblMax.Font = new Font("Segoe UI", 9F);
            lblMax.ForeColor = Color.FromArgb(71, 85, 105);
            lblMax.Location = new Point(510, 32);
            lblMax.Text = "Max:";

            txtMax.Font = new Font("Segoe UI", 10F);
            txtMax.Location = new Point(510, 52);
            txtMax.Size = new Size(120, 25);

            grp.Controls.AddRange(new Control[] { lblMethod, cboMethod, lblUnit, cboUnit, lblValue, txtValue, lblMax, txtMax });
        }

        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private SplitContainer mainSplit;
        private Panel modelPanel;
        private Label lblModels;
        private ListBox lstModels;
        private Panel rightPanel;
        private Panel scrollInputs;
        private GroupBox grpArrival;
        private Label lblArrivalUnit;
        private ComboBox cboArrivalUnit;
        private Label lblArrivalType;
        private ComboBox cboArrivalType;
        private Label lblArrivalValue;
        private TextBox txtArrivalValue;
        private GroupBox grpService;
        private Label lblServiceUnit;
        private ComboBox cboServiceUnit;
        private Label lblServiceType;
        private ComboBox cboServiceType;
        private Label lblServiceValue;
        private TextBox txtServiceValue;
        private GroupBox grpServiceVariance;
        private Label lblSvcVarMethod;
        private ComboBox cboSvcVarMethod;
        private Label lblSvcVarUnit;
        private ComboBox cboSvcVarUnit;
        private Label lblSvcVarValue;
        private TextBox txtSvcVarValue;
        private Label lblSvcVarMax;
        private TextBox txtSvcVarMax;
        private GroupBox grpArrivalVariance;
        private Label lblArrVarMethod;
        private ComboBox cboArrVarMethod;
        private Label lblArrVarUnit;
        private ComboBox cboArrVarUnit;
        private Label lblArrVarValue;
        private TextBox txtArrVarValue;
        private Label lblArrVarMax;
        private TextBox txtArrVarMax;
        private GroupBox grpServers;
        private Label lblServers;
        private NumericUpDown numServers;
        private Button btnCalculate;
        private Label lblStatus;
        private GroupBox grpResults;
        private TextBox txtResults;
    }
}
