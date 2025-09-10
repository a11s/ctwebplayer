@echo off
echo Downloading JavaScript files...

REM Download remaining JavaScript files
curl -o docs/assets/js/jquery-i18next.min.js "https://game.ero-labs.live/assets/js/jquery-i18next.min.js?v=faeef00234"
curl -o docs/assets/js/sockjs-0.3.4.js "https://game.ero-labs.live/assets/js/sockjs-0.3.4.js?v=34d72f6dff"
curl -o docs/assets/js/stomp.js "https://game.ero-labs.live/assets/js/stomp.js?v=5a574aa321"
curl -o docs/assets/js/main.js "https://game.ero-labs.live/assets/js/main.js?v=5a22e71c50"
curl -o docs/assets/js/hreflang.js "https://game.ero-labs.live/assets/js/hreflang.js?v=8751906214"
curl -o docs/assets/js/appBanner.js "https://game.ero-labs.live/assets/js/appBanner.js?v=7ce73777d8"
curl -o docs/common/js/index.js "https://game.ero-labs.live/common/js/index.js?v=6f97cc174a"
curl -o docs/assets/js/cloud_game/controller_cloud_game.js "https://game.ero-labs.live/assets/js/cloud_game/controller_cloud_game.js?v=32b14976d5"

echo.
echo Creating CSS directories...
md docs\assets\css 2>nul
md docs\common\css 2>nul

echo.
echo Downloading CSS files...
curl -o docs/assets/css/main.css "https://game.ero-labs.live/assets/css/main.css?v=a2c03c09ce"
curl -o docs/assets/css/cloudGame.css "https://game.ero-labs.live/assets/css/cloudGame.css?v=6edc60d882"
curl -o docs/common/css/google_fonts.css "https://game.ero-labs.live/common/css/google_fonts.css?v=107dd5ec63"
curl -o docs/common/css/fontawesome_all.css "https://game.ero-labs.live/common/css/fontawesome_all.css?v=561fa28dd8"

echo.
echo All resources downloaded successfully!