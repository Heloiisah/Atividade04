using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.Models;
using AplicacaoBackAula.DTO;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoItemController : ControllerBase
    {
        // DEFININDO ATRIBUTO DO CONTEXT
        private readonly BdAulaBackendContext _context;

        // MÉTODO CONSTRUTOR
        public ProdutoItemController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA TODOS OS PRODUTOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutos()
        {
            var produtos = await _context.Produtos
                .Select(p => new ProdutoDTO
                {
                    IdItem = p.Iditem,
                    Idcategoria = p.IditemNavigation.Idcategoria,
                    Item = p.IditemNavigation.Item1,
                    DescricaoItem = p.IditemNavigation.DescricaoItem,
                    PrecoItem = p.IditemNavigation.Preco,
                    TipoItem = p.IditemNavigation.TipoItem,
                    AtivoProdutoServico = p.IditemNavigation.AtivoProdutoServico,
                    CodigoPreco = p.CodigoPreco,
                    PrecoVenda = p.PrecoVenda,
                    Unidade = p.Unidade
                })
                .ToListAsync();

            return Ok(produtos);
        }

        // BUSCA UM PRODUTO PELO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDTO>> GetProdutos(int id)
        {
            var produto = await _context.Produtos
                .Where(p => p.Iditem == id)
                .Select(p => new ProdutoDTO
                {
                    IdItem = p.Iditem,
                    Idcategoria = p.IditemNavigation.Idcategoria,
                    Item = p.IditemNavigation.Item1,
                    DescricaoItem = p.IditemNavigation.DescricaoItem,
                    PrecoItem = p.IditemNavigation.Preco,
                    TipoItem = p.IditemNavigation.TipoItem,
                    AtivoProdutoServico = p.IditemNavigation.AtivoProdutoServico,
                    CodigoPreco = p.CodigoPreco,
                    PrecoVenda = p.PrecoVenda,
                    Unidade = p.Unidade
                })
                .FirstOrDefaultAsync();

            // CASO NÃO ENCONTRE O PRODUTO
            if (produto == null)
            {
                return NotFound(new
                {
                    mensagem = "Produto Não localizado"
                });
            }

            return Ok(produto);
        }

        // ADICIONAR UM PRODUTO
        [HttpPost]
        public async Task<ActionResult> PostProduto(ProdutoItemPostDTO dados)
        {
            if (dados.TipoItem != "P" || string.IsNullOrWhiteSpace(dados.Item) ||
                string.IsNullOrWhiteSpace(dados.DescricaoItem) || dados.PrecoItem <= 0 ||
                dados.CodigoPreco < 1 || dados.CodigoPreco > 3 ||
                string.IsNullOrWhiteSpace(dados.Unidade))
                return BadRequest("Dados do produto incompletos.");

            // VERIFICA SE A CATEGORIA EXISTE
            if (!_context.Categoria.Any(c => c.Idcategoria == dados.Idcategoria))
            {
                return BadRequest("Categoria nao encontrada.");
            }

            // INICIA A TRANSAÇÃO NO BANCO DE DADOS
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // CRIA O OBJETO ITEM
                var item = new Item
                {
                    Idcategoria = dados.Idcategoria,
                    Item1 = dados.Item,
                    DescricaoItem = dados.DescricaoItem,
                    Preco = dados.PrecoItem,
                    TipoItem = dados.TipoItem,
                    AtivoProdutoServico = dados.AtivoProdutoServico
                };

                // PREPARA O ITEM PARA SER ENVIADO AO BANCO DE DADOS
                _context.Items.Add(item);

                // SALVA O ITEM NO BANCO DE DADOS
                await _context.SaveChangesAsync();

                // PEGA O ID GERADO PARA O ITEM
                int idItemGerado = item.Iditem;

                // CRIA O OBJETO PRODUTO
                var produto = new Produto
                {
                    Iditem = idItemGerado,
                    CodigoPreco = dados.CodigoPreco,
                    PrecoVenda = CalcularPreco(dados.CodigoPreco, dados.PrecoItem),
                    Unidade = dados.Unidade
                };

                // PREPARA O PRODUTO PARA SER ENVIADO AO BANCO DE DADOS
                _context.Produtos.Add(produto);

                // SALVA O PRODUTO NO BANCO DE DADOS
                await _context.SaveChangesAsync();

                // CONFIRMA A TRANSAÇÃO
                await transaction.CommitAsync();

                // RETORNA OS DADOS CADASTRADOS
                return CreatedAtAction(
                    nameof(GetProdutos),
                    new { id = produto.Iditem },
                    produto
                );
            }
            catch (Exception ex)
            {
                // DESFAZ AS OPERAÇÕES CASO OCORRA UM ERRO
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensagem = "Erro ao cadastrar produto ",
                    erro = ex.Message
                });
            }
        }

        // ALTERAR PRODUTO
        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduto(int id, ProdutoItemPutDTO dados)
        {
            if (dados.TipoItem != "P" || string.IsNullOrWhiteSpace(dados.Item) ||
                string.IsNullOrWhiteSpace(dados.DescricaoItem) || dados.PrecoItem <= 0 ||
                dados.CodigoPreco < 1 || dados.CodigoPreco > 3 ||
                string.IsNullOrWhiteSpace(dados.Unidade))
                return BadRequest("Dados do produto incompletos.");

            // VERIFICA SE A CATEGORIA EXISTE
            if (!_context.Categoria.Any(c => c.Idcategoria == dados.Idcategoria))
            {
                return BadRequest("Categoria nao encontrada.");
            }

            // BUSCA O PRODUTO E O ITEM RELACIONADO
            var produtoExistente = await _context.Produtos
                .Include(p => p.IditemNavigation)
                .FirstOrDefaultAsync(p => p.Iditem == id);

            // CASO NÃO ENCONTRE O PRODUTO
            if (produtoExistente == null)
            {
                return NotFound(new
                {
                    mensagem = "Produto não encontrado."
                });
            }

            // ALTERA OS DADOS DA TABELA PRODUTO
            produtoExistente.CodigoPreco = dados.CodigoPreco;
            produtoExistente.PrecoVenda = CalcularPreco(dados.CodigoPreco, dados.PrecoItem);
            produtoExistente.Unidade = dados.Unidade;

            // ALTERA OS DADOS DA TABELA ITEM
            produtoExistente.IditemNavigation.Idcategoria = dados.Idcategoria;
            produtoExistente.IditemNavigation.Item1 = dados.Item;
            produtoExistente.IditemNavigation.DescricaoItem = dados.DescricaoItem;
            produtoExistente.IditemNavigation.Preco = dados.PrecoItem;
            produtoExistente.IditemNavigation.TipoItem = dados.TipoItem;
            produtoExistente.IditemNavigation.AtivoProdutoServico = dados.AtivoProdutoServico;

            // SALVA AS ALTERAÇÕES NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // VERIFICA SE O PRODUTO AINDA EXISTE
                if (!ProdutoExists(id))
                {
                    return NotFound(new
                    {
                        mensagem = "Produto não encontrado."
                    });
                }

                throw;
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        // EXCLUIR PRODUTO
        [HttpPut("{id:int}/desativar")]
        public async Task<IActionResult> DesativarProduto(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i =>
                i.Iditem == id && i.TipoItem == "P");
            if (item == null)
                return NotFound(new { mensagem = "Produto não encontrado." });
            item.AtivoProdutoServico = "N";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // EXCLUIR PRODUTO
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            // INICIA A TRANSAÇÃO NO BANCO DE DADOS
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // VERIFICA SE O PRODUTO EXISTE
                var produto = await _context.Produtos
                    .FirstOrDefaultAsync(p => p.Iditem == id);

                if (produto == null)
                {
                    return NotFound(new
                    {
                        mensagem = "Produto não encontrado."
                    });
                }

                // VERIFICA SE O PRODUTO POSSUI REGISTROS VINCULADOS
                if (_context.EntradaProdutos.Any(e => e.Iditem == id) ||
                    _context.Atendimentoitems.Any(a => a.Iditem == id))
                {
                    return StatusCode(409, "O produto possui entradas ou atendimentos vinculados.");
                }

                // BUSCA O ESTOQUE DO PRODUTO
                var estoque = await _context.Estoques
                    .FirstOrDefaultAsync(e => e.Iditem == id);

                // CASO EXISTA ESTOQUE, PREPARA A EXCLUSÃO
                if (estoque != null)
                {
                    _context.Estoques.Remove(estoque);
                    await _context.SaveChangesAsync();
                }

                // PREPARA A EXCLUSÃO DO PRODUTO
                _context.Produtos.Remove(produto);

                // SALVA A EXCLUSÃO DO PRODUTO
                await _context.SaveChangesAsync();

                // BUSCA O ITEM CORRESPONDENTE AO PRODUTO
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Iditem == id);

                // CASO ENCONTRE O ITEM
                if (item != null)
                {
                    // PREPARA A EXCLUSÃO DO ITEM
                    _context.Items.Remove(item);

                    // SALVA A EXCLUSÃO DO ITEM
                    await _context.SaveChangesAsync();
                }

                // CONFIRMA A TRANSAÇÃO
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // DESFAZ AS OPERAÇÕES CASO OCORRA UM ERRO
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensagem = "Erro ao excluir produto.",
                    erro = ex.Message
                });
            }
        }

        // VERIFICA SE O PRODUTO EXISTE
        private bool ProdutoExists(int id)
        {
            return _context.Produtos
                .Any(p => p.Iditem == id);
        }

        private static decimal CalcularPreco(int codigo, decimal custo)
        {
            if (codigo == 1) return Math.Round(custo * 1.04m, 2, MidpointRounding.AwayFromZero);
            if (codigo == 2) return Math.Round(custo * 1.08m, 2, MidpointRounding.AwayFromZero);
            return Math.Round(custo * 1.10m, 2, MidpointRounding.AwayFromZero);
        }
    }
}
