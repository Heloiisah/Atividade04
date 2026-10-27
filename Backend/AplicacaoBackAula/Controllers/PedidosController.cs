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
            {
                return BadRequest(new { mensagem = "Situação inválida." });
            }

            var consulta = _context.Atendimentos.AsQueryable();
            if (codigo != null)
            {
                consulta = consulta.Where(a => a.StatusAtendimento == codigo);
            }

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
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            return Ok(pedido);
        }

        // CONFERE OS DADOS INFORMADOS PELO CLIENTE ANTES DE CONSULTAR SEUS PEDIDOS
        [HttpPost("meus")]
        public async Task<ActionResult<IEnumerable<AtendimentoDTO>>> MeusPedidos(IdentificacaoClienteDTO dto)
        {
            if (!await ClienteValido(dto.IdPessoa, dto.CPF, dto.Telefone, dto.Email))
            {
                return BadRequest(new { mensagem = "Dados do cliente não conferem." });
            }

            var pedidos = await Projetar(_context.Atendimentos
                .Where(a => a.Idpessoa == dto.IdPessoa))
                .OrderByDescending(a => a.IdAtendimento).ToListAsync();
            return Ok(pedidos);
        }

        [HttpPost("meu/{id:int}")]
        public async Task<ActionResult<AtendimentoDTO>> MeuPedido(int id, IdentificacaoClienteDTO dto)
        {
            if (!await ClienteValido(dto.IdPessoa, dto.CPF, dto.Telefone, dto.Email))
            {
                return BadRequest(new { mensagem = "Dados do cliente não conferem." });
            }

            var pedido = await Projetar(_context.Atendimentos.Where(a =>
                a.Idatendimento == id && a.Idpessoa == dto.IdPessoa)).FirstOrDefaultAsync();
            if (pedido == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado para este cliente." });
            }

            return Ok(pedido);
        }

        // REGISTRA UM PEDIDO E UTILIZA O TRIGGER DO BANCO PARA BAIXAR O ESTOQUE
        [HttpPost]
        public async Task<ActionResult<AtendimentoDTO>> PostPedido(PedidoCompraDTO dto)
        {
            if (!await ClienteValido(dto.IdPessoa, dto.CPF, dto.Telefone, dto.Email))
            {
                return BadRequest(new { mensagem = "Dados do cliente não conferem." });
            }

            if (dto.Itens.Count == 0)
            {
                return BadRequest(new { mensagem = "Informe itens distintos e quantidades positivas." });
            }

            var ids = new List<int>();
            var funcionarios = new List<int>();

            foreach (var escolhido in dto.Itens)
            {
                if (escolhido.IdItem <= 0 || escolhido.IdFuncionario <= 0 || escolhido.Quantidade <= 0)
                {
                    return BadRequest(new { mensagem = "Informe itens distintos e quantidades positivas." });
                }

                if (ids.Contains(escolhido.IdItem))
                {
                    return BadRequest(new { mensagem = "Informe itens distintos e quantidades positivas." });
                }

                ids.Add(escolhido.IdItem);

                if (!funcionarios.Contains(escolhido.IdFuncionario))
                {
                    funcionarios.Add(escolhido.IdFuncionario);
                }
            }

            var itens = await _context.Items
                .Include(item => item.Produto)
                .ThenInclude(produto => produto!.Estoque)
                .Where(item => ids.Contains(item.Iditem))
                .ToListAsync();

            if (itens.Count != ids.Count)
            {
                return BadRequest(new { mensagem = "Há item inexistente ou inativo." });
            }

            foreach (var item in itens)
            {
                if (item.AtivoProdutoServico != "S")
                {
                    return BadRequest(new { mensagem = "Há item inexistente ou inativo." });
                }

                if (item.TipoItem == "P" && item.Produto == null)
                {
                    return BadRequest(new { mensagem = "Há item inexistente ou inativo." });
                }
            }

            var quantidadeFuncionarios = await _context.Funcionarios.CountAsync(funcionario =>
                funcionarios.Contains(funcionario.Idpessoa) &&
                funcionario.IdpessoaNavigation.AtivoPessoa == "S");

            if (quantidadeFuncionarios != funcionarios.Count)
            {
                return BadRequest(new { mensagem = "Funcionário inexistente ou inativo." });
            }

            var atendimento = new Atendimento
            {
                Idpessoa = dto.IdPessoa,
                DataAtendimento = DateOnly.FromDateTime(DateTime.Today),
                StatusAtendimento = "A"
            };

            foreach (var escolhido in dto.Itens)
            {
                Item? itemEncontrado = null;

                foreach (var item in itens)
                {
                    if (item.Iditem == escolhido.IdItem)
                    {
                        itemEncontrado = item;
                        break;
                    }
                }

                if (itemEncontrado == null)
                {
                    return BadRequest(new { mensagem = "Há item inexistente ou inativo." });
                }

                var valorUnitario = itemEncontrado.Preco;

                if (itemEncontrado.TipoItem == "P")
                {
                    var produto = itemEncontrado.Produto;

                    if (produto == null || produto.Estoque == null ||
                        produto.Estoque.QuantidadeEstoque < escolhido.Quantidade)
                    {
                        return BadRequest(new { mensagem = $"Estoque insuficiente para {itemEncontrado.Item1}." });
                    }

                    valorUnitario = produto.PrecoVenda;
                }

                atendimento.Atendimentoitems.Add(new Atendimentoitem
                {
                    Iditem = escolhido.IdItem,
                    Idfuncionario = escolhido.IdFuncionario,
                    QuantidadeItens = escolhido.Quantidade,
                    ValorunitarioEntrada = valorUnitario
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
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
            {
                return BadRequest(new { mensagem = "Situação inválida." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            var pedido = await _context.Atendimentos
                .Include(a => a.Atendimentoitems).ThenInclude(i => i.IditemNavigation)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);
            if (pedido == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }

            var permitido = false;

            if (pedido.StatusAtendimento == "A")
            {
                permitido = novoStatus == "P" || novoStatus == "X";
            }
            else if (pedido.StatusAtendimento == "P")
            {
                permitido = novoStatus == "C" || novoStatus == "X";
            }
            if (!permitido)
            {
                return BadRequest(new { mensagem = "Transição de situação não permitida." });
            }

            if (novoStatus == "X")
            {
                foreach (var item in pedido.Atendimentoitems)
                {
                    if (item.IditemNavigation.TipoItem != "P")
                    {
                        continue;
                    }

                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e => e.Iditem == item.Iditem);
                    if (estoque == null)
                    {
                        return BadRequest(new { mensagem = "Estoque do produto não encontrado." });
                    }

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
            {
                return null;
            }

            var codigo = status.Trim().ToUpperInvariant();
            if (codigo == "A" || codigo == "ABERTO")
            {
                return "A";
            }

            if (codigo == "P" || codigo == "EM PROCESSAMENTO")
            {
                return "P";
            }

            if (codigo == "C" || codigo == "CONCLUIDO" || codigo == "CONCLUÍDO")
            {
                return "C";
            }

            if (codigo == "X" || codigo == "CANCELADO")
            {
                return "X";
            }

            return null;
        }

        private static IQueryable<AtendimentoDTO> Projetar(IQueryable<Atendimento> consulta)
        {
            return consulta.Select(a => new AtendimentoDTO
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
}
