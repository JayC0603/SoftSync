/* Gộp toàn bộ game thành 1 file HTML duy nhất (chạy offline, mở bằng double-click).
 * Dùng: node tools/build.js   ->  dist/day-tetris.html  (+ dist/artifact.html cho nền tảng nhúng) */
const fs = require('fs'), path = require('path');
const root = path.join(__dirname, '..');
const read = (p) => fs.readFileSync(path.join(root, p), 'utf8');
const html = read('index.html');
const body = html.split('<!--BODY-->')[1].split('<!--/BODY-->')[0];
const css = read('css/style.css');
const scripts = ['config', 'game', 'report', 'render', 'main'].map((n) => read('js/' + n + '.js').replace(/<\/script>/g, '<\\/script>')).join('\n');
const fonts = '<link rel="preconnect" href="https://fonts.googleapis.com">\n<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>\n' +
  '<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@400;500;600;700&family=Bricolage+Grotesque:opsz,wght@12..96,600;12..96,800&family=JetBrains+Mono:wght@500;600;700&display=swap">';
const title = '<title>Day Tetris</title>';
fs.mkdirSync(path.join(root, 'dist'), { recursive: true });
fs.writeFileSync(path.join(root, 'dist/day-tetris.html'),
  '<!doctype html>\n<html lang="vi">\n<head>\n<meta charset="utf-8">\n<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">\n' +
  title + '\n' + fonts + '\n<style>\n' + css + '\n</style>\n</head>\n<body>' + body + '<script>\n' + scripts + '\n</script>\n</body>\n</html>\n');
// Bản không có khung <html>/<head>/<body> (dùng cho nền tảng tự bọc khung trang)
fs.writeFileSync(path.join(root, 'dist/artifact.html'),
  title + '\n<meta name="claude:viewport-fit-sides" content="cover">\n' + fonts + '\n<style>\n' + css + '\n</style>\n' + body + '<script>\n' + scripts + '\n</script>\n');
console.log('Đã build dist/day-tetris.html và dist/artifact.html');
