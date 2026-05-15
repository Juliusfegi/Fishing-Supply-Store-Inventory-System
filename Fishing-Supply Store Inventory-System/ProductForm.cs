using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fishing_Supply_Store_Inventory_System
{
    public partial class ProductForm : Form
    {
        static readonly Color BgDeep = Color.FromArgb(7, 25, 47);
        static readonly Color BgMid = Color.FromArgb(13, 40, 71);
        static readonly Color CardBg = Color.FromArgb(16, 42, 67);
        static readonly Color Blue = Color.FromArgb(15, 95, 159);
        static readonly Color Gold = Color.FromArgb(245, 158, 11);
        static readonly Color TextMain = Color.FromArgb(241, 245, 249);
        static readonly Color TextMute = Color.FromArgb(148, 163, 184);
        static readonly Color Success = Color.FromArgb(34, 197, 94);
        static readonly Color Danger = Color.FromArgb(239, 68, 68);

        private static readonly HttpClient http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private TextBox txtName;
        private TextBox txtCategory;
        private TextBox txtPrice;
        private TextBox txtQuantity;

        private Button btnAdd;
        private Button btnCancel;
        private Label lblStatus;
        public ProductForm()
        {
            Text = "Add Product - Fegi;s Fishing Gear";
            Size = new Size(460, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = BgDeep;
            Font = new Font("Segoe UI", 9f);

            BuildUI();
            InitializeComponent();
        }
        private void BuildUI()
        {
            Controls.Clear();

            Label title = new Label
            {
                Text = "ADD NEW PRODUCT",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Gold,
                AutoSize = false,
                Size = new Size(420, 35),
                Location = new Point(20, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(title);

            Label subtitle = new Label
            {
                Text = "Create a new fishing gear item for your inventory",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextMute,
                AutoSize = false,
                Size = new Size(420, 20),
                Location = new Point(20, 58),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(subtitle);

            Panel card = new Panel
            {
                BackColor = CardBg,
                Size = new Size(395, 260),
                Location = new Point(32, 95)
            };
            Controls.Add(card);

            card.Controls.Add(MakeLabel("Product Name", 18));
            txtName = MakeTextBox(18);
            
            card.Controls.Add(txtName);

            card.Controls.Add(MakeLabel("Category", 78));
            txtCategory = MakeTextBox(78);
            card.Controls.Add(txtCategory);

            card.Controls.Add(MakeLabel("Price", 138));
            txtPrice = MakeTextBox(138);
            card.Controls.Add(txtPrice);

            card.Controls.Add(MakeLabel("Quantity", 198));
            txtQuantity = MakeTextBox(198);
            card.Controls.Add(txtQuantity);

            btnAdd = new Button
            {
                Text = "Add Product",
                Size = new Size(155, 38),
                Location = new Point(70, 372),
                BackColor = Success,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += async (s, e) => await AddProductAsync();
            Controls.Add(btnAdd);

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(145, 38),
                Location = new Point(240, 372),
                BackColor = Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);

            lblStatus = new Label
            {
                Text = "",
                ForeColor = TextMute,
                Font = new Font("Segoe UI", 8f),
                Location = new Point(32, 418),
                Size = new Size(395, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblStatus);
        }

        private Label MakeLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                ForeColor = TextMute,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(160, 18)
            };
        }

        private TextBox MakeTextBox(int y)
        {
            return new TextBox
            {
                Size = new Size(350, 30),
                Location = new Point(20, y + 22),
                BackColor = BgMid,
                ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
        }

        private async Task AddProductAsync()
        {
            string name = txtName.Text.Trim();
            string category = txtCategory.Text.Trim();
            string priceText = txtPrice.Text.Trim();
            string qtyText = txtQuantity.Text.Trim();

            if (name == "" || category == "" || priceText == "" || qtyText == "")
            {
                ShowStatus("Please fill in all fields.", Danger);
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price < 0)
            {
                ShowStatus("Enter a valid product price.", Danger);
                return;
            }

            if (!int.TryParse(qtyText, out int quantity) || quantity < 0)
            {
                ShowStatus("Enter a valid product quantity.", Danger);
                return;
            }

            try
            {
                btnAdd.Enabled = false;
                btnAdd.Text = "Saving...";
                ShowStatus("Adding product...", Gold);

                JObject data = new JObject
                {
                    ["product_name"] = name,
                    ["category"] = category,
                    ["price"] = price,
                    ["quantity"] = quantity
                };

                StringContent content = new StringContent(
                    data.ToString(),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response =
                    await http.PostAsync($"{Program.ApiBase}/products", content);

                string result = await response.Content.ReadAsStringAsync();

                if (result.TrimStart().StartsWith("<"))
                {
                    ShowStatus("API returned HTML. Check server endpoint.", Danger);
                    MessageBox.Show(
                        "The API returned HTML instead of JSON.\n\nCheck that Node.js is running and endpoint /products exists.",
                        "API Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                JObject obj = JObject.Parse(result);

                if (obj["success"]?.Value<bool>() == true)
                {
                    MessageBox.Show(
                        "Product added successfully.",
                        "Fishing Gear sa Fegi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    ShowStatus(obj["message"]?.ToString() ?? "Failed to add product.", Danger);
                }
            }
            catch (TaskCanceledException)
            {
                ShowStatus("Connection timeout. Check API server.", Danger);
            }
            catch (HttpRequestException)
            {
                ShowStatus("Cannot connect to API server.", Danger);
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, Danger);
            }
            finally
            {
                btnAdd.Enabled = true;
                btnAdd.Text = "Add Product";
            }
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.ForeColor = color;
            lblStatus.Text = message;
        }
    }
}
