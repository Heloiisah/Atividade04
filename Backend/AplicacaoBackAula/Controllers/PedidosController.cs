using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public PedidosController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA OS PEDIDOS E PERMITE FILTRAR PELA SITUAÇÃO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtendimentoDTO>>> GetPedidos(string? status)
        {
            var codigo = CodigoStatus(status);
            if (status != null && codigo == null)
                return BadRequest(new { mensagem = "Situação inválida." });

            var consulta = _context.Atendimentos.AsQueryable();
            if (codigo != null)
                consulta = consulta.Where(a => a.StatusAtendimento == codigo);

            var pedidos = await Projetar(consulta)
                .OrderByDescending(a => a.IdAtendimento).ToListAsync();
            return Ok(pedidos);
        }

        // BUSCA UM PEDIDO PELO ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AtendimentoDTO>> GetPedido(int id)
        {
            var pedido = await Projetar(_context.Atendimentos
                .Where(a => a.Idatendimento == id)).FirstOrDefaultAsync();
            if (pedido == null)
                return NotFound(new { mensagem = "Pedido não encontrado." });
            return Ok(pedido);
        }

        // CONFERE OS DADOS INFORMADOS PELO CLIENTE ANTES DE CONSULTAR SEUS PEDIDOS
        [HttpPost("meus")]
        public async Task<ActionResult<IEnumerable<AtendimentoDTO>>> MeusPedidos(IdentificacaoClienteDTO dto)
        {
            if (!await ClienteValido(dto.IdPessoa, dto.CPF, dto.Telefone, dto.Email))
                return BadRequest(new { mensagem = "Dados do cliente não conferem." });

            var pedidos = await Projetar(_context.Atendimentos
                .Where(a => a.Idpessoa == dto.IdPessoa))
                .OrderByDescending(a => a.IdAtendimento).ToListAsync();
            return Ok(pedidos);
        }

        [HttpPost("meu/{id:int}")]
        public async Task<ActionResult<AtendimentoDTO>> MeuPedido(int id, IdentificacaoClienteDTO dto)
        {
            if (!await ClienteValido(dto.IdPessoa, dto.CPF, dto.Telefone, dto.Email))
                return BadRequest(new { mensagem = "Dados do cliente não conferem." });
            var pedido = await Projetar(_context.Atendimentos.Where(a =>
                a.Idatendimento == id && a.Idpessoa == dto.IdPessoa)).FirstOrDefaultAsync();
            if (pedido == null)
                return NotFound(new { mensagem = "Pedido não encontrado para este cliente." });
            return Ok(pedido);
        }

        // REGISTRA UM PEDIDO E UTILIZA O TRIGGER DO BANCO PARA BAIXAR O ESTOQUE
        [HttpPost]
        public async Task<ActionResult<AtendimentoDTO>> PostPedido(PedidoCompraDTO dto)
        {
            if (!await ClienteValido(dto.IdPessoa, dto.CPF, dto.Telefone, dto.Email))
                return BadRequest(new { mensagem = "Dados do cliente não conferem." });
            if (dto.Itens.Count == 0 || dto.Itens.Any(i => i.IdItem <= 0 ||
                i.IdFuncionario <= 0 || i.Quantidade <= 0) ||
                dto.Itens.Select(i => i.IdItem).Distinct().Count() != dto.Itens.Count)
                return BadRequest(new { mensagem = "Informe itens distintos e quantidades positivas." });

            var ids = dto.Itens.Select(i => i.IdItem).ToList();
            var itens = await _context.Items
                .Include(i => i.Produto).ThenInclude(p => p!.Estoque)
                .Where(i => ids.Contains(i.Iditem)).ToListAsync();
            if (itens.Count != ids.Count || itens.Any(i => i.AtivoProdutoServico != "S" ||
                (i.TipoItem == "P" && i.Produto == null)))
                return BadRequest(new { mensagem = "Há item inexistente ou inativo." });

            var funcionarios = dto.Itens.Select(i => i.IdFuncionario).Distinct().ToList();
            if (await _context.Funcionarios.CountAsync(f => funcionarios.Contains(f.Idpessoa) &&
                f.IdpessoaNavigation.AtivoPessoa == "S") != funcionarios.Count)
                return BadRequest(new { mensagem = "Funcionário inexistente ou inativo." });

            foreach (var escolhido in dto.Itens)
            {
                var item = itens.First(i => i.Iditem == escolhido.IdItem);
                if (item.TipoItem == "P" && (item.Produto?.Estoque == null ||
                    item.Produto.Estoque.QuantidadeEstoque < escolhido.Quantidade))
                    return BadRequest(new { mensagem = $"Estoque insuficiente para {item.Item1}." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            var atendimento = new Atendimento
            {
                Idpessoa = dto.IdPessoa,
                DataAtendimento = DateOnly.FromDateTime(DateTime.Today),
                StatusAtendimento = "A",
                Atendimentoitems = dto.Itens.Select(i =>
                {
                    var item = itens.First(x => x.Iditem == i.IdItem);
                    return new Atendimentoitem
                    {
                        Iditem = i.IdItem,
                        Idfuncionario = i.IdFuncionario,
                        QuantidadeItens = i.Quantidade,
                        ValorunitarioEntrada = item.TipoItem == "P"
                            ? item.Produto!.PrecoVenda : item.Preco
                    };
                }).ToList()
            };
            _context.Atendimentos.Add(atendimento);

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { mensagem = "Não foi possível registrar o pedido. Confira o estoque." });
            }

            var resposta = await Projetar(_context.Atendimentos
                .Where(a => a.Idatendimento == atendimento.Idatendimento)).FirstAsync();
            return CreatedAtAction(nameof(GetPedido), new { id = atendimento.Idatendimento }, resposta);
        }

        // VALIDA O FLUXO E DEVOLVE O ESTOQUE QUANDO UM PEDIDO É CANCELADO
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> PutStatus(int id, PedidoStatusDTO dto)
        {
            var novoStatus = CodigoStatus(dto.StatusAtendimento);
            if (novoStatus != "P" && novoStatus != "C" && novoStatus != "X")
                return BadRequest(new { mensagem = "Situação inválida." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            var pedido = await _context.Atendimentos
                .Include(a => a.Atendimentoitems).ThenInclude(i => i.IditemNavigation)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);
            if (pedido == null)
                return NotFound(new { mensagem = "Pedido não encontrado." });

            var permitido = pedido.StatusAtendimento == "A" &&
                (novoStatus == "P" || novoStatus == "X") ||
                pedido.StatusAtendimento == "P" &&
                (novoStatus == "C" || novoStatus == "X");
            if (!permitido)
                return BadRequest(new { mensagem = "Transição de situação não permitida." });

            if (novoStatus == "X")
            {
                foreach (var item in pedido.Atendimentoitems.Where(i => i.IditemNavigation.TipoItem == "P"))
                {
                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e => e.Iditem == item.Iditem);
                    if (estoque == null)
                        return BadRequest(new { mensagem = "Estoque do produto não encontrado." });
                    estoque.QuantidadeEstoque += item.QuantidadeItens;
                }
            }

            pedido.StatusAtendimento = novoStatus;
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return Conflict(new { mensagem = "Pedido alterado por outra operação. Consulte novamente." });
            }
            return NoContent();
        }

        private async Task<bool> ClienteValido(int id, long cpf, long telefone, string email)
        {
            return await _context.Pessoas.AnyAsync(p => p.Idpessoa == id && p.Cpf == cpf &&
                p.Telefone == telefone && p.Email == email && p.AtivoPessoa == "S" &&
                p.Funcionario == null);
        }

        private static string? CodigoStatus(string? status)
        {
            if (status == null)
                return null;

            var codigo = status.Trim().ToUpperInvariant();
            if (codigo == "A" || codigo == "ABERTO")
                return "A";
            if (codigo == "P" || codigo == "EM PROCESSAMENTO")
                return "P";
            if (codigo == "C" || codigo == "CONCLUIDO" || codigo == "CONCLUÍDO")
                return "C";
            if (codigo == "X" || codigo == "CANCELADO")
                return "X";
            return null;
        }

        private static IQueryable<AtendimentoDTO> Projetar(IQueryable<Atendimento> consulta) =>
            consulta.Select(a => new AtendimentoDTO
            {
                IdAtendimento = a.Idatendimento,
                IdPessoa = a.Idpessoa,
                NomePessoa = a.IdpessoaNavigation.Nome,
                DataAtendimento = a.DataAtendimento,
                StatusAtendimento = a.StatusAtendimento,
                ValorTotal = a.Atendimentoitems.Sum(i =>
                    (decimal?)(i.QuantidadeItens * i.ValorunitarioEntrada)) ?? 0,
                Itens = a.Atendimentoitems.Select(i => new AtendimentoItemDTO
                {
                    IdAtendimento = i.Idatendimento,
                    IdItem = i.Iditem,
                    IdFuncionario = i.Idfuncionario,
                    QuantidadeItens = i.QuantidadeItens,
                    ValorUnitarioEntrada = i.ValorunitarioEntrada,
                    Item = i.IditemNavigation.Item1,
                    Cargo = i.IdfuncionarioNavigation.Cargo,
                    NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome
                }).ToList()
            });
    }
}
