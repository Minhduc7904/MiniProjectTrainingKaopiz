# Nguyên hàm hàm lũy thừa và các dạng bài tập cơ bản

## Mục tiêu bài học
Sau bài học này, người học có thể:
- Nhận biết và áp dụng công thức nguyên hàm của hàm lũy thừa dạng $(ax+b)^\alpha$.
- Tính nguyên hàm các hàm chứa căn thức, phân thức đơn giản và các biểu thức biến đổi tương đương.
- Giải các bài toán xác định hằng số nguyên hàm từ điều kiện ban đầu.
- Vận dụng nguyên hàm vào các bài toán thực tế và các bài toán cho đạo hàm.

## 1. Công thức nguyên hàm cơ bản

### 1.1. Dạng $(ax+b)^\alpha$
Công thức sử dụng thường xuyên là:

$$
\int (ax+b)^\alpha\,dx = \frac{1}{a}\cdot \frac{(ax+b)^{\alpha+1}}{\alpha+1} + C
$$

với $\alpha \ne -1$.

### 1.2. Một công thức đặc biệt
Với hàm số dạng nghịch đảo bình phương:

$$
\int \frac{1}{(ax+b)^2}\,dx = -\frac{1}{a}\cdot \frac{1}{ax+b} + C
$$

## 2. Áp dụng công thức vào hàm lũy thừa

### 2.1. Ví dụ minh họa
Ta có:

$$
\int (3x-1)^3\,dx
$$

Đây là dạng $(ax+b)^\alpha$ nên có thể áp dụng trực tiếp công thức ở trên.

### 2.2. Một số bài tập tiêu biểu
Các bài như:
- $\int (3-x)^3\,dx$
- $\int (2x+1)^5\,dx$
- $\int (2x-10)^{2018}\,dx$
- $\int (3-4x)^{2019}\,dx$

đều là các bài toán cùng dạng: đưa về công thức nguyên hàm của hàm lũy thừa có biểu thức bậc nhất ở trong ngoặc.

## 3. Bài toán tìm nguyên hàm có điều kiện

Nhiều bài yêu cầu tìm nguyên hàm $F(x)$ thỏa mãn một giá trị tại một điểm, ví dụ:
- $F\left(\frac{1}{2}\right)=4$
- $F\left(-\frac{1}{2}\right)=4$
- $F(0)=\frac{1}{3}$

### Cách làm chung
1. Tìm họ nguyên hàm tổng quát $F(x)$.
2. Dùng điều kiện đã cho để xác định hằng số $C$.
3. Tính giá trị cần tìm tại điểm khác.

### Ví dụ về cách xử lí
Với $f(x)=(2x+1)^3$, nếu biết một nguyên hàm $F(x)$ và $F\left(\frac{1}{2}\right)=4$, ta sẽ:
- tính công thức tổng quát của $F(x)$;
- thay $x=\frac{1}{2}$ để tìm $C$;
- sau đó tính $F\left(\frac{3}{2}\right)$.

## 4. Nguyên hàm của hàm chứa căn thức

### 4.1. Dạng cơ bản
Một số bài có dạng:

$$
\int \sqrt{ax+b}\,dx
$$

hoặc tương đương với:
- $\int \sqrt{3x+2}\,dx$
- $\int \sqrt{5-3x}\,dx$
- $\int \sqrt{x}\,dx$
- $\int \sqrt{2x-1}\,dx$

Các bài này đều thuộc dạng thay biến trực tiếp từ công thức lũy thừa với số mũ $\frac{1}{2}$.

### 4.2. Bài toán xác định hằng số
Ví dụ:
- $f(x)=\sqrt{x}$, biết $F(4)=\frac{19}{3}$, hãy tính $F(9)$.
- $f(x)=\sqrt{2x-1}$, biết $F(1)=\frac{4}{3}$, hãy tính $F(5)$.
- $f(x)=\sqrt{4x-5}$, biết $F\left(\frac{9}{4}\right)=2$, hãy tính $6F\left(\frac{7}{2}\right)$.

Với mỗi bài, ý tưởng vẫn là tìm nguyên hàm tổng quát rồi dùng điều kiện đầu để xác định hằng số.

## 5. Ứng dụng vào mô hình thực tế

### 5.1. Mực nước bồn chứa
Có bài toán cho tốc độ thay đổi mực nước:

$$
h'(t)=\sqrt{3t+8}
$$

Khi biết lúc đầu bồn không có nước, ta tìm $h(t)$ bằng cách lấy nguyên hàm của $h'(t)$ rồi dùng điều kiện ban đầu.

### 5.2. Bể bơi có độ sâu cho trước
Với bài toán bể bơi:
- độ sâu $h_1=280\,\text{cm}$,
- $h'(t)=\frac{1}{500}\sqrt{t+3}$,
- lúc đầu chưa có nước,

ta cần tìm thời điểm nước đạt $\frac{3}{4}$ độ sâu của bể bằng cách tích phân tốc độ tăng theo thời gian.

### 5.3. Dân số và vi khuẩn
Các mô hình tương tự xuất hiện trong bài:
- dân số thị trấn với $f'(t)=-\frac{34}{(t+2)^2}$,
- số lượng vi khuẩn với $B'(t)=\frac{1000}{(1+0,3t)^2}$.

Cách giải vẫn là:
1. Tìm hàm số gốc từ đạo hàm đã cho.
2. Dùng giá trị ban đầu để xác định hằng số.
3. Tính giá trị tại thời điểm yêu cầu.

## 6. Các nguyên hàm phân thức thường gặp

### 6.1. Dạng $\frac{1}{(ax+b)^n}$
Một số công thức và bài tập tiêu biểu:

- $\int \frac{1}{(2x+1)^3}\,dx$
- $\int \frac{1}{(2x-1)^2}\,dx$
- $\int \frac{1}{(3x+1)^4}\,dx$
- $\int \frac{1}{(3-2x)^2}\,dx$
- $\int \frac{1}{(4x-1)^3}\,dx$

### 6.2. Dạng đặc biệt $\frac{1}{4x^2+4x+1}$
Ta nhận thấy:

$$
4x^2+4x+1=(2x+1)^2
$$

nên bài toán trở thành nguyên hàm của một biểu thức dạng nghịch đảo bình phương.

### 6.3. Dạng có tử là đa thức
Một ví dụ điển hình là:

$$
\int \frac{x}{(x+1)^3}\,dx
$$

hoặc

$$
\int \frac{x^2+3x+1}{\sqrt{x}}\,dx
$$

Khi gặp các biểu thức này, thường cần biến đổi đại số trước khi tích phân.

## 7. Dạng nguyên hàm với căn ở mẫu

### 7.1. Một số bài tiêu biểu
- $\int \frac{1}{\sqrt{3x-2}}\,dx$
- $\int \frac{1}{2\sqrt{2x-1}}\,dx$
- $\int \frac{2}{\sqrt{4x-1}}\,dx$
- $\int \frac{1}{\sqrt{3x-1}}\,dx$
- $\int \frac{1}{\sqrt{1-2x}}\,dx$

Dạng này cũng thường được giải bằng cách nhận ra số mũ $-\frac{1}{2}$ của biểu thức bậc nhất bên trong căn.

## 8. Dạng bài phối hợp hai căn thức

Một nhóm bài nâng cao trong tài liệu có dạng:

$$
\int \frac{1}{\sqrt{x+2}+\sqrt{x+1}}\,dx
$$

hoặc

$$
\int \frac{1}{(x+1)\sqrt{x}-x\sqrt{x+1}}\,dx
$$

hoặc

$$
\int \frac{1}{(x+2)\sqrt{x+1}+(x+1)\sqrt{x+2}}\,dx
$$

Các bài này thường yêu cầu:
- đưa mẫu số về dạng thuận lợi hơn bằng phép nhân liên hợp hoặc nhận dạng đạo hàm phù hợp;
- biểu diễn nguyên hàm dưới dạng tổ hợp của các biểu thức như $(x+1)\sqrt{x+1}$, $(x+2)\sqrt{x+2}$, $\sqrt{x}$, $\sqrt{x+1}$, ...;
- xác định hằng số từ điều kiện ban đầu.

## 9. Bài toán cho đạo hàm của một hàm số

### 9.1. Dạng cho $f'(x)$ và yêu cầu tìm $F(x)$
Tài liệu có các bài như:
- $f'(x)=12x+2$, $f(1)=3$;
- $f'(x)=24x^2-18x+8$, $f(1)=2$;
- $f'(x)=-\frac{1}{x^2}+2$, $f(2)=\frac{9}{2}$.

Quy trình làm bài:
1. Tìm $f(x)$ từ $f'(x)$ và điều kiện đã cho.
2. Tìm nguyên hàm $F(x)$ của $f(x)$.
3. Dùng dữ kiện của $F$ để tính giá trị cần hỏi.

### 9.2. Hàm số từng đoạn
Có các bài với hàm số từng đoạn như:

$$
f(x)=\begin{cases} 2x-1 & \text{khi } x \ge 1 \\ 3x^2-2 & \text{khi } x<1 \end{cases}
$$

hoặc

$$
f(x)=\begin{cases} \sqrt{x+4} & \text{khi } x \ge 1 \\ 2x+3 & \text{khi } x < 1 \end{cases}
$$

Khi đó, cần xét nguyên hàm trên từng khoảng và dùng điều kiện để ghép thành một hàm nguyên hàm xác định trên $\mathbb{R}$.

## 10. Bảng tóm tắt các dạng bài trong tài liệu

| Dạng bài | Ý tưởng chính |
|---|---|
| $(ax+b)^\alpha$ | Áp dụng công thức nguyên hàm trực tiếp |
| $\sqrt{ax+b}$ | Viết dưới dạng lũy thừa và tích phân |
| $\frac{1}{(ax+b)^n}$ | Dùng công thức lũy thừa âm |
| Biểu thức có căn ở mẫu | Biến đổi đại số, thường dùng liên hợp |
| Bài có điều kiện tại một điểm | Tìm hằng số $C$ từ dữ kiện |
| Bài thực tế | Từ tốc độ thay đổi suy ra hàm số bằng nguyên hàm |
| Bài cho đạo hàm | Tích phân ngược để tìm hàm gốc |

## 11. Luyện tập định hướng
Khi gặp bài trong nhóm này, hãy tự hỏi:
1. Biểu thức có thể đưa về dạng $(ax+b)^\alpha$ không?
2. Có cần khai triển hay biến đổi đại số trước khi tích phân không?
3. Có điều kiện tại một điểm để tìm hằng số không?
4. Có phải bài mô hình thực tế, cần tích phân theo thời gian không?
5. Có cần xét từng khoảng nếu hàm số cho theo từng đoạn không?

## Tóm tắt và ghi nhớ
- Công thức trung tâm là $\int (ax+b)^\alpha\,dx$ và các biến thể của nó.
- Nhiều bài chỉ khác nhau ở cách thay biến hoặc cách xác định hằng số từ điều kiện ban đầu.
- Các dạng thường gặp gồm: lũy thừa, căn thức, phân thức, biểu thức có căn ở mẫu, mô hình thực tế và bài toán ngược từ đạo hàm.
- Làm bài hiệu quả bằng cách: nhận dạng dạng nguyên hàm, biến đổi hợp lí, tích phân, rồi dùng điều kiện để tìm hằng số.