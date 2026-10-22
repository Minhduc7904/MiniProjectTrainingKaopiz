# Nguyên hàm của hàm lũy thừa

## Mục tiêu học tập
Sau bài học này, người học có thể:
- Nêu được công thức nguyên hàm cơ bản của hàm lũy thừa.
- Xử lý đúng trường hợp đặc biệt khi số mũ bằng $-1$.
- Áp dụng công thức để tìm nguyên hàm của một số hàm số đơn giản.

## 1. Công thức cơ bản

Với số thực $a \neq -1$, ta có:

$$\int x^a\,dx = \frac{x^{a+1}}{a+1} + C$$

Trong đó $C$ là hằng số tùy ý.

### Trường hợp đặc biệt $a = -1$

Khi $a = -1$, hàm số trở thành $\frac{1}{x}$. Khi đó:

$$\int \frac{1}{x}\,dx = \ln|x| + C$$

## 2. Ví dụ minh họa

Từ công thức trên, ta có các nguyên hàm sau:

- $\int x^3\,dx = \frac{x^4}{4} + C$
- $\int \frac{1}{x^2}\,dx = -\frac{1}{x} + C$
- $\int \sqrt{x}\,dx = \frac{2}{3}x\sqrt{x} + C$

## 3. Bài tập tự luyện

Hãy tìm nguyên hàm của các hàm số sau:

1. $x^5$
2. $3x^2 - 2x + 1$
3. $\frac{1}{x^3}$
4. $(x + 1)^2$

## 4. Đáp án

- $\int x^5\,dx = \frac{x^6}{6} + C$
- $\int (3x^2 - 2x + 1)\,dx = x^3 - x^2 + x + C$
- $\int \frac{1}{x^3}\,dx = -\frac{1}{2x^2} + C$
- $\int (x + 1)^2\,dx = \frac{(x + 1)^3}{3} + C$

## 5. Tóm tắt và điểm cần nhớ

- Với $a \neq -1$, nguyên hàm của $x^a$ là $\frac{x^{a+1}}{a+1} + C$.
- Trường hợp $a = -1$ cần dùng công thức đặc biệt: $\int \frac{1}{x}\,dx = \ln|x| + C$.
- Công thức này giúp giải nhanh nhiều bài toán nguyên hàm của hàm lũy thừa và các biểu thức đa thức đơn giản.