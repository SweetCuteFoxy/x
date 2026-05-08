"""Convert exam CSVs to a single create_db.sql file."""
import csv
from pathlib import Path

CSV_DIR = Path(r"C:\Users\edikk\Desktop\DEMO_E\Прил_2_ОЗ_КОД 09.02.07-2-2026-М1\import\csv")
OUT = Path(__file__).parent / "create_db.sql"


def esc(v: str) -> str:
    if v == "" or v.upper() == "NULL":
        return "NULL"
    return "'" + v.replace("'", "''") + "'"


def insert(table: str, columns: list[str], cast: dict[str, str] | None = None) -> str:
    cast = cast or {}
    rows = []
    with (CSV_DIR / f"{table}.csv").open(encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for r in reader:
            # фикс некорректных дат в данных пробника
            for k, v in list(r.items()):
                if v == "2025-02-30":
                    r[k] = "2025-02-28"
                elif v == "2025-04-31":
                    r[k] = "2025-04-30"
            vals = []
            for c in columns:
                v = r[c]
                if c in cast and cast[c] in ("int", "numeric") and v != "":
                    vals.append(v)
                else:
                    vals.append(esc(v))
            rows.append(f"  ({', '.join(vals)})")
    return f"INSERT INTO {table} ({', '.join(columns)}) VALUES\n" + ",\n".join(rows) + ";\n\n"


HEADER = """-- =============================================================
--   Демо-проект ДЭ КОД 09.02.07 — Интернет-магазин обуви
--   Создание БД, схемы, заполнение данными
-- =============================================================

DROP DATABASE IF EXISTS shop_db;
CREATE DATABASE shop_db ENCODING 'UTF8';
\\c shop_db

-- ---------- 1. Справочники ----------
CREATE TABLE roles (
    id   SERIAL PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(50) NOT NULL
);

CREATE TABLE categories (
    id   SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE manufacturers (
    id   SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE suppliers (
    id   SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE order_statuses (
    id   SERIAL PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(50) NOT NULL
);

CREATE TABLE pickup_points (
    id      SERIAL PRIMARY KEY,
    address VARCHAR(255) NOT NULL
);

-- ---------- 2. Пользователи ----------
CREATE TABLE users (
    id            SERIAL PRIMARY KEY,
    login         VARCHAR(100) UNIQUE NOT NULL,
    password_text VARCHAR(50)  NOT NULL,
    full_name     VARCHAR(150) NOT NULL,
    role_id       INT NOT NULL REFERENCES roles(id)
);

-- ---------- 3. Товары ----------
CREATE TABLE products (
    id              SERIAL PRIMARY KEY,
    article         VARCHAR(20) UNIQUE NOT NULL,
    name            VARCHAR(150) NOT NULL,
    unit            VARCHAR(20)  NOT NULL DEFAULT 'шт.',
    price           NUMERIC(10,2) NOT NULL CHECK (price >= 0),
    supplier_id     INT NOT NULL REFERENCES suppliers(id),
    manufacturer_id INT NOT NULL REFERENCES manufacturers(id),
    category_id     INT NOT NULL REFERENCES categories(id),
    discount_pct    INT  NOT NULL DEFAULT 0 CHECK (discount_pct BETWEEN 0 AND 100),
    stock_qty       INT  NOT NULL DEFAULT 0 CHECK (stock_qty >= 0),
    description     TEXT,
    photo           VARCHAR(100)
);

-- ---------- 4. Заказы ----------
CREATE TABLE orders (
    order_num       SERIAL PRIMARY KEY,
    order_date      DATE NOT NULL,
    delivery_date   DATE,
    pickup_point_id INT NOT NULL REFERENCES pickup_points(id),
    user_id         INT REFERENCES users(id),
    pickup_code     VARCHAR(10) NOT NULL,
    status_id       INT NOT NULL REFERENCES order_statuses(id)
);

CREATE TABLE order_items (
    id         SERIAL PRIMARY KEY,
    order_num  INT NOT NULL REFERENCES orders(order_num) ON DELETE CASCADE,
    product_id INT NOT NULL REFERENCES products(id),
    quantity   INT NOT NULL CHECK (quantity > 0)
);

CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_orders_user       ON orders(user_id);
CREATE INDEX idx_orderitems_order  ON order_items(order_num);

-- =============================================================
--                    ДАННЫЕ
-- =============================================================

"""


def main():
    parts = [HEADER]
    parts.append(insert("roles", ["id", "code", "name"], {"id": "int"}))
    parts.append(insert("categories", ["id", "name"], {"id": "int"}))
    parts.append(insert("manufacturers", ["id", "name"], {"id": "int"}))
    parts.append(insert("suppliers", ["id", "name"], {"id": "int"}))
    parts.append(insert("order_statuses", ["id", "code", "name"], {"id": "int"}))
    parts.append(insert("pickup_points", ["id", "address"], {"id": "int"}))
    parts.append(insert("users",
                        ["id", "login", "password_text", "full_name", "role_id"],
                        {"id": "int", "role_id": "int"}))
    parts.append(insert("products",
                        ["id", "article", "name", "unit", "price",
                         "supplier_id", "manufacturer_id", "category_id",
                         "discount_pct", "stock_qty", "description", "photo"],
                        {"id": "int", "price": "numeric", "supplier_id": "int",
                         "manufacturer_id": "int", "category_id": "int",
                         "discount_pct": "int", "stock_qty": "int"}))
    parts.append(insert("orders",
                        ["order_num", "order_date", "delivery_date",
                         "pickup_point_id", "user_id", "pickup_code", "status_id"],
                        {"order_num": "int", "pickup_point_id": "int",
                         "user_id": "int", "status_id": "int"}))
    parts.append(insert("order_items",
                        ["id", "order_num", "product_id", "quantity"],
                        {"id": "int", "order_num": "int",
                         "product_id": "int", "quantity": "int"}))

    parts.append("""-- Сброс автоинкрементов после ручного ввода id
SELECT setval(pg_get_serial_sequence('roles','id'),          (SELECT MAX(id) FROM roles));
SELECT setval(pg_get_serial_sequence('categories','id'),     (SELECT MAX(id) FROM categories));
SELECT setval(pg_get_serial_sequence('manufacturers','id'),  (SELECT MAX(id) FROM manufacturers));
SELECT setval(pg_get_serial_sequence('suppliers','id'),      (SELECT MAX(id) FROM suppliers));
SELECT setval(pg_get_serial_sequence('order_statuses','id'), (SELECT MAX(id) FROM order_statuses));
SELECT setval(pg_get_serial_sequence('pickup_points','id'),  (SELECT MAX(id) FROM pickup_points));
SELECT setval(pg_get_serial_sequence('users','id'),          (SELECT MAX(id) FROM users));
SELECT setval(pg_get_serial_sequence('products','id'),       (SELECT MAX(id) FROM products));
SELECT setval(pg_get_serial_sequence('orders','order_num'),  (SELECT MAX(order_num) FROM orders));
SELECT setval(pg_get_serial_sequence('order_items','id'),    (SELECT MAX(id) FROM order_items));
""")

    OUT.write_text("".join(parts), encoding="utf-8")
    print(f"Wrote {OUT} ({OUT.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
