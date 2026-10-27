
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntradaProdutoController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public EntradaProdutoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTAR TODAS AS ENTRADAS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntradaProdutoDTO>>> GetEntradas()
        {
            var entradas = await _context.EntradaProdutos
                .Select(e => new EntradaProdutoDTO
                {
                    IdEntradaProduto = e.IdentradaProduto,
                    IdItem = e.Iditem,
                    DataEntradaProduto = e.DataEntradaProduto,
                    QuantidadeEntrada = e.QuantidadeEntrada,
                    Unidade = e.Unidade,
                    ValorUnitarioEntrada = e.ValorUnitarioEntrada,
                    QuantidadeEstoque = e.IditemNavigation.QuantidadeEstoque
                }).ToListAsync();

            return Ok(entradas);
        }

        // BUSCAR ENTRADA PELO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<EntradaProdutoDTO>> GetEntrada(int id)
        {
            var entrada = await _context.EntradaProdutos
                .Where(e => e.IdentradaProduto == id)
                .Select(e => new EntradaProdutoDTO
                {
                    IdEntradaProduto = e.IdentradaProduto,
                    IdItem = e.Iditem,
                    DataEntradaProduto = e.DataEntradaProduto,
                    QuantidadeEntrada = e.QuantidadeEntrada,
                    Unidade = e.Unidade,
                    ValorUnitarioEntrada = e.ValorUnitarioEntrada,
                    QuantidadeEstoque = e.IditemNavigation.QuantidadeEstoque
                }).FirstOrDefaultAsync();

            if (entrada == null)
            {
                return NotFound();
            }

            return Ok(entrada);
        }

        // ADICIONAR UMA ENTRADA
        [HttpPost]
        public async Task<ActionResult<EntradaProdutoDTO>> PostEntrada(
            EntradaProdutoPostDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Dados da entrada incompletos.");
            }

            if (dto.IdItem <= 0 || dto.QuantidadeEntrada <= 0 ||
                dto.ValorUnitarioEntrada <= 0 || dto.DataEntradaProduto == default ||
                string.IsNullOrWhiteSpace(dto.Unidade))
                return BadRequest("Informe produto, data, quantidade, unidade e valor válidos.");

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Iditem == dto.IdItem);
            if (produto == null)
                return NotFound("Produto não encontrado.");
            if (produto.Unidade.Trim() != dto.Unidade.Trim())
                return BadRequest("A unidade deve ser igual à do produto.");

            var estoque = await _context.Estoques
                .FirstOrDefaultAsync(e => e.Iditem == dto.IdItem);

            if (estoque == null)
            {
                return BadRequest("Produto sem estoque cadastrado.");
            }

            var entrada = new EntradaProduto
            {
                Iditem = dto.IdItem,
                DataEntradaProduto = dto.DataEntradaProduto,
                QuantidadeEntrada = dto.QuantidadeEntrada,
                Unidade = dto.Unidade,
                ValorUnitarioEntrada = dto.ValorUnitarioEntrada
            };

            _context.EntradaProdutos.Add(entrada);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel registrar a entrada.");
            }

            var resposta = new EntradaProdutoDTO
            {
                IdEntradaProduto = entrada.IdentradaProduto,
                IdItem = entrada.Iditem,
                DataEntradaProduto = entrada.DataEntradaProduto,
                QuantidadeEntrada = entrada.QuantidadeEntrada,
                Unidade = entrada.Unidade,
                ValorUnitarioEntrada = entrada.ValorUnitarioEntrada,
                QuantidadeEstoque = await _context.Estoques
                    .Where(e => e.Iditem == dto.IdItem)
                    .Select(e => e.QuantidadeEstoque)
                    .FirstOrDefaultAsync()
            };

            return CreatedAtAction(
                nameof(GetEntrada),
                new { id = entrada.IdentradaProduto },
                resposta
            );
        }

        // ALTERAR UMA ENTRADA
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntrada(int id, EntradaProdutoPutDTO dto)
        {
            if (!await _context.EntradaProdutos.AnyAsync(e => e.IdentradaProduto == id))
                return NotFound();
            // O BANCO RECEBIDO ATUALIZA O ESTOQUE APENAS NO INSERT DA ENTRADA.
            return Conflict("Entradas registradas não podem ser alteradas. Registre uma nova entrada.");
        }

        // EXCLUIR UMA ENTRADA
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntrada(int id)
        {
            if (!await _context.EntradaProdutos.AnyAsync(e => e.IdentradaProduto == id))
                return NotFound();
            // EXCLUIR A ENTRADA NÃO DESFAZ O TRIGGER QUE SOMOU AO ESTOQUE.
            return Conflict("Entradas registradas não podem ser excluídas.");
        }
    }
}
