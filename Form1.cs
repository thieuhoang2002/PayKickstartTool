using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using MiniExcelLibs; // Nhớ cài: dotnet add package MiniExcel
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace PayKickstartAuto
{
    // Cấu trúc dữ liệu khớp với file Data.csv
    public class AccountData
    {
        public string Email { get; set; }
        public string MailPass { get; set; } // Pass ứng dụng Gmail
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string GeneratedPassword { get; set; } // Pass do hệ thống cấp
        public string Status { get; set; }
    }

    public partial class Form1 : Form
    {
        private Button btnStart;
        private Button btnBrowse;
        // Chỉ giữ nút Chọn File
        private TextBox txtPath;
        private RichTextBox rtbLog;
        private TextBox txtConcurrency;
        private TextBox txtCatchAllEmail;
        private Button btnSaveEmail;
        private CheckBox chkUseProxy;
        private TextBox txtProxyHost;
        private TextBox txtProxyPort;
        private TextBox txtProxyUser;
        private TextBox txtProxyPass;
        private Button btnSaveProxy;
        private string catchAllEmail_Password; // App Password cho Catch-All Email (dùng khi email là custom domain)
        private DataGridView dgvPreview;
        private Label lblFileInfo;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnHelp;
        private TabControl mainTabs;
        private TabPage tabHome;
        private TabPage tabCreate;
        private TabPage tabCrawl;
        private TabPage tabCoupon;
        private TabPage tabSettings;
        // Crawl Data UI
        private TextBox txtCrawlPath;
        private Button btnCrawlBrowse;
        private Label lblCrawlFileInfo;
        private DataGridView dgvCrawl;
        private RichTextBox rtbCrawlDetails;
        // Crawl Data detail UI
        private Label lblCrawlAccountInfo;
        private Label lblCrawlDateRange;
        private FlowLayoutPanel crawlSummaryPanel;
        private Label lblSumGross;
        private Label lblSumNet;
        private Label lblSumRefunded;
        private Label lblSumPaid;
        private Label lblSumDenied;
        private DataGridView dgvGraph;
        private DataGridView dgvSalesSummary;
        private TextBox txtCrawlConcurrency;
        private Button btnCrawlAll;
        private Button btnCrawlSelected;
        private ProgressBar crawlProgressBar;
        private Label lblCrawlStatus;
        private readonly BindingSource gridSource = new BindingSource();
        private readonly BindingSource crawlGridSource = new BindingSource();
        private List<AccountData> cachedAccounts = new List<AccountData>();
        private List<AccountData> crawlAccounts = new List<AccountData>();
        private string lastLoadedPath = string.Empty;
        private string lastCrawlCsvPath = string.Empty;
        private readonly string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private readonly string resultsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results");
        private readonly string crawlDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results", "Crawl");
        private readonly string emailConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_config.txt");
        private readonly string proxyConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxy_config.txt");
        private ProxyConfig currentProxyConfig = new ProxyConfig();

        public Form1()
        {
            InitializeCustomComponent();
        }

        // Vẽ giao diện (Vì VS Code không kéo thả được)
        private void InitializeCustomComponent()
        {
            this.Text = "PayKickstart Automation Suite";
            this.Size = new Size(1400, 720);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.DoubleBuffered = true;

            // THEME COLORS
            var bg = Color.FromArgb(247, 249, 252);      // Light background
            var card = Color.White;                       // Card surface
            var nav = Color.FromArgb(242, 244, 247);      // Nav background
            var fg = Color.FromArgb(28, 34, 45);          // Primary text
            var muted = Color.FromArgb(108, 115, 125);    // Secondary text
            var accent = Color.FromArgb(37, 99, 235);     // Primary accent (blue)
            var accentHover = Color.FromArgb(30, 81, 191);
            var accentDeep = Color.FromArgb(14, 165, 233); // Secondary accent (cyan)

            this.BackColor = bg;

            // NAVIGATION PANEL
            var navPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 180,
                BackColor = nav,
                Padding = new Padding(16, 20, 16, 20)
            };

            var brand = new Label
            {
                Text = "PK Dashboard",
                ForeColor = fg,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                Dock = DockStyle.Top
            };
            navPanel.Controls.Add(brand);

            var navStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 24, 0, 0),
                AutoScroll = true
            };
            navPanel.Controls.Add(navStack);

            Button CreateNavButton(string text)
            {
                return new Button
                {
                    Text = text,
                    Width = 170,
                    Height = 44,
                    Margin = new Padding(0, 0, 0, 12),
                    TextAlign = ContentAlignment.MiddleLeft,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(231, 235, 243),
                    ForeColor = fg,
                    FlatAppearance = { BorderSize = 0 },
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
            }

            var navButtons = new List<Button>();
            var btnNavHome = CreateNavButton("Home");
            var btnNavCreate = CreateNavButton("Create Account");
            var btnNavCrawl = CreateNavButton("Crawl Data");
            var btnNavCoupon = CreateNavButton("Coupon");
            var btnNavSettings = CreateNavButton("Settings");

            navButtons.AddRange(new[] { btnNavHome, btnNavCreate, btnNavCrawl, btnNavCoupon, btnNavSettings });
            navButtons.ForEach(b => navStack.Controls.Add(b));

            // NÚT MỞ THƯ MỤC (đặt dưới nút Settings)
            var btnOpenData = CreateNavButton("Mở thư mục Data");
            var btnOpenResults = CreateNavButton("Mở thư mục Results");
            // Không đưa 2 nút này vào danh sách navButtons để tránh hiệu ứng chọn tab
            btnOpenData.Click += (s, e) => OpenFolder(dataDir);
            btnOpenResults.Click += (s, e) => OpenFolder(resultsDir);
            navStack.Controls.Add(btnOpenData);
            navStack.Controls.Add(btnOpenResults);

            // MAIN AREA
            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = bg, Padding = new Padding(0) };

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(24, 16, 24, 8), BackColor = Color.White };
            var headerTitle = new Label { Text = "PayKickstart Dashboard", ForeColor = fg, AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(0, 0) };
            var headerSubtitle = new Label { Text = "Automation • CSV • Proxy-ready", ForeColor = muted, AutoSize = true, Location = new Point(0, 38), Font = new Font("Segoe UI", 11, FontStyle.Regular) };
            headerPanel.Controls.Add(headerTitle);
            headerPanel.Controls.Add(headerSubtitle);

            mainTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.Normal,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Padding = new Point(12, 6),
                BackColor = card
            };

            tabHome = new TabPage("Home") { BackColor = card };
            tabCreate = new TabPage("Create Account") { BackColor = card };
            tabCrawl = new TabPage("Crawl Data") { BackColor = card };
            tabCoupon = new TabPage("Coupon") { BackColor = card };
            tabSettings = new TabPage("Settings") { BackColor = card };

            mainTabs.TabPages.AddRange(new[] { tabHome, tabCreate, tabCrawl, tabCoupon, tabSettings });

            // --- HOME TAB ---
            var homeLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                ColumnCount = 2,
                RowCount = 2,
                BackColor = bg
            };
            homeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            homeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            homeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            homeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            Panel BuildCard(string title, string body, Color highlight)
            {
                var cardPanel = new Panel { BackColor = card, Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(10), MinimumSize = new Size(0, 180) };
                var t = new Label { Text = title, ForeColor = fg, AutoSize = true, Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold) };
                var b = new Label { Text = body, ForeColor = muted, AutoSize = false, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10, FontStyle.Regular) };
                var bar = new Panel { BackColor = highlight, Height = 4, Dock = DockStyle.Top };
                var inner = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(0, 8, 0, 0) };
                inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                inner.Controls.Add(t, 0, 0);
                inner.Controls.Add(b, 0, 1);
                cardPanel.Controls.Add(inner);
                cardPanel.Controls.Add(bar);
                return cardPanel;
            }

            homeLayout.Controls.Add(BuildCard("Bắt đầu nhanh", "1) Chọn CSV ở tab Create Account. 2) Nhập catch-all email ở Settings. 3) Nhấn Start để chạy. 4) Xem log ở khung bên phải.", accent), 0, 0);
            homeLayout.Controls.Add(BuildCard("Proxy tùy chọn", "Bạn có thể bật proxy trong Settings (host/port/user/pass). Nếu không bật, tool chạy trực tiếp.", accentDeep), 1, 0);
            homeLayout.Controls.Add(BuildCard("Tính năng sắp tới", "Crawl Data và Coupon sẽ được thêm. Bạn có thể chuẩn bị dữ liệu và proxy trước.", accentHover), 0, 1);
            homeLayout.Controls.Add(BuildCard("Lưu ý bảo trì", "Giới hạn luồng tối đa 4 để tránh bị chặn. Luôn kiểm tra log khi có lỗi.", Color.FromArgb(245, 158, 11)), 1, 1);
            tabHome.Controls.Add(homeLayout);

            // --- CREATE ACCOUNT TAB ---
            var createLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 12, 16, 16),
                ColumnCount = 2,
                BackColor = bg
            };
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var leftCard = new Panel { BackColor = card, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var leftStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = card
            };
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // title
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // path row
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // file info
            leftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // grid
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // actions

            Label lblPath = new Label() { Text = "File Data", Dock = DockStyle.Top, AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), Padding = new Padding(0, 0, 0, 6) };

            var pathRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                Height = 36,
                AutoSize = true
            };
            pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            txtPath = new TextBox() { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0), BackColor = Color.White, ForeColor = fg, BorderStyle = BorderStyle.FixedSingle };
            btnBrowse = CreateAccentButton("Chọn File", new Point(0, 0), new Size(0, 32), accent, accentHover); btnBrowse.Dock = DockStyle.Fill;
            btnHelp = CreateAccentButton("Hướng dẫn", new Point(0, 0), new Size(0, 32), accentDeep, Color.FromArgb(8, 145, 178)); btnHelp.Dock = DockStyle.Fill;
            pathRow.Controls.Add(txtPath, 0, 0);
            pathRow.Controls.Add(btnBrowse, 1, 0);
            pathRow.Controls.Add(btnHelp, 2, 0);

            lblFileInfo = new Label() { Text = "Chưa chọn file", Dock = DockStyle.Top, AutoSize = true, ForeColor = muted, Padding = new Padding(0, 6, 0, 10) };

            dgvPreview = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = gridSource,
                BackgroundColor = Color.White
            };
            StyleGrid(dgvPreview, bg, card, fg, accent);
            dgvPreview.RowTemplate.Height = 32;

            var actionRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };
            Label lblConcurrency = new Label() { Text = "Số luồng", AutoSize = true, ForeColor = fg, Margin = new Padding(0, 8, 6, 0) };
            txtConcurrency = new TextBox() { Width = 60, Text = "2", BackColor = Color.White, ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 12, 0) };
            btnStart = CreateAccentButton("BẮT ĐẦU CHẠY", new Point(0, 0), new Size(220, 38), accent, accentHover);
            btnStart.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            actionRow.Controls.Add(lblConcurrency);
            actionRow.Controls.Add(txtConcurrency);
            actionRow.Controls.Add(btnStart);

            leftStack.Controls.Add(lblPath, 0, 0);
            leftStack.Controls.Add(pathRow, 0, 1);
            leftStack.Controls.Add(lblFileInfo, 0, 2);
            leftStack.Controls.Add(dgvPreview, 0, 3);
            leftStack.Controls.Add(actionRow, 0, 4);
            leftCard.Controls.Add(leftStack);

            progressBar = new ProgressBar(){ Dock = DockStyle.Top, Height = 14, Style = ProgressBarStyle.Continuous, ForeColor = accent, Margin = new Padding(0, 6, 0, 10) };
            lblStatus = new Label(){ Text = "Sẵn sàng", Dock = DockStyle.Top, AutoSize = false, Height = 24, ForeColor = muted, Padding = new Padding(0, 4, 0, 4) };
            rtbLog = new RichTextBox() { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 10, FontStyle.Regular) };

            var logHeader = new Label { Text = "Log", Dock = DockStyle.Top, AutoSize = false, Height = 26, ForeColor = fg, Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold) };

            var rightStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = card
            };
            rightStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rightStack.Controls.Add(logHeader, 0, 0);
            rightStack.Controls.Add(lblStatus, 0, 1);
            rightStack.Controls.Add(progressBar, 0, 2);
            rightStack.Controls.Add(rtbLog, 0, 3);

            var rightCard = new Panel { BackColor = card, Dock = DockStyle.Fill, Padding = new Padding(16) };
            rightCard.Controls.Add(rightStack);

            createLayout.Controls.Add(leftCard, 0, 0);
            createLayout.Controls.Add(rightCard, 1, 0);
            createLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabCreate.Controls.Add(createLayout);

            // --- SETTINGS TAB ---
            var settingsCard = new Panel { Dock = DockStyle.Fill, BackColor = card, Padding = new Padding(20) };
            var lblSettingsTitle = new Label { Text = "Cấu hình", ForeColor = fg, AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold) };

            var lblEmail = new Label(){ Text = "Email Catch-All (Gmail nhận mọi alias)", Location = new Point(0, 40), AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold) };
            txtCatchAllEmail = new TextBox(){ Location = new Point(0, 72), Width = 260, BackColor = Color.FromArgb(24,24,24), ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "your-catchall@gmail.com" };
            btnSaveEmail = CreateAccentButton("Lưu Email", new Point(270, 70), new Size(120, 32), accent, accentHover);
            btnSaveEmail.Click += (s,e)=> SaveEmailToFile();

            var separator1 = new Panel { BackColor = Color.FromArgb(50,50,50), Height = 1, Width = 520, Location = new Point(0, 118) };

            var lblProxyTitle = new Label(){ Text = "Proxy (tùy chọn)", Location = new Point(0, 134), AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold) };
            chkUseProxy = new CheckBox(){ Text = "Bật proxy", Location = new Point(0, 162), AutoSize = true, ForeColor = fg, BackColor = Color.Transparent };
            var lblProxyHost = new Label(){ Text = "Host", Location = new Point(0, 192), AutoSize = true, ForeColor = fg };
            txtProxyHost = new TextBox(){ Location = new Point(60, 188), Width = 200, BackColor = Color.FromArgb(24,24,24), ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "103.179.188.215" };
            var lblProxyPort = new Label(){ Text = "Port", Location = new Point(280, 192), AutoSize = true, ForeColor = fg };
            txtProxyPort = new TextBox(){ Location = new Point(330, 188), Width = 80, BackColor = Color.FromArgb(24,24,24), ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "13528" };
            var lblProxyUser = new Label(){ Text = "User", Location = new Point(0, 224), AutoSize = true, ForeColor = fg };
            txtProxyUser = new TextBox(){ Location = new Point(60, 220), Width = 200, BackColor = Color.FromArgb(24,24,24), ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "username" };
            var lblProxyPass = new Label(){ Text = "Pass", Location = new Point(0, 256), AutoSize = true, ForeColor = fg };
            txtProxyPass = new TextBox(){ Location = new Point(60, 252), Width = 200, BackColor = Color.FromArgb(24,24,24), ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true, PlaceholderText = "password" };
            btnSaveProxy = CreateAccentButton("Lưu Proxy", new Point(0, 292), new Size(120, 32), accent, accentHover);
            btnSaveProxy.Click += (s,e)=> SaveProxyToFile();

            settingsCard.Controls.Add(lblSettingsTitle);
            settingsCard.Controls.Add(lblEmail);
            settingsCard.Controls.Add(txtCatchAllEmail);
            settingsCard.Controls.Add(btnSaveEmail);
            settingsCard.Controls.Add(separator1);
            settingsCard.Controls.Add(lblProxyTitle);
            settingsCard.Controls.Add(chkUseProxy);
            settingsCard.Controls.Add(lblProxyHost);
            settingsCard.Controls.Add(txtProxyHost);
            settingsCard.Controls.Add(lblProxyPort);
            settingsCard.Controls.Add(txtProxyPort);
            settingsCard.Controls.Add(lblProxyUser);
            settingsCard.Controls.Add(txtProxyUser);
            settingsCard.Controls.Add(lblProxyPass);
            settingsCard.Controls.Add(txtProxyPass);
            settingsCard.Controls.Add(btnSaveProxy);
            tabSettings.Controls.Add(settingsCard);

            // --- PLACEHOLDER TABS ---
            void AddComingSoon(TabPage page, string title)
            {
                var panel = new Panel { Dock = DockStyle.Fill, BackColor = card, Padding = new Padding(20) };
                var lbl = new Label { Text = $"{title} sẽ sớm có mặt. Bạn có thể chuẩn bị dữ liệu và proxy trước.", ForeColor = fg, AutoSize = false, Dock = DockStyle.Top, Height = 80, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
                var sub = new Label { Text = "Gợi ý: xác định endpoint, cấu trúc file đầu vào và luồng xử lý để tích hợp nhanh.", ForeColor = muted, AutoSize = false, Dock = DockStyle.Top, Height = 60, Font = new Font("Segoe UI", 10, FontStyle.Regular) };
                panel.Controls.Add(sub);
                panel.Controls.Add(lbl);
                page.Controls.Add(panel);
            }
            // --- CRAWL DATA TAB ---
            var crawlLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 12, 16, 16),
                ColumnCount = 2,
                BackColor = bg
            };
            crawlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            crawlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            var crawlLeftCard = new Panel { BackColor = card, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var crawlLeftStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = card
            };
            crawlLeftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // title
            crawlLeftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // path row
            crawlLeftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // file info
            crawlLeftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // grid
            crawlLeftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // actions (concurrency + button)
            crawlLeftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // progress + status

            var lblCrawlTitle = new Label() { Text = "Chọn file KetQua_*.csv để crawl", Dock = DockStyle.Top, AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), Padding = new Padding(0, 0, 0, 6) };

            var crawlPathRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                Height = 38,
                AutoSize = true
            };
            crawlPathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            crawlPathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            txtCrawlPath = new TextBox() { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0), BackColor = Color.White, ForeColor = fg, BorderStyle = BorderStyle.FixedSingle };
            btnCrawlBrowse = CreateAccentButton("Chọn File", new Point(0, 0), new Size(100, 34), accent, accentHover); btnCrawlBrowse.Dock = DockStyle.Fill; btnCrawlBrowse.Margin = new Padding(0);
            crawlPathRow.Controls.Add(txtCrawlPath, 0, 0);
            crawlPathRow.Controls.Add(btnCrawlBrowse, 1, 0);

            lblCrawlFileInfo = new Label() { Text = "Chưa chọn file", Dock = DockStyle.Top, AutoSize = true, ForeColor = muted, Padding = new Padding(0, 6, 0, 10) };

            dgvCrawl = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = false,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = crawlGridSource,
                BackgroundColor = Color.White
            };
            StyleGrid(dgvCrawl, bg, card, fg, accent);
            dgvCrawl.RowTemplate.Height = 36;
            // Columns - checkbox, email, và 2 nút
            var colCheck = new DataGridViewCheckBoxColumn() { HeaderText = "", Width = 30, ReadOnly = false };
            dgvCrawl.Columns.Add(colCheck);
            dgvCrawl.Columns.Add(new DataGridViewTextBoxColumn(){ DataPropertyName = nameof(AccountData.Email), HeaderText = "Email", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            var colUpdate = new DataGridViewButtonColumn(){ HeaderText = "", Text = "Cập nhật", UseColumnTextForButtonValue = true, Width = 80 };
            var colView = new DataGridViewButtonColumn(){ HeaderText = "", Text = "Xem", UseColumnTextForButtonValue = true, Width = 60 };
            dgvCrawl.Columns.Add(colUpdate);
            dgvCrawl.Columns.Add(colView);
            dgvCrawl.CellContentClick += DgvCrawl_CellContentClick;
            dgvCrawl.CellValueChanged += DgvCrawl_CellValueChanged;
            dgvCrawl.CurrentCellDirtyStateChanged += (s, e) => { if (dgvCrawl.IsCurrentCellDirty) dgvCrawl.CommitEdit(DataGridViewDataErrorContexts.Commit); };

            // Actions container with 3 rows
            var crawlActionsRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                RowCount = 3,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };
            crawlActionsRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            crawlActionsRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            crawlActionsRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            
            // Dòng 1: Số luồng
            var concurrencyRow = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            concurrencyRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            concurrencyRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            var lblCrawlConcurrency = new Label() { Text = "Số luồng:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, AutoSize = true, ForeColor = fg };
            txtCrawlConcurrency = new TextBox() { Text = "2", Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0), BackColor = Color.White, ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            concurrencyRow.Controls.Add(lblCrawlConcurrency, 0, 0);
            concurrencyRow.Controls.Add(txtCrawlConcurrency, 1, 0);
            
            // Dòng 2: Nút Cập nhật tất cả
            btnCrawlAll = CreateAccentButton("Cập nhật tất cả", new Point(0, 0), new Size(100, 34), accent, accentHover);
            btnCrawlAll.Dock = DockStyle.Top;
            btnCrawlAll.Margin = new Padding(0, 0, 0, 4);
            
            // Dòng 3: Nút Cập nhật acc đã chọn
            btnCrawlSelected = CreateAccentButton("Cập nhật acc đã chọn", new Point(0, 0), new Size(100, 34), Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            btnCrawlSelected.Dock = DockStyle.Top;
            btnCrawlSelected.Margin = new Padding(0);
            btnCrawlSelected.Enabled = false;
            
            crawlActionsRow.Controls.Add(concurrencyRow, 0, 0);
            crawlActionsRow.Controls.Add(btnCrawlAll, 0, 1);
            crawlActionsRow.Controls.Add(btnCrawlSelected, 0, 2);

            // Progress + Status row
            var crawlProgressRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };
            crawlProgressRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            crawlProgressRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            
            crawlProgressBar = new ProgressBar() { Dock = DockStyle.Fill, Height = 26, Margin = new Padding(0, 0, 0, 4) };
            lblCrawlStatus = new Label() { Text = "Sẵn sàng", Dock = DockStyle.Fill, AutoSize = true, ForeColor = muted, Padding = new Padding(0, 4, 0, 0) };
            
            crawlProgressRow.Controls.Add(crawlProgressBar, 0, 0);
            crawlProgressRow.Controls.Add(lblCrawlStatus, 0, 1);

            crawlLeftStack.Controls.Add(lblCrawlTitle, 0, 0);
            crawlLeftStack.Controls.Add(crawlPathRow, 0, 1);
            crawlLeftStack.Controls.Add(lblCrawlFileInfo, 0, 2);
            crawlLeftStack.Controls.Add(dgvCrawl, 0, 3);
            crawlLeftStack.Controls.Add(crawlActionsRow, 0, 4);
            crawlLeftStack.Controls.Add(crawlProgressRow, 0, 5);
            crawlLeftCard.Controls.Add(crawlLeftStack);

            var crawlRightCard = new Panel { BackColor = card, Dock = DockStyle.Fill, Padding = new Padding(16) };
            var detailsHeader = new Label { Text = "Thông tin tài khoản", Dock = DockStyle.Top, AutoSize = false, Height = 26, ForeColor = fg, Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold) };

            // Account info label (email + userType)
            lblCrawlAccountInfo = new Label { Text = "—", Dock = DockStyle.Top, AutoSize = false, Height = 22, ForeColor = Color.FromArgb(108,115,125), Font = new Font("Segoe UI", 9, FontStyle.Regular) };

            // Date range label
            lblCrawlDateRange = new Label { Text = "Khoảng thời gian: —", Dock = DockStyle.Top, AutoSize = false, Height = 22, ForeColor = Color.FromArgb(108,115,125) };

            // Summary cards panel
            crawlSummaryPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 115,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 6),
                AutoScroll = true,
                BackColor = card
            };
            crawlSummaryPanel.Controls.Add(BuildSummaryCard("Gross", out lblSumGross, Color.FromArgb(0, 100, 255)));
            crawlSummaryPanel.Controls.Add(BuildSummaryCard("Net", out lblSumNet, Color.FromArgb(105, 201, 0)));
            crawlSummaryPanel.Controls.Add(BuildSummaryCard("Paid", out lblSumPaid, Color.FromArgb(105, 201, 0)));
            crawlSummaryPanel.Controls.Add(BuildSummaryCard("Refunded", out lblSumRefunded, Color.FromArgb(179, 0, 0)));
            crawlSummaryPanel.Controls.Add(BuildSummaryCard("Denied", out lblSumDenied, Color.FromArgb(255, 0, 0)));

            // Graph table instead of chart
            dgvGraph = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 240,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            StyleGrid(dgvGraph, bg, card, fg, accent);
            dgvGraph.Columns.Add(new DataGridViewTextBoxColumn{ HeaderText = "Date", DataPropertyName = "Date" });
            dgvGraph.Columns.Add(new DataGridViewTextBoxColumn{ HeaderText = "Gross", DataPropertyName = "Gross" });
            dgvGraph.Columns.Add(new DataGridViewTextBoxColumn{ HeaderText = "Net", DataPropertyName = "Net" });
            dgvGraph.Columns.Add(new DataGridViewTextBoxColumn{ HeaderText = "Paid", DataPropertyName = "Paid" });
            dgvGraph.Columns.Add(new DataGridViewTextBoxColumn{ HeaderText = "Refunded", DataPropertyName = "Refunded" });
            dgvGraph.Columns.Add(new DataGridViewTextBoxColumn{ HeaderText = "Denied", DataPropertyName = "Denied" });

            // Sales summary grid
            dgvSalesSummary = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            StyleGrid(dgvSalesSummary, bg, card, fg, accent);
            dgvSalesSummary.RowTemplate.Height = 28;

            // Raw details fallback (hidden)
            rtbCrawlDetails = new RichTextBox(){ Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, ForeColor = fg, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 10, FontStyle.Regular), Visible = false };

            var rightStack2 = new TableLayoutPanel{ Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, BackColor = card };
            rightStack2.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header
            rightStack2.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // account info
            rightStack2.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // date range
            rightStack2.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // summary cards
            rightStack2.RowStyles.Add(new RowStyle(SizeType.Absolute, 250)); // graph table (fixed height)
            rightStack2.RowStyles.Add(new RowStyle(SizeType.Percent, 75)); // sales grid (more space)
            rightStack2.RowStyles.Add(new RowStyle(SizeType.Percent, 25)); // raw details (less space)
            rightStack2.Controls.Add(detailsHeader, 0, 0);
            rightStack2.Controls.Add(lblCrawlAccountInfo, 0, 1);
            rightStack2.Controls.Add(lblCrawlDateRange, 0, 2);
            rightStack2.Controls.Add(crawlSummaryPanel, 0, 3);
            rightStack2.Controls.Add(dgvGraph, 0, 4);
            rightStack2.Controls.Add(dgvSalesSummary, 0, 5);
            rightStack2.Controls.Add(rtbCrawlDetails, 0, 6);
            crawlRightCard.Controls.Add(rightStack2);

            crawlLayout.Controls.Add(crawlLeftCard, 0, 0);
            crawlLayout.Controls.Add(crawlRightCard, 1, 0);
            crawlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabCrawl.Controls.Add(crawlLayout);
            AddComingSoon(tabCoupon, "Coupon");

            // Compose
            mainPanel.Controls.Add(mainTabs);
            this.Controls.Add(mainPanel);
            this.Controls.Add(navPanel);

            // NAV BEHAVIOR
            void Activate(Button btn, TabPage tab, string title, string subtitle)
            {
                mainTabs.SelectedTab = tab;
                foreach (var b in navButtons)
                {
                    b.BackColor = b == btn ? accent : Color.FromArgb(231, 235, 243);
                    b.ForeColor = b == btn ? Color.White : fg;
                }
            }

            btnNavHome.Click += (s, e) => Activate(btnNavHome, tabHome, "PayKickstart Dashboard", "Tổng quan & hướng dẫn nhanh");
            btnNavCreate.Click += (s, e) => Activate(btnNavCreate, tabCreate, "Create Account", "Chọn CSV, xem preview và chạy");
            btnNavCrawl.Click += (s, e) => Activate(btnNavCrawl, tabCrawl, "Crawl Data", "Chọn KetQua CSV và cập nhật dữ liệu");
            btnNavCoupon.Click += (s, e) => Activate(btnNavCoupon, tabCoupon, "Coupon", "Quản lý mã giảm giá (sắp ra mắt)");
            btnNavSettings.Click += (s, e) => Activate(btnNavSettings, tabSettings, "Settings", "Proxy & Email cấu hình");

            // Default page
            Activate(btnNavHome, tabHome, "PayKickstart Dashboard", "Tổng quan & hướng dẫn nhanh");

            // Events
            btnBrowse.Click += BtnBrowse_Click;
            btnStart.Click += BtnStart_Click;
            btnHelp.Click += BtnHelp_Click;
            btnCrawlBrowse.Click += BtnCrawlBrowse_Click;
            btnCrawlAll.Click += BtnCrawlAll_Click;
            btnCrawlSelected.Click += BtnCrawlSelected_Click;

            EnsureDirectoriesAndDefaults();
            Directory.CreateDirectory(crawlDir);
            LoadEmailFromFile();
            LoadProxyFromFile();
        }

        private void BtnCrawlBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV Files|*.csv";
            ofd.InitialDirectory = Directory.Exists(resultsDir) ? resultsDir : AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtCrawlPath.Text = ofd.FileName;
                Log($"Crawl: Đã chọn file: {ofd.FileName}");
                LoadResultsCsvForCrawl(ofd.FileName);
            }
        }

        private void LoadResultsCsvForCrawl(string path)
        {
            try
            {
                var accounts = MiniExcel.Query<AccountData>(path).ToList();
                crawlAccounts = accounts;
                lastCrawlCsvPath = path;
                crawlGridSource.DataSource = new BindingList<AccountData>(accounts);
                UpdateCrawlFileInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không đọc được CSV KetQua: {ex.Message}");
            }
        }

        private void UpdateCrawlFileInfo()
        {
            if (string.IsNullOrEmpty(lastCrawlCsvPath)) { lblCrawlFileInfo.Text = "Chưa chọn file"; return; }
            var fileName = Path.GetFileName(lastCrawlCsvPath);
            lblCrawlFileInfo.Text = $"Đang chọn: {fileName} | Số tài khoản: {crawlAccounts.Count}";
        }

        private async void DgvCrawl_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var acc = (crawlGridSource[e.RowIndex] as AccountData);
            if (acc == null) return;

            // Last two columns are buttons
            if (e.ColumnIndex == dgvCrawl.Columns.Count - 2)
            {
                // Cập nhật thông tin (crawl)
                await CrawlAffiliateDataAsync(acc);
            }
            else if (e.ColumnIndex == dgvCrawl.Columns.Count - 1)
            {
                // Xem thông tin
                ShowAccountDetails(acc);
            }
        }

        private async Task CrawlAffiliateDataAsync(AccountData acc)
        {
            Log($"[Crawl:{acc.Email}] Đang đăng nhập và lấy dữ liệu...");
            IWebDriver? driver = null;
            try
            {
                var proxyConfig = currentProxyConfig;
                driver = BrowserHelper.OpenBrowser(proxyConfig.Enabled ? proxyConfig : null);
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                driver.Navigate().GoToUrl("https://app.paykickstart.com/sign-in");
                Thread.Sleep(3000);

                var emailInput = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Your Email Address']")));
                FillInputRealUser(driver, emailInput, acc.Email);
                var passInput = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Your Password']")));
                FillInputRealUser(driver, passInput, acc.GeneratedPassword ?? acc.MailPass ?? string.Empty);

                // Submit
                try
                {
                    var submitBtn = driver.FindElements(By.CssSelector("button[type='submit'], .button.is-primary"));
                    if (submitBtn.Count > 0)
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", submitBtn[0]);
                    }
                    else
                    {
                        passInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                    }
                }
                catch {}

                // Wait for dashboard
                bool logged = false;
                for (int i = 0; i < 30; i++)
                {
                    if (driver.Url.Contains("dashboard") || driver.Manage().Cookies.AllCookies.Count > 3)
                    { logged = true; break; }
                    Thread.Sleep(1000);
                }
                if (!logged) throw new Exception("Đăng nhập thất bại");

                // Try in-browser fetch first (best chance with session/CSRF)
                var apiUri = new Uri("https://app.paykickstart.com/api/v1/dashboard/affiliate/data");
                var json = await FetchDashboardJsonViaBrowserAsync(driver, apiUri);
                if (string.IsNullOrEmpty(json))
                {
                    // Fallback to HttpClient + cookies transfer
                    json = await FetchDashboardJsonWithDriverCookiesAsync(driver, apiUri);
                }
                if (string.IsNullOrEmpty(json)) throw new Exception("Không nhận được JSON từ API");

                Directory.CreateDirectory(crawlDir);
                var safeName = SafeFileName(acc.Email) + ".json";
                var outPath = Path.Combine(crawlDir, safeName);
                await File.WriteAllTextAsync(outPath, json, Encoding.UTF8);
                Log($"[Crawl:{acc.Email}] Đã lưu JSON: {outPath}");
            }
            catch (Exception ex)
            {
                Log($"[Crawl:{acc.Email}] Lỗi: {ex.Message}");
            }
            finally
            {
                try { driver?.Quit(); } catch {}
            }
        }

        private async Task<string> FetchDashboardJsonWithDriverCookiesAsync(IWebDriver driver, Uri apiUri)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            // Transfer cookies from Selenium to HttpClient
            foreach (var ck in driver.Manage().Cookies.AllCookies)
            {
                try
                {
                    var cookie = new System.Net.Cookie(ck.Name, ck.Value, ck.Path ?? "/", ck.Domain.TrimStart('.'))
                    {
                        Secure = ck.Secure,
                        HttpOnly = ck.IsHttpOnly,
                        Expires = ck.Expiry ?? DateTime.MinValue
                    };
                    handler.CookieContainer.Add(new Uri("https://app.paykickstart.com"), cookie);
                }
                catch { }
            }

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://app.paykickstart.com/");

            var resp = await client.GetAsync(apiUri);
            if (!resp.IsSuccessStatusCode) return string.Empty;
            return await resp.Content.ReadAsStringAsync();
        }

        private async Task<string> FetchDashboardJsonViaBrowserAsync(IWebDriver driver, Uri apiUri)
        {
            try
            {
                // Ensure we are on same-origin page before fetch
                try { driver.Navigate().GoToUrl("https://app.paykickstart.com/"); Thread.Sleep(1000); } catch {}

                var js = @"
                    var url = arguments[0];
                    var cb = arguments[arguments.length - 1];
                    try {
                        fetch(url, {
                            credentials: 'include',
                            headers: {
                                'Accept': 'application/json, text/plain, */*',
                                'X-Requested-With': 'XMLHttpRequest'
                            }
                        }).then(function(r){
                            return r.text();
                        }).then(function(t){
                            cb(t);
                        }).catch(function(err){
                            cb('');
                        });
                    } catch(e) {
                        cb('');
                    }
                ";
                var exec = (IJavaScriptExecutor)driver;
                var result = exec.ExecuteAsyncScript(js, apiUri.ToString());
                // Result can be string or null
                return await Task.FromResult(result as string ?? string.Empty);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string SafeFileName(string email)
        {
            var s = (email ?? "").ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
                else sb.Append('_');
            }
            return sb.ToString().Trim('_');
        }

        private async void BtnCrawlAll_Click(object? sender, EventArgs e)
        {
            if (crawlAccounts == null || crawlAccounts.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn file CSV trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtCrawlConcurrency.Text, out int concurrency) || concurrency < 1 || concurrency > 10)
            {
                MessageBox.Show("Số luồng phải từ 1-10!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable controls
            btnCrawlAll.Enabled = false;
            btnCrawlBrowse.Enabled = false;
            txtCrawlConcurrency.Enabled = false;
            dgvCrawl.Enabled = false;

            crawlProgressBar.Value = 0;
            crawlProgressBar.Maximum = crawlAccounts.Count;
            lblCrawlStatus.Text = $"Đang crawl 0/{crawlAccounts.Count}...";

            var accounts = crawlAccounts.ToList();
            var completed = 0;
            var failed = 0;
            var semaphore = new SemaphoreSlim(concurrency, concurrency);
            var tasks = new List<Task>();

            try
            {
                foreach (var acc in accounts)
                {
                    var task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await CrawlAffiliateDataAsync(acc);
                            Interlocked.Increment(ref completed);
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref failed);
                            Log($"[{acc.Email}] Lỗi: {ex.Message}");
                        }
                        finally
                        {
                            semaphore.Release();
                            var total = completed + failed;
                            this.Invoke(() =>
                            {
                                crawlProgressBar.Value = total;
                                lblCrawlStatus.Text = $"Đang crawl {total}/{accounts.Count} (Thành công: {completed}, Lỗi: {failed})";
                            });
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                this.Invoke(() =>
                {
                    lblCrawlStatus.Text = $"Hoàn tất! Thành công: {completed}/{accounts.Count}, Lỗi: {failed}";
                    MessageBox.Show($"Cập nhật xong!\n\nThành công: {completed}\nLỗi: {failed}", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi crawl batch: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable controls
                this.Invoke(() =>
                {
                    btnCrawlAll.Enabled = true;
                    btnCrawlBrowse.Enabled = true;
                    txtCrawlConcurrency.Enabled = true;
                    dgvCrawl.Enabled = true;
                });
            }
        }

        private void DgvCrawl_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                UpdateCrawlSelectedButtonState();
            }
        }

        private void UpdateCrawlSelectedButtonState()
        {
            int checkedCount = 0;
            foreach (DataGridViewRow row in dgvCrawl.Rows)
            {
                if (row.Cells[0].Value is bool isChecked && isChecked)
                {
                    checkedCount++;
                }
            }
            
            btnCrawlSelected.Enabled = checkedCount > 0;
        }

        private async void BtnCrawlSelected_Click(object? sender, EventArgs e)
        {
            // Get selected accounts
            var selectedAccounts = new List<AccountData>();
            foreach (DataGridViewRow row in dgvCrawl.Rows)
            {
                if (row.Cells[0].Value is bool isChecked && isChecked && row.DataBoundItem is AccountData acc)
                {
                    selectedAccounts.Add(acc);
                }
            }

            if (selectedAccounts.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 tài khoản!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!int.TryParse(txtCrawlConcurrency.Text, out int concurrency) || concurrency < 1 || concurrency > 10)
            {
                MessageBox.Show("Số luồng phải từ 1 đến 10!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Disable controls
            btnCrawlAll.Enabled = false;
            btnCrawlSelected.Enabled = false;
            btnCrawlBrowse.Enabled = false;
            txtCrawlConcurrency.Enabled = false;
            dgvCrawl.Enabled = false;

            // Progress tracking
            int total = selectedAccounts.Count;
            int completed = 0;
            int success = 0;
            int failed = 0;

            var semaphore = new SemaphoreSlim(concurrency);
            var tasks = new List<Task>();

            try
            {
                foreach (var acc in selectedAccounts)
                {
                    var task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await CrawlAffiliateDataAsync(acc);
                            Interlocked.Increment(ref success);
                        }
                        catch
                        {
                            Interlocked.Increment(ref failed);
                        }
                        finally
                        {
                            Interlocked.Increment(ref completed);
                            semaphore.Release();

                            // Update UI
                            this.Invoke(() =>
                            {
                                crawlProgressBar.Value = (int)((double)completed / total * 100);
                                lblCrawlStatus.Text = $"Đang crawl {completed}/{total} (Thành công: {success}, Lỗi: {failed})";
                            });
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                // Summary
                this.Invoke(() =>
                {
                    MessageBox.Show($"Hoàn thành cập nhật {total} tài khoản đã chọn!\n\nThành công: {success}\nLỗi: {failed}", "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    crawlProgressBar.Value = 0;
                    lblCrawlStatus.Text = $"Sẵn sàng (Tổng: {crawlAccounts.Count} tài khoản)";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi crawl selected: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable controls
                this.Invoke(() =>
                {
                    btnCrawlAll.Enabled = true;
                    btnCrawlBrowse.Enabled = true;
                    txtCrawlConcurrency.Enabled = true;
                    dgvCrawl.Enabled = true;
                    UpdateCrawlSelectedButtonState();
                });
            }
        }

        private void ShowAccountDetails(AccountData acc)
        {
            try
            {
                var path = Path.Combine(crawlDir, SafeFileName(acc.Email) + ".json");
                if (!File.Exists(path)) 
                { 
                    MessageBox.Show("Chưa có dữ liệu, vui lòng bấm 'Cập nhật' để lấy thông tin tài khoản.", "Chưa có dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return; 
                }
                var json = File.ReadAllText(path, Encoding.UTF8);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                // Some responses wrap under "data"
                var data = root.TryGetProperty("data", out var dataEl) ? dataEl : root;

                // Account info (email + userType)
                string userType = data.TryGetProperty("userType", out var ut) ? ut.GetString() ?? "—" : "—";
                lblCrawlAccountInfo.Text = $"Email: {acc.Email} | Loại: {userType}";

                // Date range
                string startDate = data.TryGetProperty("startDate", out var sd) ? sd.GetString() ?? "—" : "—";
                string endDate = data.TryGetProperty("endDate", out var ed) ? ed.GetString() ?? "—" : "—";
                lblCrawlDateRange.Text = $"Khoảng thời gian: {startDate} → {endDate}";

                // Summary cards
                if (data.TryGetProperty("summary", out var summary))
                {
                    void SetSum(Label lbl, JsonElement el, string fallbackColor)
                    {
                        var formatted = el.TryGetProperty("formatted", out var f) ? (f.GetString() ?? "—") : "—";
                        lbl.Text = formatted;
                        Color col = Color.Black;
                        if (el.TryGetProperty("colors", out var colors) && colors.TryGetProperty("color", out var c))
                        {
                            var hex = c.GetString();
                            col = string.IsNullOrEmpty(hex) ? col : ParseHexColor(hex, lbl.ForeColor);
                        }
                        lbl.ForeColor = col;
                    }

                    if (summary.TryGetProperty("gross", out var g)) SetSum(lblSumGross, g, "#0064ff");
                    if (summary.TryGetProperty("net", out var n)) SetSum(lblSumNet, n, "#69c900");
                    if (summary.TryGetProperty("refunded", out var r)) SetSum(lblSumRefunded, r, "#b30000");
                    if (summary.TryGetProperty("paid", out var p)) SetSum(lblSumPaid, p, "#69C900");
                    if (summary.TryGetProperty("denied", out var d)) SetSum(lblSumDenied, d, "#ff0000");
                }

                // Graph table
                if (data.TryGetProperty("graph", out var graph))
                {
                    PopulateGraphTable(graph);
                }

                // Sales summary grid
                if (data.TryGetProperty("salesSummary", out var sales))
                {
                    PopulateSalesGrid(sales);
                }

                // Raw details minimal
                rtbCrawlDetails.Text = $"Email: {acc.Email}\nUserType: { (data.TryGetProperty("userType", out var userTypeEl) ? userTypeEl.GetString() : "-") }\nGraph points: {(data.TryGetProperty("graph", out var gr) ? gr.GetArrayLength() : 0)}";
            }
            catch (Exception ex)
            {
                rtbCrawlDetails.Text = "Lỗi hiển thị: " + ex.Message;
            }
        }

        private class GraphRow
        {
            public string Date { get; set; }
            public string Gross { get; set; }
            public string Net { get; set; }
            public string Paid { get; set; }
            public string Refunded { get; set; }
            public string Denied { get; set; }
        }

        private void PopulateGraphTable(JsonElement graph)
        {
            try
            {
                var rows = new BindingList<GraphRow>();
                foreach (var item in graph.EnumerateArray())
                {
                    string D(string key)
                    {
                        if (item.TryGetProperty(key, out var el))
                        {
                            if (el.TryGetProperty("formatted", out var f)) return f.GetString() ?? "";
                            if (el.TryGetProperty("value", out var v))
                            {
                                try { return v.GetDouble().ToString("0.##"); } catch { }
                            }
                        }
                        return "";
                    }

                    string dateStr = "";
                    if (item.TryGetProperty("date", out var de))
                    {
                        if (de.TryGetProperty("formatted", out var df)) dateStr = df.GetString() ?? "";
                        else if (de.TryGetProperty("value", out var dv)) dateStr = dv.GetString() ?? "";
                    }

                    rows.Add(new GraphRow
                    {
                        Date = dateStr,
                        Gross = D("gross"),
                        Net = D("net"),
                        Paid = D("paid"),
                        Refunded = D("refunded"),
                        Denied = D("denied")
                    });
                }
                dgvGraph.DataSource = rows;
            }
            catch { }
        }

        private class MetricItem
        {
            public string Metric { get; set; }
            public string Value { get; set; }
        }

        private void PopulateSalesGrid(JsonElement sales)
        {
            try
            {
                string F(string key)
                {
                    if (sales.TryGetProperty(key, out var el))
                    {
                        if (el.TryGetProperty("formatted", out var f)) return f.GetString() ?? "";
                    }
                    return "";
                }

                var items = new BindingList<MetricItem>(new List<MetricItem>
                {
                    new MetricItem{ Metric = "Sales Count", Value = F("salesCount") },
                    new MetricItem{ Metric = "Gross Revenue", Value = F("grossRevenue") },
                    new MetricItem{ Metric = "Net Revenue", Value = F("netRevenue") },
                    new MetricItem{ Metric = "Commissions", Value = F("commissionsAmount") },
                    new MetricItem{ Metric = "Refunds Count", Value = F("refundsCount") },
                    new MetricItem{ Metric = "Refunds Amount", Value = F("refundsAmount") },
                    new MetricItem{ Metric = "Refunds Rate", Value = F("refundsRate") },
                    new MetricItem{ Metric = "Conversion Rate", Value = F("conversionRate") },
                    new MetricItem{ Metric = "EPC", Value = F("epc") },
                    new MetricItem{ Metric = "One-time Count", Value = F("oneTimeCount") },
                    new MetricItem{ Metric = "One-time Total", Value = F("oneTimeTotal") },
                    new MetricItem{ Metric = "One-time Rate", Value = F("oneTimeRate") },
                    new MetricItem{ Metric = "Recurring Count", Value = F("recurringCount") },
                    new MetricItem{ Metric = "Recurring Total", Value = F("recurringTotal") },
                    new MetricItem{ Metric = "Recurring Rate", Value = F("recurringRate") },
                });

                dgvSalesSummary.DataSource = items;
            }
            catch { }
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            try
            {
                var guide = new GuideForm();
                guide.StartPosition = FormStartPosition.CenterParent;
                guide.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được Hướng dẫn: " + ex.Message);
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV Files|*.csv"; // Chỉ nhận CSV
            ofd.InitialDirectory = Directory.Exists(dataDir) ? dataDir : AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofd.FileName;
                Log($"Đã chọn file: {ofd.FileName}");
                LoadCsvPreview(ofd.FileName);
            }
        }

        // Đã bỏ các nút: Tải lại, Mở thư mục, Mở CSV, Tạo CSV mẫu

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPath.Text)) { MessageBox.Show("Chưa chọn file CSV!"); return; }

            var catchAllEmail = (txtCatchAllEmail.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(catchAllEmail))
            {
                MessageBox.Show("Chưa cấu hình Email Catch-All! Vào tab Settings để nhập.");
                return;
            }

            var proxyConfig = GetProxyConfigFromUi();
            if (proxyConfig.Enabled && !proxyConfig.IsValidHostPort())
            {
                MessageBox.Show("Proxy chưa đủ Host/Port. Kiểm tra lại tab Settings.");
                return;
            }

            btnStart.Enabled = false;
            Log("=== BẮT ĐẦU CHIẾN DỊCH ===");

            int maxParallel = 1;
            if (!int.TryParse(txtConcurrency.Text, out maxParallel)) maxParallel = 1;
            maxParallel = Math.Max(1, Math.Min(maxParallel, 4)); // Giới hạn để tránh bị chặn

            // Đọc file CSV bằng MiniExcel
            var accounts = MiniExcel.Query<AccountData>(txtPath.Text).ToList();
            cachedAccounts = accounts;
            lastLoadedPath = txtPath.Text;
            UpdateFileInfo();
            gridSource.DataSource = new BindingList<AccountData>(accounts);

            // Reset progress UI
            this.Invoke((MethodInvoker)delegate{ progressBar.Value = 0; lblStatus.Text = $"Đang xử lý 0/{accounts.Count}"; });

            // Chạy bất đồng bộ để không treo giao diện
            await Task.Run(() =>
            {
                int done = 0;
                Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = maxParallel }, acc =>
                {
                    ProcessOneAccount(acc, catchAllEmail, proxyConfig);
                    Interlocked.Increment(ref done);
                    this.Invoke((MethodInvoker)delegate{
                        progressBar.Value = (int)(100.0 * done / accounts.Count);
                        lblStatus.Text = $"Đang xử lý {done}/{accounts.Count}";
                    });
                });
            });

            // Lưu kết quả ra file mới
            Directory.CreateDirectory(resultsDir);
            string outPath = Path.Combine(resultsDir, "KetQua_" + DateTime.Now.ToString("HHmmss") + ".csv");
            SaveCsvWithQuotes(outPath, accounts);
            
            Log($"=== HOÀN THÀNH! File: {outPath} ===");
            MessageBox.Show("Đã chạy xong! Kiểm tra file kết quả.");
            this.Invoke((MethodInvoker)delegate { btnStart.Enabled = true; });
            LoadCsvPreview(outPath);
            this.Invoke((MethodInvoker)delegate{ progressBar.Value = 100; lblStatus.Text = "Hoàn tất"; });
        }

        private void LoadCsvPreview(string path)
        {
            try
            {
                var accounts = MiniExcel.Query<AccountData>(path).ToList();
                cachedAccounts = accounts;
                lastLoadedPath = path;
                gridSource.DataSource = new BindingList<AccountData>(accounts);
                UpdateFileInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không đọc được CSV: {ex.Message}");
            }
        }

        private void UpdateFileInfo()
        {
            if (string.IsNullOrEmpty(lastLoadedPath)) { lblFileInfo.Text = "Chưa chọn file"; return; }
            var fileName = Path.GetFileName(lastLoadedPath);
            lblFileInfo.Text = $"Đang chọn: {fileName} | Số dòng: {cachedAccounts.Count}";
        }

        private void EnsureDirectoriesAndDefaults()
        {
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(resultsDir);

            var defaultCsv = Path.Combine(dataDir, "Data.csv");
            if (File.Exists(defaultCsv) && string.IsNullOrWhiteSpace(txtPath.Text))
            {
                txtPath.Text = defaultCsv;
                Log($"Tự động chọn: {defaultCsv}");
                LoadCsvPreview(defaultCsv);
            }
        }

        private void OpenFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được thư mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveEmailToFile()
        {
            try
            {
                var val = (txtCatchAllEmail.Text ?? string.Empty).Trim();
                
                // Lưu cả email và password (password được nhập từ cột MailPass của CSV hoặc Settings)
                // Cho custom domain: email catch-all + 1 password chung
                // Cho Gmail: không cần lưu, vì mỗi account có password riêng trong CSV
                File.WriteAllText(emailConfigPath, val);
                
                // Lưu password cho catch-all email nếu là custom domain
                if (!string.IsNullOrEmpty(val) && !val.ToLower().Contains("@gmail.com"))
                {
                    var passwordConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_password.txt");
                    // Note: Nên hỏi user nhập password (hoặc lấy từ lần chạy trước)
                    File.WriteAllText(passwordConfigPath, ""); // Placeholder
                }
                
                MessageBox.Show(string.IsNullOrEmpty(val) ? "Đã lưu (đang để trống email)." : "Đã lưu email catch-all thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu email thất bại: " + ex.Message);
            }
        }

        private void LoadEmailFromFile()
        {
            try
            {
                if (File.Exists(emailConfigPath))
                {
                    var val = File.ReadAllText(emailConfigPath).Trim();
                    txtCatchAllEmail.Text = val;
                }
                
                // Load password cho catch-all email
                var passwordConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_password.txt");
                if (File.Exists(passwordConfigPath))
                {
                    catchAllEmail_Password = File.ReadAllText(passwordConfigPath).Trim();
                }
            }
            catch {}
        }

        private ProxyConfig GetProxyConfigFromUi()
        {
            var cfg = new ProxyConfig();
            cfg.Enabled = chkUseProxy.Checked;
            cfg.Host = (txtProxyHost.Text ?? string.Empty).Trim();
            cfg.Username = (txtProxyUser.Text ?? string.Empty).Trim();
            cfg.Password = (txtProxyPass.Text ?? string.Empty).Trim();
            int port;
            if (int.TryParse((txtProxyPort.Text ?? string.Empty).Trim(), out port)) cfg.Port = port; else cfg.Port = 0;
            currentProxyConfig = cfg;
            return cfg;
        }

        private void ApplyProxyToUi(ProxyConfig cfg)
        {
            chkUseProxy.Checked = cfg.Enabled;
            txtProxyHost.Text = cfg.Host;
            txtProxyPort.Text = cfg.Port > 0 ? cfg.Port.ToString() : string.Empty;
            txtProxyUser.Text = cfg.Username;
            txtProxyPass.Text = cfg.Password;
        }

        private void SaveProxyToFile()
        {
            try
            {
                var cfg = GetProxyConfigFromUi();
                var lines = new List<string>
                {
                    cfg.Enabled.ToString(),
                    cfg.Host ?? string.Empty,
                    cfg.Port.ToString(),
                    cfg.Username ?? string.Empty,
                    cfg.Password ?? string.Empty
                };
                File.WriteAllLines(proxyConfigPath, lines);
                MessageBox.Show("Đã lưu Proxy.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu Proxy thất bại: " + ex.Message);
            }
        }

        private void LoadProxyFromFile()
        {
            try
            {
                if (!File.Exists(proxyConfigPath)) return;
                var lines = File.ReadAllLines(proxyConfigPath);
                var cfg = new ProxyConfig();
                if (lines.Length > 0)
                {
                    bool enabled;
                    if (bool.TryParse(lines[0], out enabled)) cfg.Enabled = enabled;
                }
                if (lines.Length > 1) cfg.Host = lines[1];
                if (lines.Length > 2)
                {
                    int p;
                    if (int.TryParse(lines[2], out p)) cfg.Port = p;
                }
                if (lines.Length > 3) cfg.Username = lines[3];
                if (lines.Length > 4) cfg.Password = lines[4];

                currentProxyConfig = cfg;
                ApplyProxyToUi(cfg);
            }
            catch {}
        }

        private Button CreateAccentButton(string text, Point location, Size size, Color accent, Color accentHover)
        {
            var btn = new Button(){ Text = text, Location = location, Size = size, BackColor = accent, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
            btn.MouseEnter += (s,e)=>{ btn.BackColor = accentHover; };
            btn.MouseLeave += (s,e)=>{ btn.BackColor = accent; };
            return btn;
        }

        private void StyleGrid(DataGridView grid, Color bgLight, Color bgCard, Color fgText, Color accent)
        {
            grid.BackgroundColor = bgCard;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 239, 245);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = fgText;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.ColumnHeadersHeight = 32;
            grid.RowHeadersVisible = false;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = fgText;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = fgText;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private Panel BuildSummaryCard(string title, out Label valueLabel, Color accentColor)
        {
            var card = new Panel { Width = 175, Height = 92, Margin = new Padding(0, 0, 10, 0), BackColor = Color.White };
            var bar = new Panel { Height = 4, Dock = DockStyle.Top, BackColor = accentColor };
            var titleLbl = new Label { Text = title, Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(108,115,125), Font = new Font("Segoe UI", 9, FontStyle.Regular), Padding = new Padding(6, 3, 0, 0) };
            valueLabel = new Label { Text = "$0.00", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(28,34,45), Font = new Font("Segoe UI", 15, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 6, 4) };
            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLbl);
            card.Controls.Add(bar);
            return card;
        }

        private static Color ParseHexColor(string hex, Color fallback)
        {
            try { return ColorTranslator.FromHtml(hex); } catch { return fallback; }
        }

        // --- HÀM XỬ LÝ CHÍNH (LOGIC ĐĂNG KÝ) ---
        private void ProcessOneAccount(AccountData acc, string catchAllEmail, ProxyConfig proxyConfig)
        {
            Log($"[{acc.Email}] >> Bắt đầu xử lý...");
            Log($"[{acc.Email}] >> {(proxyConfig.Enabled ? "Dùng proxy" : "Không dùng proxy")}.");
            IWebDriver driver = null;
            try
            {
                // BƯỚC 1: MỞ TRÌNH DUYỆT & ĐĂNG KÝ
            driver = BrowserHelper.OpenBrowser(proxyConfig.Enabled ? proxyConfig : null);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                driver.Navigate().GoToUrl("https://app.paykickstart.com/sign-up/affiliate");
                Thread.Sleep(3000); // Chờ trang load

                // Đóng các overlay nếu có
                try {
                    ((IJavaScriptExecutor)driver).ExecuteScript(@"
                        const texts = ['accept', 'i agree', 'consent', 'got it'];
                        const btns = Array.from(document.querySelectorAll('button'));
                        for (const b of btns) {
                            const t = (b.innerText || '').toLowerCase();
                            if (texts.some(x => t.includes(x))) { try { b.click(); } catch(e) {} }
                        }
                    ");
                } catch {}

                // Điền thông tin - dùng SendKeys thật để trigger validation framework
                Log($"[{acc.Email}] >> Điền First Name...");
                var firstNameInput = wait.Until(d => d.FindElement(By.Name("first_name")));
                FillInputRealUser(driver, firstNameInput, acc.FirstName);
                Thread.Sleep(500);
                
                Log($"[{acc.Email}] >> Điền Last Name...");
                var lastNameInput = driver.FindElement(By.Name("last_name"));
                FillInputRealUser(driver, lastNameInput, acc.LastName);
                Thread.Sleep(500);
                
                Log($"[{acc.Email}] >> Điền Email...");
                var emailInput = driver.FindElement(By.Name("email"));
                FillInputRealUser(driver, emailInput, acc.Email);
                Thread.Sleep(500);

                // Click Checkbox TOS - dùng JavaScript để set checked state
                Log($"[{acc.Email}] >> Tick checkbox TOS...");
                try {
                    var chkTos = wait.Until(d => d.FindElement(By.Name("tos")));
                    ((IJavaScriptExecutor)driver).ExecuteScript(@"
                        const el = arguments[0];
                        
                        // Scroll vào view
                        el.scrollIntoView({block: 'center'});
                        
                        // Set checked state
                        el.checked = true;
                        el.setAttribute('checked', 'checked');
                        
                        // Trigger change event để Vue.js detect
                        el.dispatchEvent(new Event('change', { bubbles: true, cancelable: true }));
                        el.dispatchEvent(new Event('input', { bubbles: true, cancelable: true }));
                        
                        // Trigger click event
                        el.dispatchEvent(new Event('click', { bubbles: true }));
                        
                        // Xóa error class từ parent label nếu có
                        let label = el.closest('label');
                        if (label) {
                            label.classList.remove('is-danger', 'is-warning');
                            label.classList.add('is-success');
                        }
                    ", chkTos);
                    Thread.Sleep(500);
                } catch { Log("Không tìm thấy checkbox TOS, thử bỏ qua..."); }

                Log($"[{acc.Email}] >> Đang chờ nút Submit sẵn sàng...");

                // Đợi và kiểm tra trạng thái nút submit
                bool issubmitted = false;
                bool emailAlreadyRegistered = false;
                
                for(int attempt=0; attempt<90; attempt++) // Thử trong 3 phút (mỗi lần 2s)
                {
                    // CHECK API Response qua browser logs (nếu có lỗi 422 từ API)
                    try
                    {
                        var logs = driver.Manage().Logs.GetLog("browser");
                        foreach (var log in logs)
                        {
                            if (log.Message.Contains("422") && log.Message.Contains("post-step-account-details"))
                            {
                                Log($"[{acc.Email}] >> Phát hiện API trả về 422 - Email đã được đăng ký. Chuyển sang verify...");
                                emailAlreadyRegistered = true;
                                issubmitted = true;
                                break;
                            }
                        }
                        if (emailAlreadyRegistered) break;
                    }
                    catch { }
                    
                    // CHECK notification messages (backup method)
                    try
                    {
                        var notices = driver.FindElements(By.CssSelector("div.notices.is-bottom, .notification, [class*='notice'], [class*='alert']"));
                        foreach (var notice in notices)
                        {
                            if (notice.Displayed && notice.Text.Contains("already registered"))
                            {
                                Log($"[{acc.Email}] >> Email đã được đăng ký trước đó. Chuyển sang bước verify...");
                                emailAlreadyRegistered = true;
                                issubmitted = true;
                                break;
                            }
                        }
                        if (emailAlreadyRegistered) break;
                    }
                    catch { }
                    
                    try 
                    {
                        // Kiểm tra nếu đã chuyển trang
                        if (driver.Url.Contains("almost-there") || driver.Url.Contains("verify")) 
                        {
                            issubmitted = true;
                            Log($"[{acc.Email}] >> Đã chuyển trang thành công!");
                            break;
                        }
                        
                        // Tìm và kiểm tra nút submit
                        var btnSubmit = driver.FindElement(By.CssSelector("button.signup-registration-button"));
                        
                        // Kiểm tra nút có disabled không (qua attribute hoặc class)
                        var isDisabled = btnSubmit.GetAttribute("disabled");
                        var classes = btnSubmit.GetAttribute("class") ?? "";
                        
                        if (isDisabled == null && !classes.Contains("disabled") && btnSubmit.Enabled && btnSubmit.Displayed)
                        {
                            // Scroll và click bằng JS để chắc chắn
                            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                                arguments[0].scrollIntoView({block: 'center'});
                            ", btnSubmit);
                            Thread.Sleep(500);
                            
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnSubmit);
                            Log($"[{acc.Email}] >> Đã bấm Submit, đợi redirect...");
                            
                            // Sau khi click, đợi 3s xem có chuyển trang không
                            Thread.Sleep(3000);
                        }
                        else if (attempt % 10 == 0)
                        {
                            Log($"[{acc.Email}] >> Nút vẫn disabled, đợi thêm... ({attempt}/90)");
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Button chưa có, tiếp tục đợi
                    }
                    catch {}
                    
                    Thread.Sleep(2000);
                }

                if(!issubmitted) throw new Exception("Timeout: Không thể Submit sau 3 phút (Form validation hoặc Captcha)");

                Log($"[{acc.Email}] >> Đăng ký Web xong. Đợi Mail Verify...");
                
                // Nếu email chưa được đăng ký, tắt browser
                if (!emailAlreadyRegistered)
                {
                    driver.Quit();
                }
                else
                {
                    // Email đã đăng ký, có thể đã có sẵn tài khoản
                    driver.Quit();
                }

                // BƯỚC 2: VERIFY EMAIL
                Thread.Sleep(15000); // Chờ 15s mail về
                
                // Xác định loại email: Gmail riêng hay Catch-All từ domain custom
                bool isGmailAccount = acc.Email.ToLower().Contains("@gmail.com") || acc.Email.ToLower().Contains("@googlemail.com");
                string emailForVerify = isGmailAccount ? acc.Email : catchAllEmail;
                // Custom domain ưu tiên password catch-all đã lưu; nếu chưa có, fallback sang MailPass từ CSV (hỗ trợ file thập cẩm)
                string passwordForVerify = isGmailAccount 
                    ? acc.MailPass 
                    : (!string.IsNullOrEmpty(catchAllEmail_Password) ? catchAllEmail_Password : acc.MailPass);
                
                if (string.IsNullOrEmpty(passwordForVerify))
                {
                    acc.Status = "Lỗi: Không có password để kiểm tra email";
                    Log($"[{acc.Email}] >> Lỗi: Thiếu password");
                    return;
                }
                
                EmailHelper mailHelper = new EmailHelper(emailForVerify, passwordForVerify);
                Log($"[{acc.Email}] >> Kiểm tra email {(isGmailAccount ? "Gmail (riêng)" : "Catch-All")}...");
                
                string linkVerify = null;
                // Retry tìm mail 3 lần
                for(int k=0; k<3; k++) {
                    linkVerify = mailHelper.GetVerifyLink(acc.Email);
                    if(linkVerify != null) break;
                    Log($"...Tìm mail lần {k+1}...");
                    Thread.Sleep(10000);
                }

                if (linkVerify != null)
                {
                    Log($"[{acc.Email}] >> Đang kích hoạt: {linkVerify}");
                    driver = BrowserHelper.OpenBrowser(proxyConfig.Enabled ? proxyConfig : null);
                    driver.Navigate().GoToUrl(linkVerify);

                    // --- XỬ LÝ MODAL "COMPLETE PROFILE" ---
                    // Chờ modal hiện ra sau khi verify
                    Thread.Sleep(8000);
                    try 
                    {
                        // Tìm nút "Remind Me Later" (class: remind-link)
                        var reminds = driver.FindElements(By.CssSelector(".remind-link"));
                        if(reminds.Count > 0)
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", reminds[0]);
                            Log("Đã tắt Modal (Remind Later)");
                        }
                        else
                        {
                            // Hoặc tìm nút đóng X (class: modal-close)
                            var closes = driver.FindElements(By.ClassName("modal-close"));
                            if(closes.Count > 0) ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", closes[0]);
                        }
                    }
                    catch {} // Không quan trọng, tắt trình duyệt là xong

                    driver.Quit();

                    // BƯỚC 3: LẤY PASSWORD
                    Log($"[{acc.Email}] >> Đang đợi Mail chứa Pass...");
                    Thread.Sleep(15000); // Chờ mail credentials về
                    
                    // Nếu là Gmail account, MailPass đã có sẵn (nhập từ CSV)
                    // Nếu là custom domain, lấy password từ email hệ thống gửi
                    string newPass = isGmailAccount ? acc.MailPass : mailHelper.GetPasswordFromMail(acc.Email);
                    
                    if(!string.IsNullOrEmpty(newPass))
                    {
                        acc.GeneratedPassword = newPass; // Lưu pass vào CSV
                        acc.Status = "Thành công";
                        Log($"[{acc.Email}] >> DONE! Pass: {newPass}");
                    }
                    else
                    {
                        acc.Status = "Lỗi lấy Pass";
                        Log($"[{acc.Email}] >> Cảnh báo: Verify OK nhưng chưa thấy Pass.");
                    }
                }
                else 
                {
                    acc.Status = "Lỗi: Không có Mail Verify";
                }
            }
            catch (Exception ex)
            {
                acc.Status = "Lỗi: " + ex.Message;
                Log($"Error: {ex.Message}");
            }
            finally
            {
                if (driver != null) driver.Quit();
            }
        }

        private void Log(string msg)
        {
            if (rtbLog.InvokeRequired) rtbLog.Invoke(new Action<string>(Log), msg);
            else
            {
                rtbLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + msg + "\n");
                rtbLog.ScrollToCaret();
            }
        }

        private static void FillInputRealUser(IWebDriver driver, IWebElement element, string value)
        {
            try
            {
                // Scroll to element
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
                Thread.Sleep(800);
                
                // Wait for element to be displayed
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => element.Displayed);
                Thread.Sleep(500);
                
                // Remove overlays
                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    document.querySelectorAll('[class*=""loading""], [class*=""overlay""], [role=""dialog""]')
                        .forEach(el => el.style.display = 'none');
                ", element);
                
                Thread.Sleep(300);

                // Use Actions to click and type
                var actions = new OpenQA.Selenium.Interactions.Actions(driver);
                actions.Click(element).Perform();
                Thread.Sleep(600);
                
                // Clear field
                actions.KeyDown(OpenQA.Selenium.Keys.Control)
                       .SendKeys("a")
                       .KeyUp(OpenQA.Selenium.Keys.Control)
                       .Perform();
                Thread.Sleep(200);
                
                // Type value character by character
                actions.SendKeys(value).Perform();
                Thread.Sleep(500);
                
                // Trigger blur by pressing Tab
                actions.SendKeys(OpenQA.Selenium.Keys.Tab).Perform();
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                throw new Exception($"FillInputRealUser error: {ex.Message}", ex);
            }
        }

        private static void ScrollAndFill(IWebDriver driver, IWebElement element, string value)
        {
            // Luôn dùng JavaScript để đảm bảo trigger validation đầy đủ
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                const el = arguments[0];
                const val = arguments[1];
                
                // Scroll vào view
                el.scrollIntoView({block: 'center'});
                
                // Focus vào element
                el.focus();
                
                // Xóa readonly nếu có
                if (el.hasAttribute('readonly')) el.removeAttribute('readonly');
                
                // Set value
                el.value = val;
                
                // Trigger tất cả các events cần thiết cho validation
                el.dispatchEvent(new Event('focus', {bubbles: true}));
                el.dispatchEvent(new Event('input', {bubbles: true}));
                el.dispatchEvent(new Event('change', {bubbles: true}));
                el.dispatchEvent(new Event('blur', {bubbles: true}));
                
                // Đánh dấu element đã touched (cho các framework như Vue/React)
                el.setAttribute('data-filled', 'true');
            ", element, value);
            
            Thread.Sleep(300); // Chờ validation chạy
        }

        /// <summary>
        /// Lưu danh sách AccountData vào CSV với tất cả các giá trị được bao quanh bởi dấu ngoặc kép
        /// Điều này tránh nhầm lẫn khi password chứa dấu phẩy ở đầu hoặc cuối
        /// </summary>
        private void SaveCsvWithQuotes(string path, List<AccountData> accounts)
        {
            try
            {
                using (var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
                {
                    // Viết header
                    writer.WriteLine("\"Email\",\"MailPass\",\"FirstName\",\"LastName\",\"GeneratedPassword\",\"Status\"");

                    // Viết dữ liệu
                    foreach (var account in accounts)
                    {
                        var email = EscapeCsvValue(account.Email ?? "");
                        var mailPass = EscapeCsvValue(account.MailPass ?? "");
                        var firstName = EscapeCsvValue(account.FirstName ?? "");
                        var lastName = EscapeCsvValue(account.LastName ?? "");
                        var genPassword = EscapeCsvValue(account.GeneratedPassword ?? "");
                        var status = EscapeCsvValue(account.Status ?? "");

                        writer.WriteLine($"\"{email}\",\"{mailPass}\",\"{firstName}\",\"{lastName}\",\"{genPassword}\",\"{status}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Lỗi lưu CSV: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Escape giá trị CSV: thay dấu " thành ""
        /// </summary>
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            // Theo chuẩn CSV: dấu " được escape bằng ""
            return value.Replace("\"", "\"\"");
        }
    }
}