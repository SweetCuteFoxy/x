# Demo_Exam_Template

Учебный шаблон под пробный/реальный демо-экзамен **КОД 09.02.07** (магазин обуви).

## Что внутри

- `SQL/` - `generate_sql.py` (CSV → SQL) и готовый `create_db.sql` для PostgreSQL.
- `ShopApp/` - WinForms (.NET 9) приложение: логин → каталог карточек → CRUD товара → заказы.
- `GUIDE.md` - **главный файл шпаргалки**. Выучить наизусть, на экзамене менять только имена.

## Быстрый старт

```powershell
# 1) Создать БД
$env:PGPASSWORD="postgres"
& "C:\Program Files\PostgreSQL\17\bin\psql.exe" -U postgres -h localhost -d postgres -f SQL\create_db.sql

# 2) Собрать и запустить
cd ShopApp
dotnet build
dotnet run
```

## Учётные записи (из CSV пробника)

| Логин   | Пароль   | Роль    |
|---------|----------|---------|
| admin   | admin    | admin   |
| manager | manager  | manager |
| client  | client   | client  |

(точные пары проверь в таблице `users`)

## Зависимости

- .NET 9 SDK
- PostgreSQL 17 (`postgres`/`postgres` на `localhost:5432`)
- `Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4`

## Структура

```
Demo_Exam_Template/
├-- GUIDE.md                ← главное: то, что учим
├-- README.md
├-- SQL/
|   ├-- generate_sql.py
|   └-- create_db.sql
└-- ShopApp/
    ├-- ShopApp.csproj
    ├-- Program.cs
    ├-- FormLogin.cs
    ├-- FormProducts.cs
    ├-- FormEditProduct.cs
    ├-- FormOrders.cs
    ├-- app.ico, picture.png, photos/
    └-- Models/
        ├-- ShopContext.cs
        ├-- Role.cs, User.cs
        ├-- Category.cs, Manufacturer.cs, Supplier.cs
        ├-- Product.cs
        ├-- PickupPoint.cs, OrderStatus.cs
        └-- Order.cs, OrderItem.cs
```
