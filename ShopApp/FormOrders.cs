using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;

namespace ShopApp
{
    public class FormOrders : Form
    {
        private readonly User? _user;
        private readonly bool _isAdmin;
        private readonly bool _isManager;
        private readonly DataGridView _dgv = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            BackgroundColor = Color.White,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };

        private readonly DataGridView _dgvItems = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoGenerateColumns = false,
            BackgroundColor = Color.White,
            RowHeadersVisible = false
        };

        public FormOrders(User? user)
        {
            _user = user;
            _isAdmin = user?.Role?.Code == "admin";
            _isManager = user?.Role?.Code == "manager";

            Text = "Заказы";
            Font = new Font("Times New Roman", 11F);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1000, 600);
            try { Icon = new Icon("app.ico"); } catch { }

            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "№", DataPropertyName = "OrderNum", Width = 50 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата заказа", DataPropertyName = "OrderDate", Width = 110 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата доставки", DataPropertyName = "DeliveryDate", Width = 110 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Клиент", DataPropertyName = "ClientName", Width = 180 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Пункт выдачи", DataPropertyName = "Pickup", Width = 220 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Код", DataPropertyName = "PickupCode", Width = 60 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "StatusName", Width = 130 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Сумма, ₽", DataPropertyName = "Total", Width = 90 });

            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Артикул", DataPropertyName = "Article", Width = 100 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Товар", DataPropertyName = "Name", Width = 320 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Кол-во", DataPropertyName = "Quantity", Width = 80 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена", DataPropertyName = "FinalPrice", Width = 90 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Сумма", DataPropertyName = "Subtotal", Width = 100 });

            _dgv.SelectionChanged += (s, e) => RenderItems();
            _dgv.RowPrePaint += Dgv_RowPrePaint;

            split.Panel1.Controls.Add(_dgv);
            split.Panel2.Controls.Add(_dgvItems);

            var top = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(240, 255, 240) };
            var lbl = new Label
            {
                Text = "Список заказов",
                Font = new Font("Times New Roman", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 12)
            };
            var btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(220, 10),
                Size = new Size(110, 30),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadData();

            var btnNextStatus = new Button
            {
                Text = "Сменить статус →",
                Location = new Point(335, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = _isAdmin || _isManager
            };
            btnNextStatus.Click += BtnNextStatus_Click;

            top.Controls.AddRange(new Control[] { lbl, btnRefresh, btnNextStatus });

            Controls.Add(split);
            Controls.Add(top);
        }

        private void Dgv_RowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dgv.Rows.Count) return;
            var row = _dgv.Rows[e.RowIndex];
            var status = row.Cells[6].Value?.ToString() ?? "";
            if (status.Contains("Отмен", StringComparison.OrdinalIgnoreCase))
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 182, 193);
            else if (status.Contains("Выдан", StringComparison.OrdinalIgnoreCase) || status.Contains("Заверш", StringComparison.OrdinalIgnoreCase))
                row.DefaultCellStyle.BackColor = Color.FromArgb(224, 224, 224);
            else if (status.Contains("Гото", StringComparison.OrdinalIgnoreCase))
                row.DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
        }

        private record OrderRow(int OrderNum, string OrderDate, string DeliveryDate,
            string ClientName, string Pickup, string PickupCode, string StatusName, decimal Total);

        private record ItemRow(string Article, string Name, int Quantity, decimal FinalPrice, decimal Subtotal);

        private List<OrderRow> _orders = new();
        private Dictionary<int, List<ItemRow>> _itemsByOrder = new();

        private void LoadData()
        {
            try
            {
                using var db = new ShopContext();
                var query = db.Orders
                    .Include(o => o.User)
                    .Include(o => o.PickupPoint)
                    .Include(o => o.Status)
                    .Include(o => o.Items).ThenInclude(i => i.Product)
                    .AsQueryable();

                // Обычный пользователь видит только свои заказы
                if (_user != null && !_isAdmin && !_isManager)
                    query = query.Where(o => o.UserId == _user.Id);

                var data = query.OrderByDescending(o => o.OrderNum).ToList();

                _orders = data.Select(o => new OrderRow(
                    o.OrderNum,
                    o.OrderDate.ToString("dd.MM.yyyy"),
                    o.DeliveryDate.ToString("dd.MM.yyyy"),
                    o.User?.FullName ?? "-",
                    o.PickupPoint?.Address ?? "-",
                    o.PickupCode,
                    o.Status?.Name ?? "-",
                    o.Items.Sum(i => Math.Round(i.Product!.Price * (100 - i.Product.DiscountPct) / 100m, 2) * i.Quantity)
                )).ToList();

                _itemsByOrder = data.ToDictionary(
                    o => o.OrderNum,
                    o => o.Items.Select(i =>
                    {
                        var fp = Math.Round(i.Product!.Price * (100 - i.Product.DiscountPct) / 100m, 2);
                        return new ItemRow(i.Product!.Article, i.Product.Name, i.Quantity, fp, fp * i.Quantity);
                    }).ToList());

                _dgv.DataSource = null;
                _dgv.DataSource = _orders;
                RenderItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void RenderItems()
        {
            _dgvItems.DataSource = null;
            if (_dgv.CurrentRow?.DataBoundItem is OrderRow r && _itemsByOrder.TryGetValue(r.OrderNum, out var items))
                _dgvItems.DataSource = items;
        }

        private void BtnNextStatus_Click(object? sender, EventArgs e)
        {
            if (_dgv.CurrentRow?.DataBoundItem is not OrderRow row) return;
            try
            {
                using var db = new ShopContext();
                var order = db.Orders.Include(o => o.Status).First(o => o.OrderNum == row.OrderNum);
                var statuses = db.OrderStatuses.OrderBy(s => s.Id).ToList();
                var idx = statuses.FindIndex(s => s.Id == order.StatusId);
                if (idx < 0 || idx + 1 >= statuses.Count)
                {
                    MessageBox.Show("Дальнейшая смена статуса невозможна.");
                    return;
                }
                order.StatusId = statuses[idx + 1].Id;
                db.SaveChanges();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
