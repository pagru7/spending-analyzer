#!/bin/sh
set -e

if [ -z "$ConnectionStrings__DefaultConnection" ]; then
  echo "[Migration] ConnectionStrings__DefaultConnection not provided. Skipping EF Core migrations."
else
  echo "[Migration] Applying EF Core migrations..."
  dotnet ef database update \
    --assembly /app/SpendingAnalyzer.dll \
    --startup-assembly /app/SpendingAnalyzer.dll \
    --content-root /app \
    --data-dir /app \
    --no-build
  echo "[Migration] Migrations applied successfully."
fi

echo "[Startup] Launching SpendingAnalyzer API..."
exec dotnet SpendingAnalyzer.dll
