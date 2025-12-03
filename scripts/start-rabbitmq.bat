@echo off
REM TicketWave RabbitMQ Setup Script for Windows
echo 🐰 Starting TicketWave RabbitMQ Environment...

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not running. Please start Docker first.
    pause
    exit /b 1
)

REM Stop existing containers if they exist
echo 🛑 Stopping existing RabbitMQ containers...
docker-compose down

REM Start RabbitMQ
echo 🚀 Starting RabbitMQ...
docker-compose up -d

REM Wait for RabbitMQ to be ready
echo ⏳ Waiting for RabbitMQ to be ready...
timeout /t 30 /nobreak >nul

REM Check health
echo 🔍 Checking RabbitMQ health...
for /l %%i in (1,1,12) do (
    docker exec ticketwave-rabbitmq rabbitmq-diagnostics check_running >nul 2>&1
    if not errorlevel 1 goto :ready
    echo Waiting for RabbitMQ...
    timeout /t 5 /nobreak >nul
)

echo ❌ RabbitMQ failed to start properly
docker-compose logs rabbitmq
pause
exit /b 1

:ready
echo ✅ RabbitMQ is ready!
echo.
echo 📊 RabbitMQ Management UI: http://localhost:15672
echo 👤 Username: admin
echo 🔑 Password: admin123
echo.
echo 🔌 AMQP Connection:
echo    Host: localhost
echo    Port: 5672
echo    Virtual Host: /ticketwave
echo.
echo 📋 Queues and exchanges have been configured automatically!
pause