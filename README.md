## Organização

- `Backend/`: solução ASP.NET Core Web API, Entity Framework Core, models, DTOs e controllers.
- `Frontend/`: aplicação Angular com HTML, CSS e TypeScript.
- `BancoDeDados/`: modelagem, dump com dados, rotinas, triggers e views usadas pelo backend.
- `ROTEIRO_DA_ATIVIDADE.pdf`: enunciado da atividade.

No frontend, `src/app/app.component.*` contém somente a navegação. Cada tela tem seus
arquivos `.ts` e `.html` em `src/app/paginas/`. O card do catálogo fica em
`src/app/componentes/`, as chamadas à API, o carrinho e os avisos ficam em
`src/app/servicos/`, e os tipos dos dados ficam em `src/app/modelos/`.

Os arquivos temporários de compilação e instalação (`bin`, `obj`, `.vs`, `node_modules`, `.angular` e `dist`) não fazem parte da entrega. A compilação pode deixar diretórios `bin` e `obj` vazios.

## 1. Preparar o banco no MySQL Workbench

Conecte-se ao MySQL e execute `BancoDeDados/00_criar_schema.sql`. Depois, no painel **SCHEMAS**, clique duas vezes em `bd_aula_backend` para torná-lo o schema padrão. Abra e execute os arquivos de `BancoDeDados/Dump20261005/` nesta ordem:

1. `bd_aula_backend_categoria.sql`
2. `bd_aula_backend_pessoa.sql`
3. `bd_aula_backend_funcionario.sql`
4. `bd_aula_backend_endereco.sql`
5. `bd_aula_backend_item.sql`
6. `bd_aula_backend_produto.sql`
7. `bd_aula_backend_estoque.sql`
8. `bd_aula_backend_entrada_produto.sql`
9. `bd_aula_backend_atendimento.sql`
10. `bd_aula_backend_atendimentoitem.sql`
11. `bd_aula_backend_routines.sql`

Por último, execute `BancoDeDados/views_backlo.sql`. Esses arquivos incluem os dados de exemplo e os objetos necessários ao funcionamento do projeto. Os scripts do dump contêm `DROP TABLE`: faça a importação em um schema novo para não apagar dados existentes.

Para conferir:

```sql
USE bd_aula_backend;
SELECT COUNT(*) FROM item;
SELECT COUNT(*) FROM estoque;
SELECT * FROM vw_produtos_categorias_estoque LIMIT 5;
```

## 2. Iniciar o backend

Confira os dados de conexão com o MySQL em `Backend/AplicacaoBackAula/appsettings.json`. A configuração desta entrega já foi atualizada; se o MySQL do computador de execução usar outros dados, ajuste esse arquivo antes de iniciar a API.

Se uma chamada à API mostrar `Access denied for user 'root'@'localhost'`, a senha `root` configurada no projeto não corresponde à senha desse usuário no seu MySQL. Altere `user` e `password` na linha `connection` do `appsettings.json`, salve e reinicie a API. Faça a alteração na pasta de onde você está executando o backend: o projeto original em `backlo` e esta cópia em `ENTREGAS` têm arquivos de configuração separados. Não coloque sua senha no material antes de enviá-lo.

No computador de execução, que já possui o **.NET 8 SDK**, abra o PowerShell na pasta `ENTREGAS` e execute:

```powershell
dotnet run --project '.\Backend\AplicacaoBackAula\AplicacaoBackAula.csproj' --launch-profile http
```

O Swagger estará em `http://localhost:5126/swagger`.

## 3. Iniciar o frontend

Abra outra janela do PowerShell na pasta `ENTREGAS`:

```powershell
cd Frontend
npm install
npm start
```

A aplicação estará em `http://localhost:4200`. O arquivo `Frontend/proxy.conf.json` encaminha as requisições `/api` ao backend na porta `5126`.

## Observações

- Os pedidos são registrados no MySQL. O carrinho fica temporariamente no Angular até a confirmação.
- O banco fornecido já contém alguns CPFs e e-mails repetidos. Novos cadastros são validados pelo backend.
- O banco não contém senhas ou uma tabela de usuários. A consulta dos pedidos do cliente usa a conferência dos dados cadastrais; o painel administrativo não tem autenticação.
- A modelagem original está em `BancoDeDados/modelagem_bd_loja.mwb`; para executar a atividade com os dados de exemplo, siga a importação do dump acima.
# Atividade04
