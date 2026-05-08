# GUIDE.md - Шпаргалка для пробного демо-экзамена (КОД 09.02.07)

> Цель файла - **выучить наизусть** минимум, достаточный для написания
> WinForms-приложения "Магазин" с PostgreSQL за отведённое время.
> Всё лишнее убрано. На пробнике достаточно адаптировать имена.

---

## ⚠️ Честно про формат интерфейса

В **демо-экзамене ИСИП (КОД 09.02.07)** **жёсткой обязанности делать карточки нет**.
ТЗ обычно формулирует требования ОБЩИМИ словами: "вывести список товаров с фото,
ценой, кнопкой действия". Эксперт оценивает соответствие **функционалу из ТЗ**,
а не конкретный контрол.

Что реально встречается в ТЗ ФГОС-комплектах:

| Вариант UI | Когда подходит |
|---|---|
| **Карточки в FlowLayoutPanel** (этот шаблон) | Когда в ТЗ написано "вывести в виде карточек/плиток" или есть скриншот с плиточным макетом. Самый эффектный визуал. |
| **DataGridView со списком** | Когда ТЗ говорит "таблица товаров", "список с возможностью редактирования", или скриншоты показывают сетку. Реализуется быстрее. |
| **ListBox/ListView с детализацией справа** | Реже, но бывает в комплектах с акцентом на CRUD. |

**Стратегия:** прочитай ТЗ + посмотри скриншоты-приложения, если они даны. Если в
скриншотах сетка - делай DataGridView, не теряй время на карточки. Если плитки -
бери шаблон карточек отсюда. Карточки сложнее, но дают +визуал и +креативность.

> В коде шаблона ВСЕ места, требующие изменений под конкретное ТЗ, помечены
> комментарием `// * ТЗ:`. Поиск по проекту → видишь сразу что трогать.

---

## 0. Чек-лист первого часа

1. Создать БД из CSV (5-10 минут).
2. Создать .NET-проект `dotnet new winforms -n ShopApp -f net9.0-windows` (1 мин).
3. Подключить EF Core + Npgsql (1 мин).
4. Скопировать модель `Product` и `ShopContext` из этого шаблона, поменять имена под задание.
5. `FormLogin` → `FormProducts` (карточки) → `FormOrders`.
6. Прогнать `dotnet build` каждые 10 минут - не копить ошибки.

---

## 1. PostgreSQL: создание БД

### Универсальный шаблон `create_db.sql`

```sql
DROP DATABASE IF EXISTS shop_db;
CREATE DATABASE shop_db ENCODING 'UTF8';
\c shop_db

CREATE TABLE roles (
    id   SERIAL PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE users (
    id            SERIAL PRIMARY KEY,
    login         VARCHAR(50) UNIQUE NOT NULL,
    password_text VARCHAR(100) NOT NULL,
    full_name     VARCHAR(150) NOT NULL,
    role_id       INT NOT NULL REFERENCES roles(id)
);

CREATE TABLE categories     (id SERIAL PRIMARY KEY, name VARCHAR(100) NOT NULL);
CREATE TABLE manufacturers  (id SERIAL PRIMARY KEY, name VARCHAR(100) NOT NULL);
CREATE TABLE suppliers      (id SERIAL PRIMARY KEY, name VARCHAR(100) NOT NULL);
CREATE TABLE order_statuses (id SERIAL PRIMARY KEY, code VARCHAR(20) UNIQUE, name VARCHAR(50) NOT NULL);
CREATE TABLE pickup_points  (id SERIAL PRIMARY KEY, address VARCHAR(255) NOT NULL);

CREATE TABLE products (
    id              SERIAL PRIMARY KEY,
    article         VARCHAR(50) UNIQUE NOT NULL,
    name            VARCHAR(200) NOT NULL,
    unit            VARCHAR(20),
    price           NUMERIC(10,2) NOT NULL,
    supplier_id     INT REFERENCES suppliers(id),
    manufacturer_id INT REFERENCES manufacturers(id),
    category_id     INT REFERENCES categories(id),
    discount_pct    INT DEFAULT 0 CHECK (discount_pct BETWEEN 0 AND 100),
    stock_qty       INT DEFAULT 0 CHECK (stock_qty >= 0),
    description     TEXT,
    photo           VARCHAR(255)
);

CREATE TABLE orders (
    order_num       SERIAL PRIMARY KEY,
    order_date      DATE NOT NULL,
    delivery_date   DATE,
    pickup_point_id INT REFERENCES pickup_points(id),
    user_id         INT REFERENCES users(id),
    pickup_code     VARCHAR(20),
    status_id       INT REFERENCES order_statuses(id)
);

CREATE TABLE order_items (
    id         SERIAL PRIMARY KEY,
    order_num  INT REFERENCES orders(order_num) ON DELETE CASCADE,
    product_id INT REFERENCES products(id),
    quantity   INT CHECK (quantity > 0)
);

CREATE INDEX idx_products_category  ON products(category_id);
CREATE INDEX idx_orders_user        ON orders(user_id);
CREATE INDEX idx_orderitems_order   ON order_items(order_num);
```

После INSERT-ов с явными `id` обязательно:

```sql
SELECT setval('roles_id_seq',     (SELECT MAX(id)        FROM roles));
SELECT setval('orders_order_num_seq', (SELECT MAX(order_num) FROM orders));
-- и так для каждой таблицы
```

### Запуск
```powershell
$env:PGPASSWORD="postgres"
& "C:\Program Files\PostgreSQL\17\bin\psql.exe" -U postgres -h localhost -d postgres -f create_db.sql
```

### Импорт CSV в Python (если данные большие)

```python
# generate_sql.py - превращает CSV из import/csv в один create_db.sql
import csv, pathlib
CSV = pathlib.Path("import/csv")

def esc(v): return "NULL" if v == "" else "'" + v.replace("'", "''") + "'"

def insert(table, cols, num_cols=()):
    rows = []
    with (CSV / f"{table}.csv").open(encoding="utf-8") as f:
        for r in csv.DictReader(f):
            vals = [r[c] if c in num_cols and r[c] != "" else esc(r[c]) for c in cols]
            rows.append("(" + ",".join(vals) + ")")
    return f"INSERT INTO {table} ({','.join(cols)}) VALUES\n" + ",\n".join(rows) + ";\n"
```

### Импорт CSV напрямую через psql `\copy` (без Python)

Если CSV уже в правильной форме (заголовок + строки) - самый быстрый путь:

```sql
-- внутри psql, после CREATE TABLE
\copy roles(id, code, name)        FROM 'import/csv/roles.csv'        WITH (FORMAT csv, HEADER true);
\copy categories(id, name)         FROM 'import/csv/categories.csv'   WITH (FORMAT csv, HEADER true);
\copy manufacturers(id, name)      FROM 'import/csv/manufacturers.csv' WITH (FORMAT csv, HEADER true);
\copy products(id, article, name, unit, price, supplier_id, manufacturer_id, category_id, discount_pct, stock_qty, description, photo) FROM 'import/csv/products.csv' WITH (FORMAT csv, HEADER true);
-- и так далее для каждой таблицы
```

После всех `\copy` обязательно сбрось sequences:
```sql
SELECT setval('roles_id_seq',     (SELECT COALESCE(MAX(id),1) FROM roles));
SELECT setval('products_id_seq',  (SELECT COALESCE(MAX(id),1) FROM products));
SELECT setval('orders_order_num_seq', (SELECT COALESCE(MAX(order_num),1) FROM orders));
```

---

## 2. .NET проект: создание + Npgsql

### 2.1 Создание проекта с нуля

```powershell
mkdir ShopApp; cd ShopApp
dotnet new winforms -f net9.0-windows
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.4
dotnet build   # проверка
```

> Команда `dotnet add package` сама пропишет `<PackageReference>` в `.csproj` и подтянет зависимости (`Microsoft.EntityFrameworkCore`, `Npgsql`).

### 2.2 Если интернета нет - ручная правка `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ApplicationIcon>app.ico</ApplicationIcon>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="photos\**\*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
```

После правки: `dotnet restore && dotnet build`.

### 2.3 Подключение через Visual Studio (если экзамен в IDE)

1. ПКМ по проекту → **Manage NuGet Packages** → вкладка **Browse**.
2. Ищем `Npgsql.EntityFrameworkCore.PostgreSQL` → **Install**.
3. Версия должна совпадать с .NET (9.0.x для .NET 9, 8.0.x для .NET 8).

### 2.4 Стандартные `using`-и в каждом файле с БД

```csharp
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;
```

### 2.5 Обязательная строка в `Program.Main` ДО `Application.Run`

```csharp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
ApplicationConfiguration.Initialize();
```

Без этого получишь `Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'`.

### 2.6 Проверка соединения

В любой форме можно сделать кнопку "Тест БД":
```csharp
try {
    using var db = new ShopContext();
    int n = db.Products.Count();
    MessageBox.Show($"OK, товаров: {n}");
} catch (Exception ex) {
    MessageBox.Show("Нет связи: " + ex.Message);
}
```
Если ошибка - проверь:
- работает ли служба PostgreSQL (`Get-Service postgresql*`),
- порт `5432` (см. `pg_hba.conf` / `postgresql.conf`),
- пароль в connection string,
- имя БД (`shop_db`) и что она реально создана.

---

## 3. EF Core - обязательный паттерн

**1 файл = 1 модель**, имена свойств **PascalCase**, маппинг в БД через `HasColumnName`.

```csharp
public class Product
{
    public int Id { get; set; }
    public string Article { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int DiscountPct { get; set; }
    public int StockQty { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    // вычислимое - НЕ маппить:
    public decimal FinalPrice => Math.Round(Price * (100 - DiscountPct) / 100m, 2);
}
```

`ShopContext` - **в одном файле** все DbSet и Fluent API:

```csharp
public class ShopContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    // ...

    protected override void OnConfiguring(DbContextOptionsBuilder b)
        => b.UseNpgsql("Host=localhost;Port=5432;Database=shop_db;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Article).HasColumnName("article");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Price).HasColumnName("price");
            e.Property(x => x.DiscountPct).HasColumnName("discount_pct");
            e.Property(x => x.StockQty).HasColumnName("stock_qty");
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Ignore(x => x.FinalPrice);
            e.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId);
        });
    }
}
```

⚠️ **Всегда вызывать в `Program.Main` до `Application.Run`:**
```csharp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
ApplicationConfiguration.Initialize();
```
Это снимает проблему с `timestamp without time zone` в Npgsql 6+.

---

## 4. Стандартный стиль (запомнить раз и навсегда)

| Цвет     | HEX        | Где |
|----------|------------|------|
| Фон      | `#FFFFFF`  | Form, FlowLayoutPanel |
| Доп. фон | `#F0FFF0`  | Top/Bottom Panel, кнопка-гость |
| Акцент   | `#4CAF50`  | Главные кнопки (Войти, Сохранить) |
| Персик   | `#FFE5B4`  | Карточка с малым остатком (`stock < 3`) |
| Серый    | `#E0E0E0`  | Завершённые / снятые с продажи |
| Розовый  | `#FFB6C1`  | Отменённые заказы |
| Зелёный темн.| `#2E8B57` | Текст цены со скидкой |

Шрифт: **Times New Roman 11pt**, заголовки **14-18pt Bold**.

---

## 5. FormLogin - каркас

* `TextBox` + `TextBox.UseSystemPasswordChar = true`
* Кнопка "Войти" - зелёная, кнопка "Гость" - светлая.
* Логика:

```csharp
using var db = new ShopContext();
var u = db.Users.Include(x => x.Role)
    .FirstOrDefault(x => x.Login == _login.Text && x.PasswordText == _password.Text);
if (u == null) { _attempts++; /* после 3 - Timer 10 сек */ return; }
AuthenticatedUser = u;
DialogResult = DialogResult.OK;
```

Учётные записи (есть в БД пробника, но **на экзамене проверь свои!**):
- admin/admin → роль `admin`
- manager/manager → роль `manager`
- client/client (или `user1` и т.д.) → роль `client`

---

## 6. FormProducts - карточки (сердце задачи)

Структура формы:
```
[ Top Panel: лого + поиск + фильтр Категория + сортировка + счётчик ]
[ FlowLayoutPanel (Dock=Fill, AutoScroll, WrapContents=true)        ]
[ Bottom Panel: Оформить заказ | Список заказов | + Товар | Выйти   ]
```

Карточка `Panel 320×200`:
- слева `PictureBox 110×110`, `SizeMode=Zoom`;
- справа `Label Name (Bold)` → `Manufacturer | арт. ###` → `Цена` (зелёная если скидка) → `Остаток` → `Категория`;
- внизу `CheckBox "Выбрать"` + кнопка `"Изменить"` (только для admin).

Цвет фона карточки:
```csharp
Color baseBg = Color.White;
if (p.StockQty < 3)        baseBg = Color.FromArgb(255, 229, 180);
else if (p.DiscountPct >= 15) baseBg = Color.FromArgb(240, 255, 240);
```

Hover (легко запомнить): `card.MouseEnter += (s,e) => card.BackColor = ControlPaint.Dark(baseBg, -0.05f);`
Восстановление: храним ARGB в `card.AccessibleDescription` и в `MouseLeave` парсим обратно.

Поиск+фильтр+сортировка на LINQ:
```csharp
var q = _all.AsEnumerable();
if (!string.IsNullOrWhiteSpace(_search.Text))
    q = q.Where(p => p.Name.Contains(_search.Text, StringComparison.OrdinalIgnoreCase));
if (_cbCategory.SelectedIndex > 0)
    q = q.Where(p => p.Category!.Name == (string)_cbCategory.SelectedItem!);
q = _cbSort.SelectedIndex switch {
    0 => q.OrderBy(p => p.FinalPrice),
    1 => q.OrderByDescending(p => p.FinalPrice),
    2 => q.OrderBy(p => p.Name),
    _ => q.OrderByDescending(p => p.Name)
};
```

---

## 7. FormEditProduct - CRUD одного товара

* Один `TextBox`/`NumericUpDown` на каждое поле.
* `ComboBox` с `DisplayMember = "Name"` для FK.
* Сохранение:
```csharp
Product p = _id == null ? new Product() : db.Products.First(x => x.Id == _id);
if (_id == null) db.Products.Add(p);
p.Article = _article.Text.Trim();
// ... остальные поля
db.SaveChanges();
DialogResult = DialogResult.OK;
```
* Валидация (минимум): артикул + название не пусты, артикул уникален при создании.

---

## 8. FormOrders - список + детализация

Двухпанельный `SplitContainer` (Horizontal): сверху список заказов, снизу состав.
Раскраска строк через `RowPrePaint`:
```csharp
if (status.Contains("Отмен"))    row.DefaultCellStyle.BackColor = Color.FromArgb(255,182,193);
else if (status.Contains("Выдан"))  row.DefaultCellStyle.BackColor = Color.FromArgb(224,224,224);
```

Сумма заказа на лету:
```csharp
o.Items.Sum(i => Math.Round(i.Product!.Price * (100 - i.Product.DiscountPct) / 100m, 2) * i.Quantity)
```

---

## 9. Оформление заказа из выбранных карточек

```csharp
var pickup = db.PickupPoints.OrderBy(p => p.Id).First();
var st = db.OrderStatuses.First(s => s.Code == "new" || s.Name.Contains("Новый"));
var order = new Order {
    OrderDate    = DateTime.UtcNow.Date,
    DeliveryDate = DateTime.UtcNow.Date.AddDays(3),
    PickupPointId= pickup.Id,
    UserId       = _user.Id,
    PickupCode   = new Random().Next(100, 1000).ToString(),
    StatusId     = st.Id
};
foreach (var pid in _selected) order.Items.Add(new OrderItem { ProductId = pid, Quantity = 1 });
db.Orders.Add(order); db.SaveChanges();
```

---

## 10. Роли - простой контроль доступа

```csharp
bool isAdmin   = user?.Role?.Code == "admin";
bool isManager = user?.Role?.Code == "manager";

btnAdd.Visible = isAdmin;          // CRUD товаров
btnNextStatus.Visible = isAdmin || isManager; // смена статуса заказа
// гость (user==null) - только просмотр, заказ запрещён
```

---

## 11. Шесть типичных ошибок и их фикс

1. **`error CS7065`** - `app.ico` оказался PNG. Конвертировать через Pillow:
   `Image.open('p.png').save('app.ico', sizes=[(16,16),(32,32),(48,48),(256,256)])`.
2. **Неверная дата `2025-02-30` в CSV** - патчить в питоне перед INSERT.
3. **`setval` падает на пустой таблице** - заполняй её хоть одной строкой или убирай setval.
4. **`Cannot write DateTime ...`** - забыл `Npgsql.EnableLegacyTimestampBehavior=true`.
5. **DataGridView пустой** - забыл `AutoGenerateColumns = false` и `DataPropertyName`.
6. **Карточки не обновляются** - после `SaveChanges()` вызывай `LoadData()` повторно.
7. **`Column 'photo' is null`** - поле в БД может быть NULL. Делай свойство `string? Photo` (или `string? Description`), а в коде используй `p.Photo ?? ""`.

---

## 12. План на 4 часа

| Время | Что делать |
|-------|------------|
| 0:00-0:20 | Создать БД (SQL/CSV → psql) |
| 0:20-0:40 | Скелет проекта, `dotnet new`, csproj, ShopContext + 1 модель, build |
| 0:40-1:00 | Все остальные модели + Fluent API |
| 1:00-1:30 | FormLogin (3 попытки + Timer) |
| 1:30-2:30 | FormProducts: карточки, поиск, фильтр, сортировка |
| 2:30-3:00 | FormEditProduct (CRUD) |
| 3:00-3:30 | FormOrders + оформление заказа |
| 3:30-3:50 | Стиль (цвета, иконка, шрифт), скриншоты |
| 3:50-4:00 | Скомпилировать в Release, проверить запуск |

---

## 13. Команды-рутина (запомни как стихи)

```powershell
# создать БД
$env:PGPASSWORD="postgres"
& "C:\Program Files\PostgreSQL\17\bin\psql.exe" -U postgres -h localhost -d postgres -f create_db.sql

# собрать проект
cd ShopApp; dotnet build

# запустить
dotnet run --project ShopApp.csproj
```

```bash
# git
git init && git add . && git commit -m "init" && git push
```

---

> **Главное правило экзамена**: каждые 15 минут **компилируй**.
> Не пиши 2 часа подряд без `dotnet build` - потом не разгребёшь.
> Если что-то не получается - оставь заглушку и иди дальше.
> Лучше 80% работающего, чем 100% сломанного.

---

## 14. Альтернатива карточкам - DataGridView (быстрее)

Если ТЗ предполагает таблицу:

```csharp
var dgv = new DataGridView {
    Dock = DockStyle.Fill,
    AutoGenerateColumns = false,
    ReadOnly = true,
    AllowUserToAddRows = false,
    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    RowHeadersVisible = false,
    BackgroundColor = Color.White
};
dgv.Columns.Add(new DataGridViewImageColumn { HeaderText = "Фото", Width = 80 });
dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Артикул",  DataPropertyName = "Article",   Width = 100 });
dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = "Name",      Width = 250 });
dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена",     DataPropertyName = "FinalPrice", Width = 90 });
dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Остаток",  DataPropertyName = "StockQty",  Width = 80 });
dgv.DataSource = products;

// Раскраска строк по правилу
dgv.RowPrePaint += (s, e) => {
    var p = (Product)dgv.Rows[e.RowIndex].DataBoundItem;
    if (p.StockQty < 3)  dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255,229,180);
    else if (p.DiscountPct >= 15) dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(240,255,240);
};
```

Плюсы: пишется в 3 раза быстрее карточек, без overflow и hover'ов.
Минусы: не "вау" по визуалу. Эксперт обычно ставит +1 балл за карточки сверх ТЗ,
но **только** если они сделаны без багов.

---

## 15. `touch_times.ps1` - выровнять метки времени

Если файлы шаблона надо выдать за "свежесозданные" (например, чтобы они выглядели
как работа последнего часа), используй скрипт `touch_times.ps1` из корня репо.

```powershell
# Запуск из любой папки:
powershell -ExecutionPolicy Bypass -File .\touch_times.ps1
# Или с другим окном:
powershell -ExecutionPolicy Bypass -File .\touch_times.ps1 -Minutes 30
```

Скрипт **сам определяет свою папку** (через `$MyInvocation`) и обходит все
файлы рекурсивно, ставя каждому **случайное** время в пределах последнего часа.
Кладёшь его в корень проекта на любом ПК → запускаешь → готово.
