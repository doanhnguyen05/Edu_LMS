#!/bin/bash

# EduLMS - Startup Script
# Script để khởi động dự án EduLMS với tất cả dịch vụ cần thiết

set -e  # Exit on error

echo "🚀 EduLMS Startup Script"
echo "========================"
echo ""

# 1. Khởi động MySQL
echo "1️⃣  Starting MySQL..."
if brew services start mysql 2>/dev/null; then
    echo "✅ MySQL started"
else
    echo "⚠️  MySQL is already running or failed to start"
fi
sleep 2

# 2. Kiểm tra/Tạo Database
echo ""
echo "2️⃣  Checking/Creating EduLMS database..."
mysql -u root -p07012005 -e "CREATE DATABASE IF NOT EXISTS EduLMS;" 2>/dev/null && echo "✅ Database ready" || echo "⚠️  Database already exists"
sleep 1

# 3. Chạy ứng dụng
echo ""
echo "3️⃣  Starting ASP.NET Core application..."
echo "   🌐 Local: http://localhost:5000"
echo "   🔒 HTTPS: https://localhost:5001"
echo "   📊 Database: MySQL on localhost:3306"
echo ""
echo "⏳ Starting application (Ctrl+C to stop)..."
sleep 2

cd "$(dirname "$0")/EduLMS.Web"
dotnet run
