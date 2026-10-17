@echo off
chcp 65001 > nul
title Sky Hopper 웹 게임
rem 더블클릭하면 Builds\WebGL 폴더를 내 컴퓨터 안의 웹 서버로 열고 브라우저를 띄웁니다.
rem 이 창을 닫으면 웹 서버도 꺼집니다.

cd /d "%~dp0Builds\WebGL" 2> nul
if not exist index.html (
    echo [오류] 웹 빌드가 없습니다.
    echo Unity에서 Sky Hopper ^> 빌드 ^> 웹 빌드 ^(WebGL^) 를 먼저 실행해 주세요.
    echo.
    pause
    exit /b 1
)

where python > nul 2> nul
if errorlevel 1 (
    echo [오류] Python을 찾을 수 없습니다. https://www.python.org 에서 설치해 주세요.
    echo.
    pause
    exit /b 1
)

echo ============================================
echo   Sky Hopper 웹 게임을 실행합니다
echo   주소: http://localhost:8000
echo   게임을 다 했으면 이 창을 닫으세요.
echo ============================================

rem 서버가 켜질 시간을 2초 준 뒤 브라우저 열기
start "" /b cmd /c "timeout /t 2 /nobreak > nul && start "" http://localhost:8000"
python -m http.server 8000 --bind 127.0.0.1
