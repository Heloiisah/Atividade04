using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemServicoController : ControllerBase
    {
        // DEFININDO ATRIBUTO DO CONTEXT
        private readonly BdAulaBackendContext _context;

        // MÉTODO CONSTRUTOR
        public ItemServicoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA OS ITENS DO TIPO SERVIÇO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItens()
        {
            var itens = await _context.Items
                .Where(i => i.TipoItem == "S")
                .Select(i => new ItemDTO
                {
                    IdItem = i.Iditem,
                    IdCategoria = i.Idcategoria,
                    Item = i.Item1,
                    DescricaoItem = i.DescricaoItem,
                    Preco = i.Preco,
                    TipoItem = i.TipoItem,
                    AtivoProdutoServico = i.AtivoProdutoServico
                }).ToListAsync();

            return Ok(itens);
        }

        // BUSCA UM ITEM PELO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDTO>> GetItem(int id)
        {
            var item = await _context.Items
                .Where(i => i.Iditem == id && i.TipoItem == "S")
                .Select(i => new ItemDTO
                {
                    IdItem = i.Iditem,
                    IdCategoria = i.Idcategoria,
                    Item = i.Item1,
                    DescricaoItem = i.DescricaoItem,
                    Preco = i.Preco,
                    TipoItem = i.TipoItem,
                    AtivoProdutoServico = i.AtivoProdutoServico
                }).FirstOrDefaultAsync();

            // CASO NÃO ENCONTRE O ITEM
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // ADICIONAR UM ITEM
        [HttpPost]
        public async Task<ActionResult<ItemDTO>> PostItem(ItemDTO dados)
        {
            if (dados.TipoItem != "S" || string.IsNullOrWhiteSpace(dados.Item) ||
                string.IsNullOrWhiteSpace(dados.DescricaoItem) || dados.Preco <= 0)
            {
                return BadRequest("Dados do serviço incompletos.");
            }

            // VERIFICA SE A CATEGORIA EXISTE
            if (!_context.Categoria.Any(c => c.Idcategoria == dados.IdCategoria))
            {
                return BadRequest("Categoria nao encontrada.");
            }

            // CRIA O OBJETO ITEM E PROJETA OS DADOS DO DTO NESSE OBJETO
            var item = new Item
            {
                Idcategoria = dados.IdCategoria,
                Item1 = dados.Item,
                DescricaoItem = dados.DescricaoItem,
                Preco = dados.Preco,
                TipoItem = dados.TipoItem,
                AtivoProdutoServico = dados.AtivoProdutoServico
            };

            // PREPARA OS DADOS PARA SEREM ENVIADOS AO BANCO DE DADOS
            _context.Items.Add(item);

            // SALVA OS DADOS NO BANCO DE DADOS
            await _context.SaveChangesAsync();

            // ATRIBUI O ID GERADO AO DTO
            dados.IdItem = item.Iditem;

            // RETORNA OS DADOS CADASTRADOS
            return CreatedAtAction(nameof(GetItem), new { id = item.Iditem }, dados);
        }

        // ALTERAR ITEM
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(int id, ItemDTO dados)
        {
            if (dados.TipoItem != "S" || string.IsNullOrWhiteSpace(dados.Item) ||
                string.IsNullOrWhiteSpace(dados.DescricaoItem) || dados.Preco <= 0)
            {
                return BadRequest("Dados do serviço incompletos.");
            }

            // VERIFICA SE O ITEM EXISTE NO BANCO DE DADOS
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Iditem == id && i.TipoItem == "S");

            if (item == null)
            {
                return NotFound();
            }

            // VERIFICA SE A CATEGORIA EXISTE
            if (!_context.Categoria.Any(c => c.Idcategoria == dados.IdCategoria))
            {
                return BadRequest("Categoria nao encontrada.");
            }

            // VERIFICA SE O TIPO DO ITEM PODE SER ALTERADO
            if (item.TipoItem != dados.TipoItem &&
                (_context.Produtos.Any(p => p.Iditem == id) ||
                 _context.Atendimentoitems.Any(a => a.Iditem == id)))
            {
                return StatusCode(409, "O tipo do item nao pode ser alterado enquanto ele estiver vinculado.");
            }

            // ALTERA OS DADOS DO ITEM
            item.Idcategoria = dados.IdCategoria;
            item.Item1 = dados.Item;
            item.DescricaoItem = dados.DescricaoItem;
            item.Preco = dados.Preco;
            item.TipoItem = dados.TipoItem;
            item.AtivoProdutoServico = dados.AtivoProdutoServico;

            // SALVA AS ALTERAÇÕES NO BANCO DE DADOS
            await _context.SaveChangesAsync();

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        [HttpPut("{id:int}/desativar")]
        public async Task<IActionResult> DesativarItem(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i =>
                i.Iditem == id && i.TipoItem == "S");
            if (item == null)
            {
                return NotFound();
            }

            item.AtivoProdutoServico = "N";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // EXCLUIR ITEM
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            // VERIFICA SE O ITEM EXISTE NO BANCO DE DADOS
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Iditem == id);

            // CASO NÃO ENCONTRE O ITEM
            if (item == null)
            {
                return NotFound();
            }

            // PREPARA A EXCLUSÃO DO ITEM
            _context.Items.Remove(item);

            // SALVA A EXCLUSÃO NO BANCO DE DADOS
            await _context.SaveChangesAsync();

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }
    }
}
