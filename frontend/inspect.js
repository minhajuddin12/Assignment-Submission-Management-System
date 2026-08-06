const fs = require('fs');
const p = 'src/components/ui/DashboardLayout.tsx';
const s = fs.readFileSync(p, 'utf8');
const lines = s.split('\n');

for (let i = 29; i < 55 && i < lines.length; i++) {
  const line = lines[i];
  let flagged = '';
  for (const ch of line) {
    const code = ch.codePointAt(0);
    if (code > 127) {
      flagged += ` [char="${ch}" code=U+${code.toString(16).toUpperCase()}]`;
    }
  }
  console.log(`${i + 1}: ${JSON.stringify(line)}${flagged}`);
}