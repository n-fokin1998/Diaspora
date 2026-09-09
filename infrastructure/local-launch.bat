@echo off
setlocal

set "ROOT=%~dp0.."
set "API_DIR=%ROOT%\src\Web\Client.Api"
set "SPA_DIR=%ROOT%\src\Web\Client.Api\spa"
set "COMPOSE_FILE=%~dp0docker\docker-compose.yml"

start "Diaspora DB" cmd /k docker compose -f "%COMPOSE_FILE%" up db pgadmin -d
start "Diaspora Backend" cmd /k cd /d "%API_DIR%" ^& dotnet watch run
start "Diaspora Frontend" cmd /k cd /d "%SPA_DIR%" ^& npm i ^& npm run dev

endlocal
