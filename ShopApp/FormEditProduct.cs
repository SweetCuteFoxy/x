using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;

namespace ShopApp
{
    public class FormEditProduct : Form
    {
        private readonly int? _productId;

        private readonly TextBox _article = new();
        private readonly TextBox _name = new();
        private readonly TextBox _unit = new();
        private readonly NumericUpDown _price = new() { Maximum = 1_000_000, DecimalPlaces = 2, Increment = 10 };
        private readonly NumericUpDown _stock = new() { Maximum = 100000 };
        private readonly NumericUpDown _discount = new() { Maximum = 100 };
        private readonly TextBox _photo = new();
        private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly ComboBox _cbCategory = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cbManuf = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cbSupplier = new() { DropDownStyle = ComboBoxStyle.DropDownList };

        public FormEditProduct(int? productId)
        {
            _productId = productId;
            Text = productId == null ? "Новый товар" : "Редактирование товара";
            Font = new Font("Times New Roman", 11F);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(540, 560);
            try { Icon = new Icon("app.ico"); } catch { }

            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            int y = 15, h = 30;

            void Row(string caption, Control c)
            {
                var lbl = new Label { Text = caption, Location = new Point(20, y + 3), AutoSize = true };
                c.Location = new Point(180, y);
                c.Size = new Size(330, 25);
                Controls.Add(lbl);
                Controls.Add(c);
                y += h;
            }

            Row("Артикул:", _article);
            Row("Название:", _name);
            Row("Категория:", _cbCategory);
            Row("Производитель:", _cbManuf);
            Row("Поставщик:", _cbSupplier);
            Row("Ед. изм.:", _unit);
            Row("Цена (₽):", _price);
            Row("Скидка (%):", _discount);
            Row("Остаток:", _stock);
            Row("Фото (имя файла):", _photo);

            var lblD = new Label { Text = "Описание:", Location = new Point(20, y + 3), AutoSize = true };
            _description.Location = new Point(180, y);
            _description.Size = new Size(330, 110);
            Controls.Add(lblD);
            Controls.Add(_description);
            y += 120;

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(180, y + 5),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(350, y + 5),
                Size = new Size(160, 32),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            CancelButton = btnCancel;
        }

        private void LoadData()
        {
            using var db = new ShopContext();
            foreach (var c in db.Categories.OrderBy(x => x.Name)) _cbCategory.Items.Add(c);
            foreach (var m in db.Manufacturers.OrderBy(x => x.Name)) _cbManuf.Items.Add(m);
            foreach (var s in db.Suppliers.OrderBy(x => x.Name)) _cbSupplier.Items.Add(s);
            _cbCategory.DisplayMember = "Name";
            _cbManuf.DisplayMember = "Name";
            _cbSupplier.DisplayMember = "Name";

            if (_productId == null)
            {
                if (_cbCategory.Items.Count > 0) _cbCategory.SelectedIndex = 0;
                if (_cbManuf.Items.Count > 0) _cbManuf.SelectedIndex = 0;
                if (_cbSupplier.Items.Count > 0) _cbSupplier.SelectedIndex = 0;
                _unit.Text = "пара";
                return;
            }

            var p = db.Products.First(x => x.Id == _productId.Value);
            _article.Text = p.Article;
            _name.Text = p.Name;
            _unit.Text = p.Unit;
            _price.Value = p.Price;
            _stock.Value = p.StockQty;
            _discount.Value = p.DiscountPct;
            _photo.Text = p.Photo;
            _description.Text = p.Description;
            _cbCategory.SelectedItem = _cbCategory.Items.Cast<Category>().FirstOrDefault(c => c.Id == p.CategoryId);
            _cbManuf.SelectedItem = _cbManuf.Items.Cast<Manufacturer>().FirstOrDefault(m => m.Id == p.ManufacturerId);
            _cbSupplier.SelectedItem = _cbSupplier.Items.Cast<Supplier>().FirstOrDefault(s => s.Id == p.SupplierId);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_article.Text) || string.IsNullOrWhiteSpace(_name.Text))
            {
                MessageBox.Show("Артикул и название обязательны.", "Проверка");
                return;
            }
            if (_cbCategory.SelectedItem is not Category cat ||
                _cbManuf.SelectedItem is not Manufacturer man ||
                _cbSupplier.SelectedItem is not Supplier sup)
            {
                MessageBox.Show("Выберите категорию, производителя и поставщика.", "Проверка");
                return;
            }

            try
            {
                using var db = new ShopContext();
                Product p;
                if (_productId == null)
                {
                    if (db.Products.Any(x => x.Article == _article.Text))
                    {
                        MessageBox.Show("Артикул уже существует.", "Проверка");
                        return;
                    }
                    p = new Product();
                    db.Products.Add(p);
                }
                else
                {
                    p = db.Products.First(x => x.Id == _productId.Value);
                }

                p.Article = _article.Text.Trim();
                p.Name = _name.Text.Trim();
                p.Unit = _unit.Text.Trim();
                p.Price = _price.Value;
                p.StockQty = (int)_stock.Value;
                p.DiscountPct = (int)_discount.Value;
                p.Photo = _photo.Text.Trim();
                p.Description = _description.Text.Trim();
                p.CategoryId = cat.Id;
                p.ManufacturerId = man.Id;
                p.SupplierId = sup.Id;

                db.SaveChanges();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
    }
}
