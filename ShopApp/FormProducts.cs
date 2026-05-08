using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;

namespace ShopApp
{
    public class FormProducts : Form
    {
        private readonly User? _user;
        private readonly bool _isAdmin;
        private readonly bool _isManager;

        private readonly TextBox _search = new() { PlaceholderText = "Поиск по названию..." };
        private readonly ComboBox _cbCategory = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cbSort = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblCount = new() { AutoSize = true };
        private readonly FlowLayoutPanel _flow = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White,
            Padding = new Padding(10),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };

        private List<Product> _all = new();
        private readonly HashSet<int> _selected = new();

        public FormProducts(User? user)
        {
            _user = user;
            _isAdmin = user?.Role?.Code == "admin";
            _isManager = user?.Role?.Code == "manager";

            Text = "Магазин обуви — Каталог" + (user != null ? $"  ({user.FullName} / {user.Role?.Name})" : "  (гость)");
            Font = new Font("Times New Roman", 11F);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 700);
            try { Icon = new Icon("app.ico"); } catch { }

            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(240, 255, 240) };

            var logo = new PictureBox { Size = new Size(70, 70), Location = new Point(15, 15), SizeMode = PictureBoxSizeMode.Zoom };
            try { logo.Image = Image.FromFile("picture.png"); } catch { }

            var lblTitle = new Label
            {
                Text = "Каталог товаров",
                Font = new Font("Times New Roman", 18F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(95, 20)
            };

            _search.Location = new Point(95, 60);
            _search.Size = new Size(240, 25);
            _search.TextChanged += (s, e) => Render();

            var lblCat = new Label { Text = "Категория:", AutoSize = true, Location = new Point(355, 65) };
            _cbCategory.Location = new Point(440, 60);
            _cbCategory.Size = new Size(160, 25);
            _cbCategory.SelectedIndexChanged += (s, e) => Render();

            var lblSort = new Label { Text = "Сортировка:", AutoSize = true, Location = new Point(615, 65) };
            _cbSort.Location = new Point(705, 60);
            _cbSort.Size = new Size(180, 25);
            _cbSort.Items.AddRange(new object[]
            {
                "Цена (по возрастанию)", "Цена (по убыванию)",
                "Название (А-Я)", "Название (Я-А)"
            });
            _cbSort.SelectedIndex = 0;
            _cbSort.SelectedIndexChanged += (s, e) => Render();

            _lblCount.Location = new Point(900, 65);

            top.Controls.AddRange(new Control[] { logo, lblTitle, _search, lblCat, _cbCategory, lblSort, _cbSort, _lblCount });

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(240, 255, 240) };

            var btnOrder = new Button
            {
                Text = "Оформить заказ из выбранных",
                Location = new Point(15, 10),
                Size = new Size(230, 32),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOrder.Click += BtnOrder_Click;

            var btnOrders = new Button
            {
                Text = "Список заказов",
                Location = new Point(255, 10),
                Size = new Size(150, 32),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOrders.Click += (s, e) =>
            {
                using var f = new FormOrders(_user);
                f.ShowDialog();
                LoadData();
            };

            var btnAdd = new Button
            {
                Text = "+ Добавить товар",
                Location = new Point(415, 10),
                Size = new Size(150, 32),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = _isAdmin
            };
            btnAdd.Click += (s, e) =>
            {
                using var f = new FormEditProduct(null);
                if (f.ShowDialog() == DialogResult.OK) LoadData();
            };

            var btnLogout = new Button
            {
                Text = "Выйти",
                Location = new Point(975, 10),
                Size = new Size(100, 32),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.Click += (s, e) => { DialogResult = DialogResult.Retry; Close(); };

            bottom.Controls.AddRange(new Control[] { btnOrder, btnOrders, btnAdd, btnLogout });

            Controls.Add(_flow);
            Controls.Add(bottom);
            Controls.Add(top);
        }

        private void LoadData()
        {
            try
            {
                using var db = new ShopContext();
                _all = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer)
                    .Include(p => p.Supplier)
                    .OrderBy(p => p.Name)
                    .ToList();

                _cbCategory.Items.Clear();
                _cbCategory.Items.Add("Все категории");
                foreach (var c in db.Categories.OrderBy(c => c.Name))
                    _cbCategory.Items.Add(c.Name);
                _cbCategory.SelectedIndex = 0;

                Render();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void Render()
        {
            _flow.SuspendLayout();
            _flow.Controls.Clear();

            IEnumerable<Product> q = _all;

            if (!string.IsNullOrWhiteSpace(_search.Text))
                q = q.Where(p => p.Name.Contains(_search.Text, StringComparison.OrdinalIgnoreCase)
                              || p.Article.Contains(_search.Text, StringComparison.OrdinalIgnoreCase));

            if (_cbCategory.SelectedIndex > 0)
                q = q.Where(p => p.Category?.Name == (string)_cbCategory.SelectedItem!);

            q = _cbSort.SelectedIndex switch
            {
                0 => q.OrderBy(p => p.FinalPrice),
                1 => q.OrderByDescending(p => p.FinalPrice),
                2 => q.OrderBy(p => p.Name),
                3 => q.OrderByDescending(p => p.Name),
                _ => q
            };

            var list = q.ToList();
            _lblCount.Text = $"Показано: {list.Count} из {_all.Count}";

            foreach (var p in list)
                _flow.Controls.Add(BuildCard(p));

            _flow.ResumeLayout();
        }

        private Panel BuildCard(Product p)
        {
            // Цвет по бизнес-правилу: малый остаток / большая скидка
            Color baseBg = Color.White;
            if (p.StockQty < 3) baseBg = Color.FromArgb(255, 229, 180);   // персиковый — мало остатка
            else if (p.DiscountPct >= 15) baseBg = Color.FromArgb(240, 255, 240); // светло-зелёный — большая скидка

            var card = new Panel
            {
                Width = 320,
                Height = 200,
                Margin = new Padding(8),
                BackColor = baseBg,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = p,
                Cursor = Cursors.Hand,
                AccessibleDescription = baseBg.ToArgb().ToString()
            };

            var pic = new PictureBox
            {
                Location = new Point(8, 8),
                Size = new Size(110, 110),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            try
            {
                if (!string.IsNullOrWhiteSpace(p.Photo) && File.Exists(Path.Combine("photos", p.Photo)))
                    pic.Image = Image.FromFile(Path.Combine("photos", p.Photo));
                else if (File.Exists("picture.png"))
                    pic.Image = Image.FromFile("picture.png");
            }
            catch { }

            var lblName = new Label
            {
                Text = p.Name,
                Location = new Point(125, 8),
                Size = new Size(190, 36),
                Font = new Font("Times New Roman", 11F, FontStyle.Bold)
            };

            var lblManuf = new Label
            {
                Text = $"{p.Manufacturer?.Name} | арт. {p.Article}",
                Location = new Point(125, 48),
                Size = new Size(190, 32),
                ForeColor = Color.DimGray
            };

            string priceText;
            if (p.DiscountPct > 0)
                priceText = $"{p.FinalPrice:0.00} ₽   (-{p.DiscountPct}%)";
            else
                priceText = $"{p.Price:0.00} ₽";
            var lblPrice = new Label
            {
                Text = priceText,
                Location = new Point(125, 90),
                Size = new Size(190, 22),
                Font = new Font("Times New Roman", 12F, FontStyle.Bold),
                ForeColor = p.DiscountPct > 0 ? Color.FromArgb(46, 139, 87) : Color.Black
            };

            var lblStock = new Label
            {
                Text = $"На складе: {p.StockQty} {p.Unit}",
                Location = new Point(8, 125),
                Size = new Size(180, 22)
            };

            var lblCat = new Label
            {
                Text = $"Категория: {p.Category?.Name}",
                Location = new Point(8, 148),
                Size = new Size(310, 22),
                ForeColor = Color.DimGray
            };

            var chk = new CheckBox
            {
                Text = "Выбрать",
                Location = new Point(8, 170),
                AutoSize = true,
                Checked = _selected.Contains(p.Id)
            };
            chk.CheckedChanged += (s, e) =>
            {
                if (chk.Checked) _selected.Add(p.Id); else _selected.Remove(p.Id);
            };

            var btnEdit = new Button
            {
                Text = "Изменить",
                Location = new Point(220, 168),
                Size = new Size(95, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Visible = _isAdmin
            };
            btnEdit.Click += (s, e) =>
            {
                using var f = new FormEditProduct(p.Id);
                if (f.ShowDialog() == DialogResult.OK) LoadData();
            };

            // hover
            card.MouseEnter += (s, e) => card.BackColor = ControlPaint.Dark(baseBg, -0.05f);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(int.Parse(card.AccessibleDescription!));
            foreach (Control c in new Control[] { pic, lblName, lblManuf, lblPrice, lblStock, lblCat })
            {
                c.MouseEnter += (s, e) => card.BackColor = ControlPaint.Dark(baseBg, -0.05f);
                c.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(int.Parse(card.AccessibleDescription!));
            }

            card.Controls.AddRange(new Control[] { pic, lblName, lblManuf, lblPrice, lblStock, lblCat, chk, btnEdit });
            return card;
        }

        private void BtnOrder_Click(object? sender, EventArgs e)
        {
            if (_user == null)
            {
                MessageBox.Show("Гости не могут оформлять заказы. Авторизуйтесь.", "Внимание");
                return;
            }
            if (_selected.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один товар.", "Внимание");
                return;
            }
            try
            {
                using var db = new ShopContext();
                var pickup = db.PickupPoints.OrderBy(p => p.Id).First();
                var statusNew = db.OrderStatuses.First(s => s.Code == "new" || s.Name.Contains("Новый"));

                var order = new Order
                {
                    OrderDate = DateTime.UtcNow.Date,
                    DeliveryDate = DateTime.UtcNow.Date.AddDays(3),
                    PickupPointId = pickup.Id,
                    UserId = _user.Id,
                    PickupCode = new Random().Next(100, 1000).ToString(),
                    StatusId = statusNew.Id
                };
                foreach (var pid in _selected)
                    order.Items.Add(new OrderItem { ProductId = pid, Quantity = 1 });

                db.Orders.Add(order);
                db.SaveChanges();
                _selected.Clear();
                MessageBox.Show($"Заказ № {order.OrderNum} оформлен.\nКод получения: {order.PickupCode}",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Render();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
