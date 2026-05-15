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
using System.Xml.Linq;
using Newtonsoft.Json.Linq; // Para sa JObject ug JArray


namespace Fishing_Supply_Store_Inventory_System
{
    public partial class Form1 : Form
    {
        // THEME COLORS
        static readonly Color BgDeep = Color.FromArgb(7, 25, 47);
        static readonly Color BgCard = Color.FromArgb(16, 42, 67);
        static readonly Color Wave = Color.FromArgb(15, 95, 159);
        static readonly Color Gold = Color.FromArgb(245, 158, 11);
        static readonly Color TextMain = Color.FromArgb(241, 245, 249);
        static readonly Color TextMute = Color.FromArgb(148, 163, 184);
        static readonly Color Danger = Color.FromArgb(239, 68, 68);

        private TextBox txtUser, txtPass, txtApi;
        private Button btnLogin;
        private Label lblStatus;

        private static readonly HttpClient http =
            new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public Form1()
        {
            Text = "Fishing Supply Store — Inventory System";
            Size = new Size(430, 610);

            StartPosition = FormStartPosition.CenterScreen;

            FormBorderStyle = FormBorderStyle.FixedSingle;

            MaximizeBox = false;

            BackColor = BgDeep;

            Font = new Font("Segoe UI", 9.5f);

            BuildUI();
        }
        private void BuildUI()
        {
            // LOGO
            Label logo = new Label()
            {
                Text = "🎣",
                AutoSize = false,
                Size = new Size(400, 70),
                Location = new Point(10, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 34f),
                ForeColor = Gold
            };

            Controls.Add(logo);

            // TITLE
            Label lblTitle = new Label()
            {
                Text = "FISHING SUPPLY STORE",
                AutoSize = false,
                Size = new Size(400, 35),
                Location = new Point(10, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Gold
            };

            Controls.Add(lblTitle);

            // SUBTITLE
            Label lblSub = new Label()
            {
                Text = "Inventory Management System",
                AutoSize = false,
                Size = new Size(400, 25),
                Location = new Point(10, 136),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextMute
            };

            Controls.Add(lblSub);

            // CARD PANEL
            Panel card = new Panel()
            {
                Size = new Size(350, 400),
                Location = new Point(35, 185),
                BackColor = BgCard
            };

            Controls.Add(card);

            // LOGIN TITLE
            Label lblLogin = new Label()
            {
                Text = "Admin Login",
                AutoSize = false,
                Size = new Size(300, 30),
                Location = new Point(25, 20),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = TextMain
            };

            card.Controls.Add(lblLogin);

            // DEFAULT INFO
            Label lblDefault = new Label()
            {
                Text = "Default: admin / admin123",
                AutoSize = false,
                Size = new Size(300, 20),
                Location = new Point(25, 55),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextMute
            };

            card.Controls.Add(lblDefault);

            // USERNAME LABEL
            Label lblUser = MakeLabel("Username", 90);
            card.Controls.Add(lblUser);

            // USERNAME TEXTBOX
            txtUser = MakeTextBox(115);
            txtUser.Text = "admin";
            card.Controls.Add(txtUser);

            // PASSWORD LABEL
            Label lblPass = MakeLabel("Password", 160);
            card.Controls.Add(lblPass);

            // PASSWORD TEXTBOX
            txtPass = MakeTextBox(185);
            txtPass.PasswordChar = '●';

            txtPass.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    _ = DoLoginAsync();
                }
            };

            card.Controls.Add(txtPass);

            // API LABEL
            Label lblApi = MakeLabel("API Server URL", 230);
            card.Controls.Add(lblApi);

            // API TEXTBOX
            txtApi = MakeTextBox(255);
            txtApi.Text = Program.ApiBase;
            txtApi.TextChanged += (_, __) =>
            {
                Program.ApiBase = txtApi.Text.TrimEnd('/');
            };

            card.Controls.Add(txtApi);

            // LOGIN BUTTON
            btnLogin = new Button()
            {
                Text = "LOGIN",
                Size = new Size(300, 42),
                Location = new Point(25, 305),
                BackColor = Gold,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnLogin.FlatAppearance.BorderSize = 0;

            btnLogin.Click += (_, __) => _ = DoLoginAsync();

            card.Controls.Add(btnLogin);

            // STATUS LABEL
            lblStatus = new Label()
            {
                Text = "",
                AutoSize = false,
                Size = new Size(300, 22),
                Location = new Point(25, 370),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Danger
            };

            card.Controls.Add(lblStatus);

            // FOOTER
            Label lblFooter = new Label()
            {
                Text = "Fishing Supply Store Inventory System",
                AutoSize = false,
                Size = new Size(400, 22),
                Location = new Point(10, 535),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextMute
            };

            Controls.Add(lblFooter);
        }

        private Label MakeLabel(string text, int y)
        {
            return new Label()
            {
                Text = text,
                AutoSize = false,
                Size = new Size(300, 20),
                Location = new Point(25, y),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextMute
            };
        }

        private TextBox MakeTextBox(int y)
        {
            return new TextBox()
            {
                Size = new Size(300, 34),
                Location = new Point(25, y),
                BackColor = Color.FromArgb(7, 25, 47),
                ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
        }

        private async Task DoLoginAsync()
        {
            btnLogin.Enabled = false;

            lblStatus.ForeColor = Color.FromArgb(56, 189, 248);
            lblStatus.Text = "Connecting to API...";

            try
            {
                string payload =
                    $"{{\"username\":\"{txtUser.Text.Trim()}\",\"password\":\"{txtPass.Text}\"}}";

                var content =
                    new StringContent(payload, Encoding.UTF8, "application/json");

                var response =
                    await http.PostAsync($"{Program.ApiBase}/admin/login", content);

                var json = await response.Content.ReadAsStringAsync();

                var obj = JObject.Parse(json);

                if (obj["success"]?.Value<bool>() == true)
                {
                    lblStatus.ForeColor = Color.LightGreen;
                    lblStatus.Text = "Login successful!";

                    Program.CurrentUser =
                        obj["data"]?["username"]?.ToString() ?? txtUser.Text;

                    Program.CurrentRole =
                        obj["data"]?["role"]?.ToString() ?? "staff";

                    await Task.Delay(700);

                    MainForm main = new MainForm();

                    main.Show();

                    Hide();
                }
                else
                {
                    lblStatus.ForeColor = Danger;

                    lblStatus.Text =
                        obj["message"]?.ToString() ?? "Login failed.";
                }
            }
            catch (TaskCanceledException)
            {
                lblStatus.ForeColor = Danger;
                lblStatus.Text = "Connection timeout.";
            }
            catch (HttpRequestException)
            {
                lblStatus.ForeColor = Danger;
                lblStatus.Text = "Cannot connect to API server.";
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Danger;
                lblStatus.Text = ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }
    }
}
