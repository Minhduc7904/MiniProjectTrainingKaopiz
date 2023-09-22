# API

## GET /api/authentication/login

### Overview

- API phục vụ cho việc đăng nhập, dựa vào tài khoản, mật khẩu đã cung cấp để trả lại thông tin account và token

### Sequence Diagram

- Đăng nhập dựa vào tài khoản, mật khẩu đã cung cấp để trả lại thông tin account và token

```mermaid
sequenceDiagram
  participant fe as Frontend
  participant be as Backend
  participant db as Database

  fe ->>+ be: Api Request POST /api/authentication/login
    be ->>+ db: Lấy data account từ bảng accounts theo field username
    db -->>- be: Trả kết quả là account
      alt Băm và so sánh mật khẩu
        be ->> be: Nếu đúng: Tạo token
      else
        be ->> be: Nếu sai: Throw ra lỗi đăng nhập thất bại
      end
  be -->>- fe: Trả về kết quả Api
```

### Query Parameter

| Query Parameter | Overview | Validator 
|-----------------|----------|-----------

Không có

### Body Parameter

| Body Parameter | Overview  | Validator 
|----------------|-----------|-----------
| username       | Tài khoản | Required  
| password       | Mật khẩu  | Required  

- Example:

```json
{
  "username": "username", 
  "password": "password"  
}
```

- Thao tác với DB
    - Chỉ lấy thông tin 1 tài khoản dựa theo username đã cung cấp
  ```sql
    SELECT * FROM accounts WHERE username = {username}
    LIMIT {1}
  ```
- Logic
    - Query bảng accounts lấy account theo username
    - Băm mật khẩu người dùng nhập
    - So sánh mật khẩu đã băm với mật khẩu trong database
        - Nếu đúng: Tạo token, object trả kết quả
        - Nếu sai: Throw ra lỗi đăng nhập thất bại
    - Trả về kết quả gồm token và account (chứa thông tin cần như ví dụ dưới)
- Response
  Success:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEiLCJyb2xlIjoiQURNSU4iLCJleHAiOjE2OTUxMzY1NjV9.BdolFj82ttdoOftRz4b9b5xhvP6fCmmb6bSLcQ8pO3k", //Access Token
  "account": { //Object account chứa thông tin tài khoản
    "username": "string",
    "fullName": "string",
    "phone": "string",
    "email": "string",
    "address": "string",
    "role": "string",
    "id": 0
  }
}
```

Error - Tài khoản hoặc mật khẩu không đúng:

```json
{
  "ErrorCode": "LOGIN_FAIL",
  "ErrorDetail": null
}
```

# Others

Không có
