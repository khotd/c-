## Описание проекта
Мой проект — это сервис для управления приютом животных. В нём есть основной функционал для управления животными, приютами, усыновлениями, пользователями.

### Модели проекта
- User.cs — модель пользователя (id, username, email, password_hash, role, permissions)
- Animal.cs — модель животного (id, name, species, breed, age, gender, status, shelter_id)
- Shelter.cs — модель приюта (id, name, address, phone, email, capacity)
- Adoption.cs — модель усыновления (id, animal_id, user_id, adoption_date, status, notes)
- AnimalVolunteer.cs — модель связи животного и волонтера (id, animal_id, user_id, role, assigned_date)

## Технологический стек

### Backend
- **.NET 8.0** — платформа разработки
- **ASP.NET Core** — веб-фреймворк
- **Entity Framework Core 8.0.7** — ORM для работы с базой данных
- **Dapper 2.1.35** — микро-ORM для оптимизации запросов

### База данных и кэширование
- **PostgreSQL 16** — реляционная база данных
- **Redis 7** — in-memory хранилище для кэширования
- **Liquibase** — управление миграциями базы данных

### Аутентификация
- **JWT (JSON Web Tokens)** — токены для аутентификации
- **JwtBearer** — JWT middleware

## Запуск проекта

### Требования
- Docker Desktop
- .NET 8.0 SDK

### Быстрый старт

1. **Клонируйте репозиторий или перейдите в папку проекта:**
   ```bash
   cd path/to/project
   ```

2. **Запустите проект через Docker Compose:**
   ```bash
   docker-compose up --build
   ```

3. **Cтатус контейнеров:**
   ```bash
   docker-compose ps
   ```

4. **Swagger UI:**
   ```
   http://localhost:8081
   ```

### Первый запуск
Тестовые учетные данные:

**Тестовые пользователи:**
- **Admin:** username: `admin`, password: `admin123`
- **User:** username: `testuser`, password: `test123`

### Получение JWT токена

1. Используем endpoint `POST /api/auth/login`:
   ```json
   {
     "username": "admin",
     "password": "admin123"
   }
   ```

2. Копируем `access_token` из ответа

3. В Swagger UI нажимаем кнопку **"Authorize"** и вставляем токен


### Структура проекта

```
project/
├── Controllers/          # API контроллеры
├── Data/                 # DbContext и конфигурация БД
├── Models/
│   ├── Entities/         # Модели данных
│   └── DTO/              # Data Transfer Objects
├── Repositories/         # Слой доступа к данным
├── Services/             # Бизнес-логика
├── Middleware/           # Пользовательские middleware
├── Validators/           # Валидаторы FluentValidation
├── liquibase/            # Миграции базы данных
├── docker-compose.yml    # Конфигурация Docker Compose
├── Dockerfile            # Образ приложения
└── Program.cs            # Точка входа приложения
```

## API Endpoints

### Публичные (не требуют авторизации)
- `GET /api/health` — проверка здоровья сервиса
- `GET /api/animals` — список животных (с пагинацией)
- `GET /api/animals/{id}` — информация о животном
- `GET /api/shelters` — список приютов
- `GET /api/shelters/{id}` — информация о приюте

### Требуют авторизации
- `POST /api/auth/login` — авторизация и получение JWT токена
- Все остальные endpoints требуют JWT токен в заголовке `Authorization: Bearer {token}`

## Роли пользователей

- **Admin** — полный доступ ко всем операциям
- **Manager** — может создавать и обновлять, но не может удалять
- **User** — только чтение и создание запросов на усыновление
