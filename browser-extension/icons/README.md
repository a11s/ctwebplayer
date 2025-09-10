# 图标文件说明

本文件夹需要包含以下 PNG 格式的图标文件：

- `icon16.png` - 16x16 像素
- `icon48.png` - 48x48 像素  
- `icon128.png` - 128x128 像素

## 生成图标

您可以使用提供的 `icon.svg` 文件生成所需的 PNG 图标：

### 使用在线工具
1. 访问 https://cloudconvert.com/svg-to-png
2. 上传 `icon.svg` 文件
3. 分别设置输出尺寸为 16x16、48x48、128x128
4. 下载并重命名为对应的文件名

### 使用命令行工具（需要安装 ImageMagick）
```bash
# 安装 ImageMagick (Windows)
# 下载地址: https://imagemagick.org/script/download.php#windows

# 生成不同尺寸的 PNG
magick convert icon.svg -resize 16x16 icon16.png
magick convert icon.svg -resize 48x48 icon48.png
magick convert icon.svg -resize 128x128 icon128.png
```

### 使用 Node.js 脚本（需要安装 sharp）
```bash
npm install sharp
node generate-icons.js
```

## 图标设计规范

- 使用渐变色：#667eea 到 #764ba2
- 包含云朵元素（代表云游戏）
- 包含缓存符号（三条横线）
- 底部包含 "CTW" 文字标识
- 背景为圆形，带白色边框