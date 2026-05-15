using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fishing_Supply_Store_Inventory_System
{
    public partial class MainForm : Form
    {
        static readonly Color BgDeep = Color.FromArgb(7, 25, 47);
        static readonly Color BgMid = Color.FromArgb(13, 40, 71);
        static readonly Color BgLight = Color.FromArgb(16, 42, 67);
        static readonly Color Blue = Color.FromArgb(15, 95, 159);
        static readonly Color Sky = Color.FromArgb(56, 189, 248);
        static readonly Color Gold = Color.FromArgb(245, 158, 11);
        static readonly Color Green = Color.FromArgb(34, 197, 94);
        static readonly Color Red = Color.FromArgb(239, 68, 68);
        static readonly Color White = Color.FromArgb(241, 245, 249);
        static readonly Color Muted = Color.FromArgb(148, 163, 184);
        static readonly Color Border = Color.FromArgb(39, 76, 119);

        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        private Panel sidebar, contentPanel, topBar;
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cboCategory, cboProducts, cboStockOutProduct, cboReason;
        private Label lblHeader, lblSub, lblTotalProducts, lblTotalValue, lblLowStock, lblTotalOrders, lblStatus;
        private JArray allProducts = new JArray();
        private JArray allOrders = new JArray();

        public MainForm()
        {
            InitializeComponent();
            BuildUI();
            _ = LoadProductsAsync();
            _ = LoadCategoriesAsync();
        }
        private void BuildUI()
        {
            Text = "Fishing Supply Store";
            Size = new Size(1280, 760);
            MinimumSize = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            BackColor = BgDeep;
            Font = new Font("Segoe UI", 9f);

            sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = Color.FromArgb(5, 20, 38) };
            Controls.Add(sidebar);
            BuildSidebar();

            topBar = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = BgMid };
            Controls.Add(topBar);

            lblHeader = new Label
            {
                Text = "Dashboard",
                ForeColor = White,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                Location = new Point(260, 10),
                Size = new Size(500, 25)
            };
            topBar.Controls.Add(lblHeader);

            lblSub = new Label
            {
                Text = "Add Product → Stock In → Inventory → Customer Order → Stock Out → Reports",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(262, 36),
                Size = new Size(720, 18)
            };
            topBar.Controls.Add(lblSub);

            Label lblUser = new Label
            {
                Text = $"👤 {Program.CurrentUser}  |  {Program.CurrentRole.ToUpper()}",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5f),
                Size = new Size(280, 62),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            topBar.Resize += (s, e) => lblUser.Location = new Point(topBar.Width - 295, 0);
            topBar.Controls.Add(lblUser);

            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = BgDeep, Padding = new Padding(18) };
            Controls.Add(contentPanel);
            contentPanel.BringToFront();

            ShowDashboard();
        }

        private void BuildSidebar()
        {
            sidebar.Controls.Clear();

            sidebar.Controls.Add(new Label
            {
                Text = "🎣",
                ForeColor = Gold,
                Font = new Font("Segoe UI Emoji", 32f),
                Location = new Point(0, 22),
                Size = new Size(240, 48),
                TextAlign = ContentAlignment.MiddleCenter
            });

            sidebar.Controls.Add(new Label
            {
                Text = "Fishing Supply Inventory",
                ForeColor = Gold,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(0, 75),
                Size = new Size(240, 55),
                TextAlign = ContentAlignment.MiddleCenter
            });

            sidebar.Controls.Add(new Label
            {
                Text = "Inventory & Order System",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8f),
                Location = new Point(0, 132),
                Size = new Size(240, 20),
                TextAlign = ContentAlignment.MiddleCenter
            });

            sidebar.Controls.Add(new Panel
            {
                BackColor = Border,
                Location = new Point(24, 165),
                Size = new Size(192, 1)
            });

            AddSidebarButton("🏠  Dashboard", 190, Blue, ShowDashboard);
            AddSidebarButton("➕  Add Products", 235, BgLight, OpenAddForm);
            AddSidebarButton("📥  Stock In", 280, BgLight, ShowStockIn);
            AddSidebarButton("📦  Inventory", 325, BgLight, ShowProducts);
            AddSidebarButton("🛒  Customer Buys", 370, BgLight, ShowOrders);
            AddSidebarButton("📤  Stock Out", 415, BgLight, ShowStockOut);
            AddSidebarButton("📊  Reports", 460, BgLight, ShowReports);

            Button btnLogout = SideButton("🚪  Logout", 0, Red);
            btnLogout.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnLogout.Location = new Point(24, sidebar.Height - 58);
            btnLogout.Click += (s, e) =>
            {
                new Form1().Show();
                Close();
            };
            sidebar.Resize += (s, e) => btnLogout.Location = new Point(24, sidebar.Height - 58);
            sidebar.Controls.Add(btnLogout);
        }

        private void AddSidebarButton(string text, int y, Color bg, Action action)
        {
            Button btn = SideButton(text, y, bg);
            btn.Click += (s, e) => action();
            sidebar.Controls.Add(btn);
        }

        private Button SideButton(string text, int y, Color bg)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(24, y),
                Size = new Size(192, 38),
                BackColor = bg,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SetPage(string title, string subtitle)
        {
            lblHeader.Text = title;
            lblSub.Text = subtitle;
            contentPanel.Controls.Clear();
        }

        private void ShowDashboard()
        {
            SetPage("Dashboard", "System flow: Add Product → Stock In → Inventory Storage → Customer Order → Stock Out → Reports");

            Panel stats = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = BgDeep };
            contentPanel.Controls.Add(stats);

            lblTotalProducts = MakeCard(stats, "Total Products", allProducts.Count.ToString(), 0, Sky);
            lblTotalValue = MakeCard(stats, "Inventory Value", "₱0.00", 1, Gold);
            lblLowStock = MakeCard(stats, "Low Stock", "0", 2, Red);
            lblTotalOrders = MakeCard(stats, "Customer Orders", allOrders.Count.ToString(), 3, Green);

            Panel flow = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = BgDeep };
            contentPanel.Controls.Add(flow);
            flow.BringToFront();

            string[] steps = { "Add Product", "Stock In", "Inventory", "Customer Order", "Stock Out", "Reports" };
            Color[] colors = { Green, Sky, Gold, Blue, Red, Green };

            for (int i = 0; i < steps.Length; i++)
            {
                Label box = new Label
                {
                    Text = steps[i],
                    BackColor = BgLight,
                    ForeColor = colors[i],
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(i * 145, 35),
                    Size = new Size(120, 42)
                };
                flow.Controls.Add(box);

                if (i < steps.Length - 1)
                {
                    Label arrow = new Label
                    {
                        Text = "→",
                        ForeColor = Muted,
                        Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(i * 145 + 122, 38),
                        Size = new Size(24, 35)
                    };
                    flow.Controls.Add(arrow);
                }
            }

            ShowProducts();
        }

        private void ShowProducts()
        {
            SetPage(
                "Inventory / Track Stock",
                "Monitor fishing products, current stocks, low stock alerts, and product value"
            );

            Panel stats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = BgDeep
            };
            contentPanel.Controls.Add(stats);

            MakeCard(stats, "Total Items", allProducts.Count.ToString(), 0, Sky);
            MakeCard(stats, "Low Stock", CountLowStock(allProducts).ToString(), 1, Red);
            MakeCard(stats, "Inventory Value", "₱" + InventoryValue().ToString("N2"), 2, Gold);
            MakeCard(stats, "Storage Status", "Active", 3, Green);

            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = BgDeep
            };
            contentPanel.Controls.Add(toolbar);
            toolbar.BringToFront();

            toolbar.Controls.Add(new Label
            {
                Text = "Search Product",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Location = new Point(0, 5),
                Size = new Size(120, 18)
            });

            txtSearch = new TextBox
            {
                Location = new Point(0, 28),
                Size = new Size(260, 28),
                BackColor = BgMid,
                ForeColor = White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8.5f)
            };
            txtSearch.TextChanged += (s, e) => FilterGrid();
            toolbar.Controls.Add(txtSearch);

            toolbar.Controls.Add(new Label
            {
                Text = "Category",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Location = new Point(275, 5),
                Size = new Size(120, 18)
            });

            cboCategory = new ComboBox
            {
                Location = new Point(275, 28),
                Size = new Size(175, 28),
                BackColor = BgMid,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5f)
            };
            toolbar.Controls.Add(cboCategory);
            FillCategoryCombo();

            Button btnAdd = TopButton("Add Product", 470, 27, Green, Color.Black);
            btnAdd.Size = new Size(120, 30);
            btnAdd.Click += (s, e) => OpenAddForm();
            toolbar.Controls.Add(btnAdd);

            Button btnStockIn = TopButton("Stock In", 600, 27, Sky, Color.Black);
            btnStockIn.Size = new Size(100, 30);
            btnStockIn.Click += (s, e) => ShowStockIn();
            toolbar.Controls.Add(btnStockIn);

            Button btnStockOut = TopButton("Stock Out", 710, 27, Red, White);
            btnStockOut.Size = new Size(105, 30);
            btnStockOut.Click += (s, e) => ShowStockOut();
            toolbar.Controls.Add(btnStockOut);

            Button btnRefresh = TopButton("Refresh", 825, 27, Blue, White);
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.Click += (s, e) => _ = LoadProductsAsync();
            toolbar.Controls.Add(btnRefresh);

            dgv = MakeGrid();
            dgv.CellClick += OnGridCellClick;
            contentPanel.Controls.Add(dgv);
            dgv.BringToFront();

            AddStatusBar("Inventory storage loaded. Green = normal, yellow = low stock, red = out of stock.");
            RenderGrid(allProducts);
        }
        private void ShowStockIn()
        {
            SetPage("Stock In", "Add incoming stock quantity to existing products");

            Panel card = MakeFormCard();
            contentPanel.Controls.Add(card);

            card.Controls.Add(MakeFormLabel("Select Product", 25));
            cboProducts = MakeProductCombo(25);
            card.Controls.Add(cboProducts);

            TextBox txtQty = MakeTextBox(95);
            card.Controls.Add(MakeFormLabel("Stock In Quantity", 95));
            card.Controls.Add(txtQty);

            Button btnSave = TopButton("Save Stock In", 20, 170, Green, Color.Black);
            btnSave.Size = new Size(150, 34);
            btnSave.Click += async (s, e) =>
            {
                if (cboProducts.SelectedValue == null) return;
                if (!int.TryParse(txtQty.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("Enter valid stock quantity.");
                    return;
                }

                await StockInAsync(cboProducts.SelectedValue.ToString(), qty);
                txtQty.Clear();
            };
            card.Controls.Add(btnSave);
        }

        private void ShowStockOut()
        {
            SetPage("Stock Out", "Deduct stock due to customer order, damaged items, or returned items");

            Panel card = MakeFormCard();
            contentPanel.Controls.Add(card);

            card.Controls.Add(MakeFormLabel("Select Product", 25));
            cboStockOutProduct = MakeProductCombo(25);
            card.Controls.Add(cboStockOutProduct);

            TextBox txtQty = MakeTextBox(95);
            card.Controls.Add(MakeFormLabel("Stock Out Quantity", 95));
            card.Controls.Add(txtQty);

            card.Controls.Add(MakeFormLabel("Reason", 165));
            cboReason = new ComboBox
            {
                Location = new Point(20, 188),
                Size = new Size(330, 30),
                BackColor = BgMid,
                ForeColor = White,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cboReason.Items.AddRange(new string[] { "Customer Order", "Damaged", "Returned" });
            cboReason.SelectedIndex = 0;
            card.Controls.Add(cboReason);

            Button btnSave = TopButton("Save Stock Out", 20, 245, Red, White);
            btnSave.Size = new Size(150, 34);
            btnSave.Click += async (s, e) =>
            {
                if (cboStockOutProduct.SelectedValue == null) return;
                if (!int.TryParse(txtQty.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("Enter valid stock quantity.");
                    return;
                }

                await StockOutAsync(cboStockOutProduct.SelectedValue.ToString(), qty, cboReason.Text);
                txtQty.Clear();
            };
            card.Controls.Add(btnSave);
        }

        private void ShowOrders()
        {
            SetPage("Customer Orders", "View online shop orders and current order statuses");

            dgv = MakeGrid();
            contentPanel.Controls.Add(dgv);
            dgv.BringToFront();

            AddStatusBar("Customer orders loaded.");
            RenderOrdersGrid(allOrders);
            _ = LoadOrdersAsync();
        }

        private void ShowChecklist()
        {
            SetPage("Delivery Checklist", "Admin delivery preparation checklist for placed orders");

            dgv = MakeGrid();
            contentPanel.Controls.Add(dgv);
            dgv.BringToFront();

            AddStatusBar("Delivery checklist loaded.");
            _ = LoadChecklistAsync();
        }

        private void ShowReports()
        {
            SetPage("Reports", "Inventory report, low stock report, stock in/out report, and sales report");

            Panel stats = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = BgDeep };
            contentPanel.Controls.Add(stats);

            int low = CountLowStock(allProducts);
            double value = InventoryValue();

            MakeCard(stats, "Inventory Report", allProducts.Count + " items", 0, Sky);
            MakeCard(stats, "Low Stock Report", low + " items", 1, Red);
            MakeCard(stats, "Sales / Orders", allOrders.Count + " orders", 2, Green);
            MakeCard(stats, "Stock Value", "₱" + value.ToString("N2"), 3, Gold);

            Label note = new Label
            {
                Text = "Printable reports can be added here using PrintDocument or export to Excel/PDF.",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 10f),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleLeft
            };
            contentPanel.Controls.Add(note);
            note.BringToFront();

            dgv = MakeGrid();
            contentPanel.Controls.Add(dgv);
            dgv.BringToFront();

            RenderGrid(allProducts);
            AddStatusBar("Reports ready.");
        }

        private Panel MakeFormCard()
        {
            return new Panel
            {
                Width = 390,
                Height = 320,
                Location = new Point(0, 20),
                BackColor = BgLight
            };
        }

        private Label MakeFormLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(20, y),
                Size = new Size(200, 20),
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
        }

        private TextBox MakeTextBox(int y)
        {
            return new TextBox
            {
                Location = new Point(20, y + 23),
                Size = new Size(330, 30),
                BackColor = BgMid,
                ForeColor = White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
        }

        private ComboBox MakeProductCombo(int y)
        {
            ComboBox cbo = new ComboBox
            {
                Location = new Point(20, y + 23),
                Size = new Size(330, 30),
                BackColor = BgMid,
                ForeColor = White,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };

            DataTable dt = new DataTable();
            dt.Columns.Add("id");
            dt.Columns.Add("name");

            foreach (JObject p in allProducts)
                dt.Rows.Add(p["product_id"]?.ToString(), p["product_name"]?.ToString());

            cbo.DataSource = dt;
            cbo.DisplayMember = "name";
            cbo.ValueMember = "id";

            return cbo;
        }

        private DataGridView MakeGrid()
        {
            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = BgLight,
                BorderStyle = BorderStyle.None,
                GridColor = Border,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                Font = new Font("Segoe UI", 8.5f),
                EnableHeadersVisualStyles = false
            };

            grid.DefaultCellStyle.BackColor = BgLight;
            grid.DefaultCellStyle.ForeColor = White;
            grid.DefaultCellStyle.SelectionBackColor = Blue;
            grid.DefaultCellStyle.SelectionForeColor = White;
            grid.DefaultCellStyle.Padding = new Padding(4);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(12, 34, 58);
            grid.ColumnHeadersDefaultCellStyle.BackColor = BgMid;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Sky;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            grid.ColumnHeadersHeight = 34;
            grid.RowTemplate.Height = 30;

            return grid;
        }

        private Button TopButton(string text, int x, int y, Color bg, Color fg)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(110, 29),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Label MakeCard(Panel parent, string title, string value, int index, Color accent)
        {
            Panel card = new Panel
            {
                Size = new Size(220, 78),
                Location = new Point(index * 235, 10),
                BackColor = BgLight
            };

            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(accent, 4))
                    e.Graphics.DrawLine(pen, 0, 0, 0, card.Height);
            };

            Label val = new Label
            {
                Text = value,
                ForeColor = accent,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Location = new Point(16, 12),
                Size = new Size(190, 28)
            };
            card.Controls.Add(val);

            card.Controls.Add(new Label
            {
                Text = title,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8f),
                Location = new Point(16, 45),
                Size = new Size(190, 20)
            });

            parent.Controls.Add(card);
            return val;
        }

        private void FillCategoryCombo()
        {
            if (cboCategory == null) return;

            cboCategory.Items.Clear();
            cboCategory.Items.Add("All Categories");

            foreach (JObject p in allProducts)
            {
                string cat = p["category"]?.ToString();
                if (!string.IsNullOrWhiteSpace(cat) && !cboCategory.Items.Contains(cat))
                    cboCategory.Items.Add(cat);
            }

            cboCategory.SelectedIndex = 0;
        }

        private async Task LoadProductsAsync()
        {
            SetStatus("Loading Fishing Gear inventory...");

            try
            {
                HttpResponseMessage resp = await http.GetAsync($"{Program.ApiBase}/products");
                string json = await resp.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("<"))
                {
                    SetStatus("API returned HTML. Check Program.ApiBase or Node server.");
                    MessageBox.Show("API returned HTML instead of JSON.\nCheck if Node.js is running at http://localhost:3000.", "API Error");
                    return;
                }

                JObject obj = JObject.Parse(json);

                if (obj["success"]?.Value<bool>() == true)
                {
                    allProducts = obj["data"] as JArray ?? new JArray();

                    Invoke(new Action(() =>
                    {
                        if (lblTotalProducts != null) lblTotalProducts.Text = allProducts.Count.ToString();
                        if (lblTotalValue != null) lblTotalValue.Text = "₱" + InventoryValue().ToString("N2");
                        if (lblLowStock != null) lblLowStock.Text = CountLowStock(allProducts).ToString();

                        FillCategoryCombo();
                        RenderGrid(allProducts);
                        SetStatus($"{allProducts.Count} product(s) loaded • {DateTime.Now:hh:mm tt}");
                    }));
                }
                else SetStatus("Error: " + obj["message"]?.ToString());
            }
            catch (Exception ex)
            {
                SetStatus("API error: " + ex.Message);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                HttpResponseMessage resp = await http.GetAsync($"{Program.ApiBase}/categories");
                string json = await resp.Content.ReadAsStringAsync();
                if (json.TrimStart().StartsWith("<")) return;
            }
            catch { }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                HttpResponseMessage resp = await http.GetAsync($"{Program.ApiBase}/orders");
                string json = await resp.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("<")) return;

                JObject obj = JObject.Parse(json);

                if (obj["success"]?.Value<bool>() == true)
                {
                    allOrders = obj["data"] as JArray ?? new JArray();

                    if (lblTotalOrders != null)
                        lblTotalOrders.Text = allOrders.Count.ToString();
                }
            }
            catch { }
        }

        private async Task LoadChecklistAsync()
        {
            try
            {
                HttpResponseMessage resp = await http.GetAsync($"{Program.ApiBase}/checklist");
                string json = await resp.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("<")) return;

                JObject obj = JObject.Parse(json);

                if (obj["success"]?.Value<bool>() == true)
                {
                    JArray rows = obj["data"] as JArray ?? new JArray();
                    DataTable dt = new DataTable();

                    dt.Columns.Add("Tracking ID");
                    dt.Columns.Add("Customer");
                    dt.Columns.Add("Status");
                    dt.Columns.Add("Packed");
                    dt.Columns.Add("Checked");
                    dt.Columns.Add("Out For Delivery");
                    dt.Columns.Add("Delivered");

                    foreach (JObject r in rows)
                    {
                        dt.Rows.Add(
                            r["tracking_id"]?.ToString(),
                            r["customer_name"]?.ToString(),
                            r["order_status"]?.ToString(),
                            r["packed"]?.ToString(),
                            r["checked"]?.ToString(),
                            r["out_for_delivery"]?.ToString(),
                            r["delivered"]?.ToString()
                        );
                    }

                    if (dgv != null) dgv.DataSource = dt;
                }
            }
            catch { }
        }

        private void RenderOrdersGrid(JArray orders)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Tracking ID");
            dt.Columns.Add("Customer");
            dt.Columns.Add("Contact");
            dt.Columns.Add("Total");
            dt.Columns.Add("Status");
            dt.Columns.Add("Payment");

            foreach (JObject o in orders)
            {
                double total = o["total_price"]?.Value<double>() ?? 0;

                dt.Rows.Add(
                    o["tracking_id"]?.ToString(),
                    o["customer_name"]?.ToString(),
                    o["customer_contact"]?.ToString(),
                    "₱" + total.ToString("N2"),
                    o["order_status"]?.ToString(),
                    o["payment_status"]?.ToString()
                );
            }

            if (dgv != null) dgv.DataSource = dt;
        }

        private async Task StockInAsync(string productId, int qty)
        {
            try
            {
                JObject data = new JObject { ["quantity"] = qty };
                StringContent content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage resp = await http.PostAsync($"{Program.ApiBase}/stock-in/{productId}", content);
                string json = await resp.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("<"))
                {
                    MessageBox.Show("API returned HTML. Add /stock-in route in server.js.");
                    return;
                }

                JObject obj = JObject.Parse(json);

                if (obj["success"]?.Value<bool>() == true)
                {
                    MessageBox.Show("Stock In saved successfully.");
                    await LoadProductsAsync();
                }
                else MessageBox.Show(obj["message"]?.ToString() ?? "Stock In failed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "API Error");
            }
        }

        private async Task StockOutAsync(string productId, int qty, string reason)
        {
            try
            {
                JObject data = new JObject
                {
                    ["quantity"] = qty,
                    ["reason"] = reason
                };

                StringContent content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage resp = await http.PostAsync($"{Program.ApiBase}/stock-out/{productId}", content);
                string json = await resp.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("<"))
                {
                    MessageBox.Show("API returned HTML. Add /stock-out route in server.js.");
                    return;
                }

                JObject obj = JObject.Parse(json);

                if (obj["success"]?.Value<bool>() == true)
                {
                    MessageBox.Show("Stock Out saved successfully.");
                    await LoadProductsAsync();
                }
                else MessageBox.Show(obj["message"]?.ToString() ?? "Stock Out failed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "API Error");
            }
        }

        private double InventoryValue()
        {
            double value = 0;

            foreach (JObject p in allProducts)
            {
                double price = p["price"]?.Value<double>() ?? 0;
                int qty = p["quantity"]?.Value<int>() ?? 0;
                value += price * qty;
            }

            return value;
        }

        private int CountLowStock(JArray products)
        {
            int count = 0;

            foreach (JObject p in products)
            {
                int qty = p["quantity"]?.Value<int>() ?? 0;
                if (qty <= 5) count++;
            }

            return count;
        }

        private void FilterGrid()
        {
            if (txtSearch == null || cboCategory == null) return;

            string search = txtSearch.Text.Trim().ToLower();
            string category = cboCategory.SelectedItem?.ToString() ?? "All Categories";

            JArray filtered = new JArray();

            foreach (JObject p in allProducts)
            {
                string name = p["product_name"]?.ToString().ToLower() ?? "";
                string cat = p["category"]?.ToString() ?? "";

                if (search.Length > 0 && !name.Contains(search)) continue;
                if (category != "All Categories" && cat != category) continue;

                filtered.Add(p);
            }

            RenderGrid(filtered);
        }

        private void RenderGrid(JArray products)
        {
            if (dgv == null) return;

            DataTable dt = new DataTable();

            dt.Columns.Add("ID");
            dt.Columns.Add("Product Name");
            dt.Columns.Add("Category");
            dt.Columns.Add("Price");
            dt.Columns.Add("Quantity");
            dt.Columns.Add("Stock Value");
            dt.Columns.Add("Edit");
            dt.Columns.Add("Delete");

            foreach (JObject p in products)
            {
                double price = p["price"]?.Value<double>() ?? 0;
                int qty = p["quantity"]?.Value<int>() ?? 0;

                dt.Rows.Add(
                    p["product_id"]?.ToString(),
                    p["product_name"]?.ToString(),
                    p["category"]?.ToString(),
                    "₱" + price.ToString("N2"),
                    qty.ToString(),
                    "₱" + (price * qty).ToString("N2"),
                    "Edit",
                    "Delete"
                );
            }

            dgv.DataSource = dt;

            if (dgv.Columns["ID"] != null) dgv.Columns["ID"].Width = 55;
            if (dgv.Columns["Edit"] != null) dgv.Columns["Edit"].Width = 70;
            if (dgv.Columns["Delete"] != null) dgv.Columns["Delete"].Width = 75;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (dgv.Columns.Contains("Quantity"))
                {
                    int.TryParse(row.Cells["Quantity"].Value?.ToString(), out int qty);

                    if (qty <= 0)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(70, 70, 25, 35);
                    else if (qty <= 5)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(65, 75, 55, 15);
                }

                if (dgv.Columns.Contains("Edit"))
                {
                    row.Cells["Edit"].Style.ForeColor = Sky;
                    row.Cells["Edit"].Style.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
                }

                if (dgv.Columns.Contains("Delete"))
                {
                    row.Cells["Delete"].Style.ForeColor = Red;
                    row.Cells["Delete"].Style.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
                }
            }
        }

        private void OnGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgv.Columns[e.ColumnIndex].Name;

            if (!dgv.Columns.Contains("ID")) return;

            string id = dgv.Rows[e.RowIndex].Cells["ID"].Value?.ToString();
            string name = dgv.Columns.Contains("Product Name")
                ? dgv.Rows[e.RowIndex].Cells["Product Name"].Value?.ToString()
                : "";
            string cat = dgv.Columns.Contains("Category")
                ? dgv.Rows[e.RowIndex].Cells["Category"].Value?.ToString()
                : "";
            string price = dgv.Columns.Contains("Price")
                ? dgv.Rows[e.RowIndex].Cells["Price"].Value?.ToString().Replace("₱", "").Replace(",", "")
                : "0";
            string qty = dgv.Columns.Contains("Quantity")
                ? dgv.Rows[e.RowIndex].Cells["Quantity"].Value?.ToString()
                : "0";

            if (colName == "Edit")
            {
                EditProductForm frm = new EditProductForm(id, name, cat, price, qty);
                frm.FormClosed += (s, ev) => _ = LoadProductsAsync();
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show($"Delete \"{name}\"?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _ = DeleteProductAsync(id);
                }
            }
        }

        private async Task DeleteProductAsync(string id)
        {
            try
            {
                HttpResponseMessage resp = await http.DeleteAsync($"{Program.ApiBase}/products/{id}");
                string json = await resp.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("<"))
                {
                    MessageBox.Show("API returned HTML instead of JSON.", "API Error");
                    return;
                }

                JObject obj = JObject.Parse(json);

                if (obj["success"]?.Value<bool>() == true)
                {
                    MessageBox.Show("Product deleted successfully.", "Fishing Gear sa Fegi");
                    await LoadProductsAsync();
                }
                else
                {
                    MessageBox.Show(obj["message"]?.ToString() ?? "Delete failed.", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "API Error");
            }
        }

        private void OpenAddForm()
        {
            ProductForm frm = new ProductForm();
            frm.FormClosed += (s, e) => _ = LoadProductsAsync();
            frm.ShowDialog();
        }

        private void AddStatusBar(string text)
        {
            Panel statusBar = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = BgMid };
            contentPanel.Controls.Add(statusBar);
            statusBar.BringToFront();

            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            statusBar.Controls.Add(lblStatus);
        }

        private void SetStatus(string msg)
        {
            if (lblStatus == null) return;

            if (lblStatus.InvokeRequired)
                Invoke(new Action(() => lblStatus.Text = msg));
            else
                lblStatus.Text = msg;
        }
    }
}