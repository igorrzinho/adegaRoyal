# Adega Royal API — Guia de Uso

Base URL local: `http://localhost:5065`

---

## 🔐 Autenticação

### Criar conta de Customer (sem login)

```bash
curl -X POST http://localhost:5065/api/auth/register/customer \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao@email.com",
    "password": "Senha!123"
  }'
```

### Criar conta de Admin (sem login)

```bash
curl -X POST http://localhost:5065/api/auth/register/admin \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Igor Admin",
    "email": "igor@adega.com",
    "password": "Admin!123"
  }'
```

### Login (retorna JWT)

```bash
curl -X POST http://localhost:5065/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@email.com",
    "password": "Senha!123"
  }'
```

> Guarde o `accessToken` retornado. Use-o como `Bearer` em todas as requisições protegidas.

---

## 🛍️ Produtos (Catálogo)

### Listar todos os produtos

```bash
curl http://localhost:5065/api/products \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Listar por categoria

```bash
curl "http://localhost:5065/api/products?categoryId={CATEGORY_GUID}" \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Criar produto (Admin)

```bash
curl -X POST http://localhost:5065/api/products \
  -H "Authorization: Bearer {TOKEN_ADMIN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Vinho Tinto Reserva",
    "description": "Vinho encorpado com notas de frutas vermelhas",
    "price": 89.90,
    "stockQuantity": 50,
    "imageUrl": "https://exemplo.com/vinho.jpg",
    "categoryId": "{CATEGORY_GUID}"
  }'
```

---

## 📂 Categorias

### Listar categorias

```bash
curl http://localhost:5065/api/categories \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Criar categoria (Admin)

```bash
curl -X POST http://localhost:5065/api/categories \
  -H "Authorization: Bearer {TOKEN_ADMIN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Vinhos Tintos",
    "description": "Vinhos tintos nacionais e importados"
  }'
```

---

## 🛒 Carrinho

### Ver meu carrinho

```bash
curl http://localhost:5065/api/cart \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Adicionar item ao carrinho

```bash
curl -X POST http://localhost:5065/api/cart/items \
  -H "Authorization: Bearer {SEU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "{PRODUCT_GUID}",
    "quantity": 2
  }'
```

### Atualizar quantidade de um item

```bash
curl -X PATCH http://localhost:5065/api/cart/items/{CART_ITEM_GUID} \
  -H "Authorization: Bearer {SEU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{ "quantity": 3 }'
```

### Remover item do carrinho

```bash
curl -X DELETE http://localhost:5065/api/cart/items/{CART_ITEM_GUID} \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Limpar carrinho inteiro

```bash
curl -X DELETE http://localhost:5065/api/cart \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

---

## 💳 Checkout (Compra Parcial)

Envie apenas os IDs dos itens do carrinho que deseja comprar.
Os itens não selecionados continuam no carrinho.

```bash
curl -X POST http://localhost:5065/api/orders/checkout \
  -H "Authorization: Bearer {SEU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "cartItemIds": [
      "{CART_ITEM_GUID_1}",
      "{CART_ITEM_GUID_2}"
    ],
    "notes": "Entregar no período da tarde"
  }'
```

> O checkout processa o pagamento via Abacate Pay, cria o pedido e gera automaticamente o registro de entrega com código OTP.

---

## 📦 Pedidos

### Meus pedidos

```bash
curl http://localhost:5065/api/orders \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Detalhe de um pedido

```bash
curl http://localhost:5065/api/orders/{ORDER_GUID} \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Cancelar pedido pendente

```bash
curl -X DELETE http://localhost:5065/api/orders/{ORDER_GUID} \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Todos os pedidos (Admin)

```bash
curl http://localhost:5065/api/orders/admin/all \
  -H "Authorization: Bearer {TOKEN_ADMIN}"
```

### Atualizar status do pedido (Admin)

```bash
curl -X PATCH http://localhost:5065/api/orders/{ORDER_GUID}/status \
  -H "Authorization: Bearer {TOKEN_ADMIN}" \
  -H "Content-Type: application/json" \
  -d '{ "status": "Paid" }'
```

**Status disponíveis:** `Pending` · `Paid` · `Shipped` · `Delivered` · `Cancelled` · `Returned`

---

## 🚚 Entregas

### Ver status da entrega (Customer — sem código OTP)

```bash
curl http://localhost:5065/api/deliveries/order/{ORDER_GUID} \
  -H "Authorization: Bearer {SEU_TOKEN}"
```

### Ver entrega com código OTP (Admin)

```bash
curl http://localhost:5065/api/deliveries/order/{ORDER_GUID} \
  -H "Authorization: Bearer {TOKEN_ADMIN}"
```

### Atualizar status da entrega (Admin)

```bash
curl -X PATCH http://localhost:5065/api/deliveries/order/{ORDER_GUID}/status \
  -H "Authorization: Bearer {TOKEN_ADMIN}" \
  -H "Content-Type: application/json" \
  -d '{ "status": "OnTheWay" }'
```

**Status disponíveis:** `Preparing` · `WaitingForCourier` · `OnTheWay` · `Delivered`

### Verificar código OTP na entrega (Admin/Entregador)

```bash
curl -X POST http://localhost:5065/api/deliveries/order/{ORDER_GUID}/verify \
  -H "Authorization: Bearer {TOKEN_ADMIN}" \
  -H "Content-Type: application/json" \
  -d '{ "code": "4217" }'
```

> Se o código estiver correto, a entrega e o pedido são marcados como `Delivered` automaticamente.

---

## ❤️ Health Check

```bash
curl http://localhost:5065/health
```

---

## 🔑 Configuração Keycloak (appsettings.json)

```json
"Keycloak": {
  "BaseUrl": "http://localhost:8080",
  "Realm": "adega-royal",
  "AdminUsername": "admin",
  "AdminPassword": "admin",
  "PublicClientId": "adega-client"
}
```

> `AdminUsername`/`AdminPassword` são as credenciais `KEYCLOAK_ADMIN` / `KEYCLOAK_ADMIN_PASSWORD` do `docker-compose.yml`.
> O `adega-client` é usado como client público (sem secret) para obter o token de administração.