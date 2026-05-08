-- =============================================================
--   Демо-проект ДЭ КОД 09.02.07 — Интернет-магазин обуви
--   Создание БД, схемы, заполнение данными
-- =============================================================

DROP DATABASE IF EXISTS shop_db;
CREATE DATABASE shop_db ENCODING 'UTF8';
\c shop_db

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

INSERT INTO roles (id, code, name) VALUES
  (1, 'guest', 'Гость'),
  (2, 'client', 'Клиент'),
  (3, 'manager', 'Менеджер'),
  (4, 'admin', 'Администратор');

INSERT INTO categories (id, name) VALUES
  (1, 'Женская обувь'),
  (2, 'Мужская обувь');

INSERT INTO manufacturers (id, name) VALUES
  (1, 'Alessio Nesca'),
  (2, 'CROSBY'),
  (3, 'Kari'),
  (4, 'Marco Tozzi'),
  (5, 'Rieker'),
  (6, 'Рос');

INSERT INTO suppliers (id, name) VALUES
  (1, 'Kari'),
  (2, 'Обувь для вас');

INSERT INTO order_statuses (id, code, name) VALUES
  (1, 'new', 'Новый'),
  (2, 'confirmed', 'Подтверждён'),
  (3, 'paid', 'Оплачен'),
  (4, 'shipped', 'Отправлен'),
  (5, 'done', 'Завершён'),
  (6, 'cancelled', 'Отменён'),
  (7, 'returned', 'Возврат');

INSERT INTO pickup_points (id, address) VALUES
  (1, '420151, г. Лесной, ул. Вишневая, 32'),
  (2, '125061, г. Лесной, ул. Подгорная, 8'),
  (3, '630370, г. Лесной, ул. Шоссейная, 24'),
  (4, '400562, г. Лесной, ул. Зеленая, 32'),
  (5, '614510, г. Лесной, ул. Маяковского, 47'),
  (6, '410542, г. Лесной, ул. Светлая, 46'),
  (7, '620839, г. Лесной, ул. Цветочная, 8'),
  (8, '443890, г. Лесной, ул. Коммунистическая, 1'),
  (9, '603379, г. Лесной, ул. Спортивная, 46'),
  (10, '603721, г. Лесной, ул. Гоголя, 41'),
  (11, '410172, г. Лесной, ул. Северная, 13'),
  (12, '614611, г. Лесной, ул. Молодежная, 50'),
  (13, '454311, г.Лесной, ул. Новая, 19'),
  (14, '660007, г.Лесной, ул. Октябрьская, 19'),
  (15, '603036, г. Лесной, ул. Садовая, 4'),
  (16, '394060, г.Лесной, ул. Фрунзе, 43'),
  (17, '410661, г. Лесной, ул. Школьная, 50'),
  (18, '625590, г. Лесной, ул. Коммунистическая, 20'),
  (19, '625683, г. Лесной, ул. 8 Марта'),
  (20, '450983, г.Лесной, ул. Комсомольская, 26'),
  (21, '394782, г. Лесной, ул. Чехова, 3'),
  (22, '603002, г. Лесной, ул. Дзержинского, 28'),
  (23, '450558, г. Лесной, ул. Набережная, 30'),
  (24, '344288, г. Лесной, ул. Чехова, 1'),
  (25, '614164, г.Лесной,  ул. Степная, 30'),
  (26, '394242, г. Лесной, ул. Коммунистическая, 43'),
  (27, '660540, г. Лесной, ул. Солнечная, 25'),
  (28, '125837, г. Лесной, ул. Шоссейная, 40'),
  (29, '125703, г. Лесной, ул. Партизанская, 49'),
  (30, '625283, г. Лесной, ул. Победы, 46'),
  (31, '614753, г. Лесной, ул. Полевая, 35'),
  (32, '426030, г. Лесной, ул. Маяковского, 44'),
  (33, '450375, г. Лесной ул. Клубная, 44'),
  (34, '625560, г. Лесной, ул. Некрасова, 12'),
  (35, '630201, г. Лесной, ул. Комсомольская, 17'),
  (36, '190949, г. Лесной, ул. Мичурина, 26');

INSERT INTO users (id, login, password_text, full_name, role_id) VALUES
  (1, '94d5ous@gmail.com', 'uzWC67', 'Никифорова Весения Николаевна', 4),
  (2, 'uth4iz@mail.com', '2L6KZG', 'Сазонов Руслан Германович', 4),
  (3, 'yzls62@outlook.com', 'JlFRCZ', 'Одинцов Серафим Артёмович', 4),
  (4, '1diph5e@tutanota.com', '8ntwUp', 'Степанов Михаил Артёмович', 3),
  (5, 'tjde7c@yahoo.com', 'YOyhfR', 'Ворсин Петр Евгеньевич', 3),
  (6, 'wpmrc3do@tutanota.com', 'RSbvHv', 'Старикова Елена Павловна', 3),
  (7, '5d4zbu@tutanota.com', 'rwVDh9', 'Михайлюк Анна Вячеславовна', 2),
  (8, 'ptec8ym@yahoo.com', 'LdNyos', 'Ситдикова Елена Анатольевна', 2),
  (9, '1qz4kw@mail.com', 'gynQMT', 'Ворсин Петр Евгеньевич', 2),
  (10, '4np6se@mail.com', 'AtnDjr', 'Старикова Елена Павловна', 2);

INSERT INTO products (id, article, name, unit, price, supplier_id, manufacturer_id, category_id, discount_pct, stock_qty, description, photo) VALUES
  (1, 'А112Т4', 'Ботинки', 'шт.', 4990, 1, 3, 1, 3, 6, 'Женские Ботинки демисезонные kari', '1.jpg'),
  (2, 'F635R4', 'Ботинки', 'шт.', 3244, 2, 4, 1, 2, 13, 'Ботинки Marco Tozzi женские демисезонные, размер 39, цвет бежевый', '2.jpg'),
  (3, 'H782T5', 'Туфли', 'шт.', 4499, 1, 3, 2, 4, 5, 'Туфли kari мужские классика MYZ21AW-450A, размер 43, цвет: черный', '3.jpg'),
  (4, 'G783F5', 'Ботинки', 'шт.', 5900, 1, 6, 2, 2, 8, 'Мужские ботинки Рос-Обувь кожаные с натуральным мехом', '4.jpg'),
  (5, 'J384T6', 'Ботинки', 'шт.', 3800, 2, 5, 2, 2, 16, 'B3430/14 Полуботинки мужские Rieker', '5.jpg'),
  (6, 'D572U8', 'Кроссовки', 'шт.', 4100, 2, 6, 2, 3, 6, '129615-4 Кроссовки мужские', '6.jpg'),
  (7, 'F572H7', 'Туфли', 'шт.', 2700, 1, 4, 1, 2, 14, 'Туфли Marco Tozzi женские летние, размер 39, цвет черный', '7.jpg'),
  (8, 'D329H3', 'Полуботинки', 'шт.', 1890, 2, 1, 1, 4, 4, 'Полуботинки Alessio Nesca женские 3-30797-47, размер 37, цвет: бордовый', '8.jpg'),
  (9, 'B320R5', 'Туфли', 'шт.', 4300, 1, 5, 1, 2, 6, 'Туфли Rieker женские демисезонные, размер 41, цвет коричневый', '9.jpg'),
  (10, 'G432E4', 'Туфли', 'шт.', 2800, 1, 3, 1, 3, 15, 'Туфли kari женские TR-YR-413017, размер 37, цвет: черный', '10.jpg'),
  (11, 'S213E3', 'Полуботинки', 'шт.', 2156, 2, 2, 2, 3, 6, '407700/01-01 Полуботинки мужские CROSBY', NULL),
  (12, 'E482R4', 'Полуботинки', 'шт.', 1800, 1, 3, 1, 2, 14, 'Полуботинки kari женские MYZ20S-149, размер 41, цвет: черный', NULL),
  (13, 'S634B5', 'Кеды', 'шт.', 5500, 2, 2, 2, 3, 0, 'Кеды Caprice мужские демисезонные, размер 42, цвет черный', NULL),
  (14, 'K345R4', 'Полуботинки', 'шт.', 2100, 2, 2, 2, 2, 3, '407700/01-02 Полуботинки мужские CROSBY', NULL),
  (15, 'O754F4', 'Туфли', 'шт.', 5400, 2, 5, 1, 4, 18, 'Туфли женские демисезонные Rieker артикул 55073-68/37', NULL),
  (16, 'G531F4', 'Ботинки', 'шт.', 6600, 1, 3, 1, 12, 9, 'Ботинки женские зимние ROMER арт. 893167-01 Черный', NULL),
  (17, 'J542F5', 'Тапочки', 'шт.', 500, 1, 3, 2, 13, 0, 'Тапочки мужские Арт.70701-55-67син р.41', NULL),
  (18, 'B431R5', 'Ботинки', 'шт.', 2700, 2, 5, 2, 2, 5, 'Мужские кожаные ботинки/мужские ботинки', NULL),
  (19, 'P764G4', 'Туфли', 'шт.', 6800, 1, 2, 1, 15, 15, 'Туфли женские, ARGO, размер 38', NULL),
  (20, 'C436G5', 'Ботинки', 'шт.', 10200, 1, 1, 1, 15, 9, 'Ботинки женские, ARGO, размер 40', NULL),
  (21, 'F427R5', 'Ботинки', 'шт.', 11800, 2, 5, 1, 15, 11, 'Ботинки на молнии с декоративной пряжкой FRAU', NULL),
  (22, 'N457T5', 'Полуботинки', 'шт.', 4600, 1, 2, 1, 3, 13, 'Полуботинки Ботинки черные зимние, мех', NULL),
  (23, 'D364R4', 'Туфли', 'шт.', 12400, 1, 3, 1, 16, 5, 'Туфли Luiza Belly женские Kate-lazo черные из натуральной замши', NULL),
  (24, 'S326R5', 'Тапочки', 'шт.', 9900, 2, 2, 2, 17, 15, 'Мужские кожаные тапочки "Профиль С.Дали"', NULL),
  (25, 'L754R4', 'Полуботинки', 'шт.', 1700, 1, 3, 1, 2, 7, 'Полуботинки kari женские WB2020SS-26, размер 38, цвет: черный', NULL),
  (26, 'M542T5', 'Кроссовки', 'шт.', 2800, 2, 5, 2, 18, 3, 'Кроссовки мужские TOFA', NULL),
  (27, 'D268G5', 'Туфли', 'шт.', 4399, 2, 5, 1, 3, 12, 'Туфли Rieker женские демисезонные, размер 36, цвет коричневый', NULL),
  (28, 'T324F5', 'Сапоги', 'шт.', 4699, 1, 2, 1, 2, 5, 'Сапоги замша Цвет: синий', NULL),
  (29, 'K358H6', 'Тапочки', 'шт.', 599, 1, 5, 2, 20, 2, 'Тапочки мужские син р.41', NULL),
  (30, 'H535R5', 'Ботинки', 'шт.', 2300, 2, 5, 1, 2, 7, 'Женские Ботинки демисезонные', NULL);

INSERT INTO orders (order_num, order_date, delivery_date, pickup_point_id, user_id, pickup_code, status_id) VALUES
  (1, '2025-02-27', '2025-04-20', 1, 4, '901', 5),
  (2, '2022-09-28', '2025-04-21', 11, 1, '902', 5),
  (3, '2025-03-21', '2025-04-22', 2, 2, '903', 5),
  (4, '2025-02-20', '2025-04-23', 11, 3, '904', 5),
  (5, '2025-03-17', '2025-04-24', 2, 4, '905', 5),
  (6, '2025-03-01', '2025-04-25', 15, 1, '906', 5),
  (7, '2025-02-28', '2025-04-26', 3, 2, '907', 5),
  (8, '2025-03-31', '2025-04-27', 19, 3, '908', 1),
  (9, '2025-04-02', '2025-04-28', 5, 4, '909', 1),
  (10, '2025-04-03', '2025-04-29', 19, 4, '910', 1);

INSERT INTO order_items (id, order_num, product_id, quantity) VALUES
  (1, 1, 1, 2),
  (2, 1, 2, 2),
  (3, 2, 3, 1),
  (4, 2, 4, 1),
  (5, 3, 5, 10),
  (6, 3, 6, 10),
  (7, 4, 7, 5),
  (8, 4, 8, 4),
  (9, 5, 1, 2),
  (10, 5, 2, 2),
  (11, 6, 3, 1),
  (12, 6, 4, 1),
  (13, 7, 5, 10),
  (14, 7, 6, 10),
  (15, 8, 7, 5),
  (16, 8, 8, 4),
  (17, 9, 9, 5),
  (18, 9, 10, 1),
  (19, 10, 11, 5),
  (20, 10, 12, 5);

-- Сброс автоинкрементов после ручного ввода id
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

-- Удобные тестовые пользователи (чтобы не вбивать длинные email'ы)
INSERT INTO users (login, password_text, full_name, role_id) VALUES
  ('admin',   'admin',   'Администратор Тестовый', 4),
  ('manager', 'manager', 'Менеджер Тестовый',      3),
  ('client',  'client',  'Клиент Тестовый',        2);
