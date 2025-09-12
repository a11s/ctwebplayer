@echo off
echo Downloading JavaScript files...

REM Download remaining JavaScript files
curl -o assets/js/jquery-i18next.min.js "https://game.ero-labs.live/assets/js/jquery-i18next.min.js?v=faeef00234"
curl -o assets/js/sockjs-0.3.4.js "https://game.ero-labs.live/assets/js/sockjs-0.3.4.js?v=34d72f6dff"
curl -o assets/js/stomp.js "https://game.ero-labs.live/assets/js/stomp.js?v=5a574aa321"
curl -o assets/js/main.js "https://game.ero-labs.live/assets/js/main.js?v=5a22e71c50"
curl -o assets/js/hreflang.js "https://game.ero-labs.live/assets/js/hreflang.js?v=8751906214"
curl -o assets/js/appBanner.js "https://game.ero-labs.live/assets/js/appBanner.js?v=7ce73777d8"
curl -o common/js/index.js "https://game.ero-labs.live/common/js/index.js?v=6f97cc174a"
curl -o assets/js/cloud_game/controller_cloud_game.js "https://game.ero-labs.live/assets/js/cloud_game/controller_cloud_game.js?v=32b14976d5"

echo.
echo Creating CSS directories...
md assets\css 2>nul
md common\css 2>nul

echo.
echo Downloading CSS files...
curl -o assets/css/main.css "https://game.ero-labs.live/assets/css/main.css?v=a2c03c09ce"
curl -o assets/css/cloudGame.css "https://game.ero-labs.live/assets/css/cloudGame.css?v=6edc60d882"
curl -o common/css/google_fonts.css "https://game.ero-labs.live/common/css/google_fonts.css?v=107dd5ec63"
curl -o common/css/fontawesome_all.css "https://game.ero-labs.live/common/css/fontawesome_all.css?v=561fa28dd8"

echo.
echo All resources downloaded successfully!