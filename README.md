# ⚙️ Resilient Integration Worker

> Фоновый ETL-сервис для обработки финансовых фидов с защитой от каскадных отказов.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-5126C1?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

---

## 🌟 Ключевые особенности
- **Отказоустойчивость:** Реализованы паттерны **Retry** (с экспоненциальной задержкой) и **Circuit Breaker** через библиотеку Polly.
- **Идемпотентность:** Защита от дубликатов на уровне БД (UNIQUE constraint) и проверка перед вставкой.
- **Контейнеризация:** Полная поддержка Docker Compose с Healthchecks для корректного порядка запуска.
- **Тестирование:** Unit-тесты (xUnit + Moq) для проверки бизнес-логики и политик отказоустойчивости.

---

## 🚀 Как запустить
```bash
# Клонируем репозиторий
git clone https://github.com/andreyrusov2106/resilient-integration-worker.git
cd resilient-integration-worker

# Запускаем через Docker (включая PostgreSQL)
docker-compose up --build
