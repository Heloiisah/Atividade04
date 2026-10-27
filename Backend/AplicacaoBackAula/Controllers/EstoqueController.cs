using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstoqueController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public EstoqueController(BdAulaBackendContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstoqueConsultaDTO>>> GetEstoques()
        {
            var estoques = await Projetar(_context.Estoques)
                .OrderBy(e => e.Item).ToListAsync();
            return Ok(estoques);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EstoqueConsultaDTO>> GetEstoque(int id)
        {
            var estoque = await Projetar(_context.Estoques.Where(e => e.Iditem == id))
                .FirstOrDefaultAsync();
            if (estoque == null)
            {
                return NotFound(new { mensagem = "Produto não encontrado no estoque." });
            }

            return Ok(estoque);
        }

        private static IQueryable<EstoqueConsultaDTO> Projetar(IQueryable<Estoque> consulta)
        {
            return consulta.Select(e => new EstoqueConsultaDTO
            {
                IdItem = e.Iditem,
                Item = e.IditemNavigation.IditemNavigation.Item1,
                DescricaoItem = e.IditemNavigation.IditemNavigation.DescricaoItem,
                Preco = e.IditemNavigation.IditemNavigation.Preco,
                PrecoVenda = e.IditemNavigation.PrecoVenda,
                Unidade = e.IditemNavigation.Unidade,
                Categoria = e.IditemNavigation.IditemNavigation.IdcategoriaNavigation.Categoria,
                QuantidadeEstoque = e.QuantidadeEstoque,
                AtivoProdutoServico = e.IditemNavigation.IditemNavigation.AtivoProdutoServico
            });
        }
    }
}
