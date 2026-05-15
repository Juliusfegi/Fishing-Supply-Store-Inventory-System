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
    public partial class EditProductForm : Form
    {
        static readonly Color BgDeep = Color.FromArgb(10, 22, 40);
        static readonly Color BgMid = Color.FromArgb(13, 40, 71);
        static readonly Color CardBg = Color.FromArgb(19, 34, 54);
        static readonly Color Wave = Color.FromArgb(26, 127, 193);
        static readonly Color Foam = Color.FromArgb(41, 182, 246);
        static readonly Color Gold = Color.FromArgb(245, 158, 11);
        static readonly Color TextMain = Color.FromArgb(232, 244, 253);

        private static readonly HttpClient http = new HttpClient();

        private readonly string productId;
        private TextBox txtName, txtCategory, txtPrice, txtQuantity;
        private Button btnUpdate, btnCancel;


        public EditProductForm(string id, string name, string category, string price, string quantity)
        {
           
            InitializeComponent();
            productId = id;
            BuildUI();
            txtName.Text = name;
            txtCategory.Text = category;
            txtPrice.Text = price;
            txtQuantity.Text = quantity;
        }
        private void BuildUI()
        {
            Text = "Edit Product";
            Size = new Size(450, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = BgDeep;
            Font = new Font("Segoe UI", 9.5f);

            Controls.Add(new Label
            {
                Text = "✏ EDIT PRODUCT",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Gold,
                AutoSize = false,
                Size = new Size(410, 40),
                Location = new Point(20, 18),
                TextAlign = ContentAlignment.MiddleCenter,
            });

            var card = new Panel { BackColor = CardBg, Size = new Size(390, 256), Location = new Point(25, 76) };
            Controls.Add(card);

            card.Controls.Add(MakeLabel("Product Name", 18));
            txtName = MakeTB(18); card.Controls.Add(txtName);

            card.Controls.Add(MakeLabel("Category", 78));
            txtCategory = MakeTB(78); card.Controls.Add(txtCategory);

            card.Controls.Add(MakeLabel("Price (₱)", 138));
            txtPrice = MakeTB(138); card.Controls.Add(txtPrice);

            card.Controls.Add(MakeLabel("Quantity", 198));
            txtQuantity = MakeTB(198); card.Controls.Add(txtQuantity);

            btnUpdate = new Button
            {
                Text = "💾 Update",
                Size = new Size(150, 40),
                Location = new Point(60, 348),
                BackColor = Foam,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += async (s, e) => await UpdateProductAsync();
            Controls.Add(btnUpdate);

            btnCancel = new Button
            {
                Text = "❌ Cancel",
                Size = new Size(140, 40),
                Location = new Point(225, 348),
                BackColor = Wave,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);
        }

        private Label MakeLabel(string text, int y) => new Label
        {
            Text = text,
            ForeColor = TextMain,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Location = new Point(20, y),
            Size = new Size(140, 20),
        };

        private TextBox MakeTB(int y) => new TextBox
        {
            Size = new Size(350, 30),
            Location = new Point(20, y + 22),
            BackColor = BgMid,
            ForeColor = TextMain,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f),
        };

        private async Task UpdateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCategory.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) || string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Please fill all fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnUpdate.Enabled = false;
                btnUpdate.Text = "Updating…";

                var json = new JObject
                {
                    ["product_name"] = txtName.Text.Trim(),
                    ["category"] = txtCategory.Text.Trim(),
                    ["price"] = Convert.ToDouble(txtPrice.Text),
                    ["quantity"] = Convert.ToInt32(txtQuantity.Text),
                };

                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                var response = await http.PutAsync($"{Program.ApiBase}/inventory1/products/{productId}", content);
                var result = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(result);

                if (obj["success"]?.Value<bool>() == true)
                {
                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                    MessageBox.Show(obj["message"]?.ToString() ?? "Update failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpdate.Enabled = true;
                btnUpdate.Text = "💾 Update";
            }
        }
    }
}
