#!/bin/bash

# TicketWave RabbitMQ Setup Script
echo "🐰 Starting TicketWave RabbitMQ Environment..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

# Stop existing containers if they exist
echo "🛑 Stopping existing RabbitMQ containers..."
docker-compose down

# Remove old volumes (optional - only if you want a fresh start)
# docker volume rm ticketwave_rabbitmq_data

# Start RabbitMQ
echo "🚀 Starting RabbitMQ..."
docker-compose up -d

# Wait for RabbitMQ to be ready
echo "⏳ Waiting for RabbitMQ to be ready..."
sleep 30

# Check health
echo "🔍 Checking RabbitMQ health..."
timeout 60 bash -c 'until docker exec ticketwave-rabbitmq rabbitmq-diagnostics check_running; do echo "Waiting for RabbitMQ..."; sleep 5; done'

if [ $? -eq 0 ]; then
    echo "✅ RabbitMQ is ready!"
    echo ""
    echo "📊 RabbitMQ Management UI: http://localhost:15672"
    echo "👤 Username: admin"
    echo "🔑 Password: admin123"
    echo ""
    echo "🔌 AMQP Connection:"
    echo "   Host: localhost"
    echo "   Port: 5672"
    echo "   Virtual Host: /ticketwave"
    echo ""
    echo "📋 Queues and exchanges have been configured automatically!"
else
    echo "❌ RabbitMQ failed to start properly"
    docker-compose logs rabbitmq
    exit 1
fi