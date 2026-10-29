# Estética & Bem-Estar

Frontend em Angular para catálogo, carrinho, cadastro de clientes e administração da loja.

## Executar

Use Node.js 22 e rode os comandos na raiz do projeto:

```bash
npm ci
npm start
```

Abra http://localhost:4200.

## API

As telas usam uma API externa. O arquivo `proxy.conf.json` encaminha as chamadas `/api` para `http://localhost:5126`.

## Compilar

```bash
npm run build
```

Os arquivos compilados ficam em `dist/loja-frontend/browser`.
