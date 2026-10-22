# Mẫu số liệu ghép nhóm: số trung bình, trung vị, mốt

## Mục tiêu học tập
Sau bài học này, người học có thể:
- Hiểu cách biểu diễn một mẫu số liệu ghép nhóm theo các khoảng lớp.
- Xác định tần số, giá trị đại diện và cỡ mẫu của mẫu số liệu ghép nhóm.
- Tính số trung bình, trung vị và mốt của số liệu ghép nhóm.
- Vận dụng công thức để giải một bài toán thực tế đơn giản.

## 1. Khái niệm về mẫu số liệu ghép nhóm

Mẫu số liệu ghép nhóm chia các giá trị thành các nhóm dạng $[a_i; a_{i+1})$.

Trong mỗi nhóm:
- **Tần số** $m_i$ là số giá trị rơi vào nhóm đó.
- **Giá trị đại diện** của nhóm là
  $\displaystyle c_i = \frac{a_i + a_{i+1}}{2}$.

Tổng cỡ mẫu được tính bởi:

$\displaystyle n = m_1 + m_2 + \dots + m_k$.

## 2. Các công thức cơ bản

### 2.1. Số trung bình

Số trung bình của mẫu số liệu ghép nhóm được tính theo công thức:

$\displaystyle \bar{x} = \frac{m_1c_1 + m_2c_2 + \dots + m_kc_k}{n}$.

### 2.2. Trung vị

Gọi $[a_p; a_{p+1})$ là nhóm đầu tiên có tần số tích lũy lớn hơn hoặc bằng $\frac{n}{2}$, khi đó trung vị là:

$$M_e = a_p + \frac{\frac{n}{2} - (m_1 + \dots + m_{p-1})}{m_p}\cdot (a_{p+1} - a_p).$$

### 2.3. Mốt

Gọi $[a_j; a_{j+1})$ là nhóm có tần số lớn nhất, khi đó mốt là:

$$M_o = a_j + \frac{m_j - m_{j-1}}{(m_j - m_{j-1}) + (m_j - m_{j+1})}\cdot (a_{j+1} - a_j).$$

## 3. Ví dụ minh họa

Xét thống kê tốc độ (km/h) của 42 ô tô khi qua một trạm đo:

| Nhóm | Tần số | Giá trị đại diện | Tần số tích lũy |
| --- | --- | --- | --- |
| $[40;45)$ | 5 | 42,5 | 5 |
| $[45;50)$ | 10 | 47,5 | 15 |
| $[50;55)$ | 7 | 52,5 | 22 |
| $[55;60)$ | 9 | 57,5 | 31 |
| $[60;65)$ | 7 | 62,5 | 38 |
| $[65;70)$ | 4 | 67,5 | 42 |

### a) Số xe vi phạm tốc độ tối đa 60 km/h

Quy định tốc độ tối đa là 60 km/h. Các xe vi phạm thuộc các nhóm có tốc độ từ 60 km/h trở lên, nên số xe vi phạm là:

$7 + 4 = 11$ xe.

### b) Tính số trung bình

Áp dụng công thức số trung bình:

$\displaystyle \bar{x} = \frac{5\cdot 42{,}5 + 10\cdot 47{,}5 + 7\cdot 52{,}5 + 9\cdot 57{,}5 + 7\cdot 62{,}5 + 4\cdot 67{,}5}{42}$

$\displaystyle = \frac{2280}{42} \approx 54{,}3$ (km/h).

### c) Tính trung vị

Vì $\dfrac{n}{2} = 21$, nên xét tần số tích lũy để tìm nhóm đầu tiên có tần số tích lũy lớn hơn hoặc bằng 21.

Nhóm $[50;55)$ là nhóm đầu tiên có tần số tích lũy $22 \ge 21$, do đó:

$\displaystyle M_e = 50 + \frac{21 - 15}{7}\cdot 5 \approx 54{,}3$.

### d) Tính mốt

Nhóm $[45;50)$ có tần số lớn nhất là 10, nên đây là nhóm mốt. Khi đó:

$\displaystyle M_o = 45 + \frac{10 - 5}{(10 - 5) + (10 - 7)}\cdot 5 \approx 48{,}1$.

## 4. Lưu ý khi sử dụng các đại lượng đặc trưng

- Số trung bình chịu ảnh hưởng của mọi giá trị trong mẫu.
- Trung vị ít bị ảnh hưởng bởi giá trị ngoại lai.
- Khi tính trung vị và mốt, cần kiểm tra các nhóm có độ rộng bằng nhau.

## 5. Tóm tắt và điểm cần nhớ

- Mẫu số liệu ghép nhóm được chia thành các khoảng $[a_i; a_{i+1})$.
- Tần số $m_i$ cho biết số giá trị trong mỗi nhóm, còn giá trị đại diện là $c_i = \dfrac{a_i + a_{i+1}}{2}$.
- Số trung bình: $\displaystyle \bar{x} = \frac{\sum m_i c_i}{n}$.
- Trung vị được xác định từ nhóm có tần số tích lũy đầu tiên đạt $\dfrac{n}{2}$.
- Mốt là giá trị đặc trưng của nhóm có tần số lớn nhất.
- Số trung bình nhạy với mọi giá trị, còn trung vị ít bị ảnh hưởng bởi ngoại lai.