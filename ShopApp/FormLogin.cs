using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;

namespace ShopApp
{
    public class FormLogin : Form
    {
        private readonly TextBox _login = new();
        private readonly TextBox _password = new() { UseSystemPasswordChar = true };
        private readonly Label _info = new() { ForeColor = Color.DarkRed, AutoSize = true };
        private int _attempts;

        public User? AuthenticatedUser { get; private set; }

        public FormLogin()
        {
            // * ТЗ: название приложения в заголовке и в логотипе - менять под формулировку задания.
            Text = "Авторизация - Магазин обуви";
            // * ТЗ: шрифт и размер. Часто в ТЗ хотят Segoe UI / Calibri / TNR.
            Font = new Font("Times New Roman", 11F);
            // * ТЗ: фоновый цвет. Бренд-цвета из ТЗ подставляем сюда.
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 360);
            try { Icon = new Icon("app.ico"); } catch { }

            var logo = new PictureBox
            {
                Size = new Size(110, 110),
                Location = new Point(155, 20),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            try { logo.Image = Image.FromFile("picture.png"); } catch { }

            var lTitle = new Label
            {
                Text = "Вход в систему",
                Font = new Font("Times New Roman", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 140)
            };

            var lLogin = new Label { Text = "Логин:", AutoSize = true, Location = new Point(50, 185) };
            _login.Location = new Point(150, 182);
            _login.Size = new Size(220, 25);

            var lPass = new Label { Text = "Пароль:", AutoSize = true, Location = new Point(50, 220) };
            _password.Location = new Point(150, 217);
            _password.Size = new Size(220, 25);

            var btnOk = new Button
            {
                Text = "Войти",
                Location = new Point(150, 260),
                Size = new Size(105, 32),
                // * ТЗ: акцентный цвет главной кнопки. Меняется под палитру варианта.
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += BtnOk_Click;

            var btnGuest = new Button
            {
                Text = "Гость",
                Location = new Point(265, 260),
                Size = new Size(105, 32),
                BackColor = Color.FromArgb(240, 255, 240),
                FlatStyle = FlatStyle.Flat
            };
            btnGuest.Click += (s, e) =>
            {
                AuthenticatedUser = null;
                DialogResult = DialogResult.OK;
            };

            _info.Location = new Point(50, 305);

            Controls.AddRange(new Control[] { logo, lTitle, lLogin, _login, lPass, _password, btnOk, btnGuest, _info });
            AcceptButton = btnOk;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            try
            {
                using var db = new ShopContext();
                var user = db.Users.Include(u => u.Role)
                    .FirstOrDefault(u => u.Login == _login.Text && u.PasswordText == _password.Text);
                if (user == null)
                {
                    _attempts++;
                    _info.Text = $"Неверный логин или пароль (попытка {_attempts})";
                    if (_attempts >= 3)
                    {
                        MessageBox.Show("Превышено количество попыток. Подождите 10 секунд.",
                            "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Enabled = false;
                        var t = new System.Windows.Forms.Timer { Interval = 10000 };
                        t.Tick += (s, ev) => { Enabled = true; _attempts = 0; _info.Text = ""; t.Stop(); };
                        t.Start();
                    }
                    return;
                }
                AuthenticatedUser = user;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка БД: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
