using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogoController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public CatalogoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA PRODUTOS E SERVIÇOS COM FILTROS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogoDTO>>> GetItens(
            string? nome, int? categoria, string? tipo, bool ativos = true)
        {
            var consulta = _context.Items.AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
            {
                consulta = consulta.Where(i => i.Item1.Contains(nome));
            }

            if (categoria.HasValue)
            {
                consulta = consulta.Where(i => i.Idcategoria == categoria.Value);
            }

            if (tipo == "P" || tipo == "S")
            {
                consulta = consulta.Where(i => i.TipoItem == tipo);
            }

            if (ativos)
            {
                consulta = consulta.Where(i => i.AtivoProdutoServico == "S");
            }

            var itens = await Projetar(consulta).OrderBy(i => i.Item).ToListAsync();
            return Ok(itens);
        }

        // BUSCA UM PRODUTO OU SERVIÇO PELO ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CatalogoDTO>> GetItem(int id)
        {
            var item = await Projetar(_context.Items.Where(i => i.Iditem == id))
                .FirstOrDefaultAsync();
            if (item == null)
            {
                return NotFound(new { mensagem = "Item não encontrado." });
            }

            return Ok(item);
        }

        private static IQueryable<CatalogoDTO> Projetar(IQueryable<Item> consulta)
        {
            return consulta.Select(i => new CatalogoDTO
            {
                IdItem = i.Iditem,
                IdCategoria = i.Idcategoria,
                Categoria = i.IdcategoriaNavigation.Categoria,
                Item = i.Item1,
                DescricaoItem = i.DescricaoItem,
                TipoItem = i.TipoItem,
                AtivoProdutoServico = i.AtivoProdutoServico,
                Preco = i.Produto == null ? i.Preco : i.Produto.PrecoVenda,
                Unidade = i.Produto == null ? null : i.Produto.Unidade,
                QuantidadeEstoque = i.Produto == null || i.Produto.Estoque == null
                    ? null : i.Produto.Estoque.QuantidadeEstoque
            });
        }
    }
}
