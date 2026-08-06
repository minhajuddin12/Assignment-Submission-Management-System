const fs = require('fs');
const p = 'src/components/ui/DashboardLayout.tsx';
let s = fs.readFileSync(p, 'utf8');
const before = s;
s = s.replace(/[\u201C\u201D]/g, '"').replace(/[\u2018\u2019]/g, "'");
fs.writeFileSync(p, s);
console.log(before === s ? 'No smart quotes found' : 'Fixed smart quotes and saved');