# Mẫu số liệu ghép nhóm: số trung bình, trung vị, mốt

## 1. Khái niệm

Mẫu số liệu ghép nhóm chia các giá trị thành các nhóm dạng $[a_i; a_{i+1})$. Mỗi nhóm có **tần số** $m_i$ là số giá trị
rơi vào nhóm, và **giá trị đại diện** $c_i = \dfrac{a_i + a_{i+1}}{2}$. Tổng cỡ mẫu là $n = m_1 + m_2 + \dots + m_k$.

## 2. Các công thức

- Số trung bình: $\bar{x} = \dfrac{m_1 c_1 + m_2 c_2 + \dots + m_k c_k}{n}$.
- Trung vị: gọi $[a_p; a_{p+1})$ là nhóm đầu tiên có tần số tích lũy $\ge \dfrac{n}{2}$, khi đó
  $$M_e = a_p + \dfrac{\dfrac{n}{2} - (m_1 + \dots + m_{p-1})}{m_p}\cdot (a_{p+1} - a_p).$$
- Mốt: gọi $[a_j; a_{j+1})$ là nhóm có tần số lớn nhất, khi đó
  $$M_o = a_j + \dfrac{m_j - m_{j-1}}{(m_j - m_{j-1}) + (m_j - m_{j+1})}\cdot (a_{j+1} - a_j).$$

## 3. Ví dụ

Thống kê tốc độ (km/h) của 42 ô tô khi qua một trạm đo:

| Nhóm | Tần số | Giá trị đại diện | Tần số tích lũy |
| --- | --- | --- | --- |
| $[40;45)$ | 5 | 42,5 | 5 |
| $[45;50)$ | 10 | 47,5 | 15 |
| $[50;55)$ | 7 | 52,5 | 22 |
| $[55;60)$ | 9 | 57,5 | 31 |
| $[60;65)$ | 7 | 62,5 | 38 |
| $[65;70)$ | 4 | 67,5 | 42 |

**a)** Quy định tốc độ tối đa là 60 km/h. Số xe vi phạm là $7 + 4 = 11$ xe.

**b)** Số trung bình: $\bar{x} = \dfrac{5\cdot 42{,}5 + 10\cdot 47{,}5 + 7\cdot 52{,}5 + 9\cdot 57{,}5 + 7\cdot 62{,}5 + 4\cdot 67{,}5}{42}
= \dfrac{2280}{42} \approx 54{,}3$ (km/h).

**c)** Vì $\dfrac{n}{2} = 21$ và nhóm $[50;55)$ là nhóm đầu tiên có tần số tích lũy $22 \ge 21$ nên
$M_e = 50 + \dfrac{21 - 15}{7}\cdot 5 \approx 54{,}3$.

**d)** Nhóm $[45;50)$ có tần số lớn nhất (10) nên $M_o = 45 + \dfrac{10 - 5}{(10 - 5) + (10 - 7)}\cdot 5 \approx 48{,}1$.

## 4. Lưu ý

- Số trung bình chịu ảnh hưởng của mọi giá trị, còn trung vị ít bị ảnh hưởng bởi giá trị ngoại lai.
- Khi tính trung vị và mốt phải kiểm tra các nhóm có độ rộng bằng nhau.
