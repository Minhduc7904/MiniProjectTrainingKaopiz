// Đọc JSON mảng chuỗi công thức từ stdin, in ra {total, ok, failed:[{tex,error}]} bằng KaTeX của frontend.
// Chạy bằng Node >= 22: node katex_check.mjs < formulas.json
import { createRequire } from "node:module";
import { readFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const require = createRequire(path.join(here, "../../frontend/lms-web/package.json"));
const katex = require("katex");

const items = JSON.parse(readFileSync(0, "utf8"));
const failed = [];
let ok = 0;
for (const it of items) {
  try {
    katex.renderToString(it.tex, { throwOnError: true, displayMode: !!it.display });
    ok++;
  } catch (e) {
    failed.push({ tex: it.tex, error: String(e.message).slice(0, 200) });
  }
}
console.log(JSON.stringify({ total: items.length, ok, failed }));
