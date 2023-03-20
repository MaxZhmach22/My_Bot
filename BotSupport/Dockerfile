# Используйте образ .NET Core в качестве базового образа
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base

# Устанавливаем метаданные образа
LABEL author="Max_Bot"
LABEL description="Docker image for BotSupport"

# Копируем бинарный файл приложения в образ
COPY bin/Release/net7.0/ /app

# Устанавливаем рабочую директорию
WORKDIR /app

# Задаем команду для запуска приложения
CMD ["dotnet", "BotSupport.dll"]