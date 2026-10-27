using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtendimentoController : ControllerBase
    {

        //DEFININADO ATRIBUTO DO CONTEXT
        private readonly BdAulaBackendContext _context;


        // METODO CONSTRUTOR
        public AtendimentoController(BdAulaBackendContext context)
        {
            _context = context;
        }


       // LISTA ATENDIMENTOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtendimentoDTO>>> GetAll()
        {
            var atendimentos = await _context.Atendimentos
                .Select(a => new AtendimentoDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,
                    NomePessoa = a.IdpessoaNavigation.Nome,
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
                }).ToListAsync();

            return Ok(atendimentos);
        }
          //
        [HttpGet("{id}")]
        public async Task<ActionResult<AtendimentoDTO>> GetAtendimento(int id)
        {
            var atendimento = await _context.Atendimentos
                .Where(a => a.Idatendimento == id)
                .Select(a => new AtendimentoDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    ValorTotal = a.Atendimentoitems.Sum(i =>
                        (decimal?)(i.QuantidadeItens * i.ValorunitarioEntrada)) ?? 0,
                    Itens = a.Atendimentoitems.Select(i => new AtendimentoItemDTO
                    {
                        IdItem = i.Iditem,
                        IdFuncionario = i.Idfuncionario,
                        QuantidadeItens = i.QuantidadeItens,
                        ValorUnitarioEntrada = i.ValorunitarioEntrada,
                        Item = i.IditemNavigation.Item1,
                        Cargo = i.IdfuncionarioNavigation.Cargo,
                        NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (atendimento == null)
            {
                return NotFound(new
                {
                    mensagem = "ATENDIMENTO NÃO LOCALIZADO!!"
                });
            }

            return Ok(atendimento);
        }
        // ADICIONAR UM ATENDIMENTO
        [HttpPost]
        [NonAction] // PedidosController valida preço, estoque e situação antes de gravar.
        public async Task<ActionResult<AtendimentoDTO>> PostAtendimento(AtendimentoPostDTO dto)
        {

            //VERIFICAR SE O ID DA PESSOA EXISTE

            var pessoa = await _context.Pessoas
                .FirstOrDefaultAsync(a => a.Idpessoa == dto.IdPessoa);

            if (pessoa == null)
            {
                return BadRequest("Pessoa não encontrada.");
            }

            //CRIAR O OBJETO ATENDIMENTO E PROJETA OS DADOS DO DTO NESSE OBJETO
            var atendimento = new Atendimento
            {
                Idpessoa = dto.IdPessoa,
                DataAtendimento = dto.DataAtendimento,
                StatusAtendimento = dto.StatusAtendimento,
                Atendimentoitems = dto.Itens.Select(i => new Atendimentoitem
                {
                    Iditem = i.IdItem,
                    Idfuncionario = i.IdFuncionario,
                    QuantidadeItens = i.Quantidade,
                    ValorunitarioEntrada = i.ValorUnitario
                }).ToList()
            };


            //PREPARA OS DADOS PARA SEREM ENVIADOS AO BANCO DE DADOS 
            _context.Atendimentos.Add(atendimento);

            //SALVA OS DADOS NO BANCO DE DADOS
            await _context.SaveChangesAsync();

            //RETORNA OS DADOS QUE FORAM ENVIADOS AO BANCO DE DADOS UTILIZANDO GetAtendimento 
            return CreatedAtAction(nameof(GetAtendimento),
                new { id = atendimento.Idatendimento }, dto);
        }

        //ALTERAR ATENDIMENTO
        [HttpPut("{id}")]
        [NonAction] // Alterar os itens aqui não devolve estoque no banco original.
        public async Task<IActionResult> PutAtendimento(int id, AtendimentoPutDTO dto)
        {

            var atendimento = await _context.Atendimentos
                .Include(a => a.Atendimentoitems)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);

            if (atendimento == null)
            {
                return NotFound(new
                {
                    mensagem = "Atendimento não Localizado!!!"
                });
            }


            atendimento.StatusAtendimento = dto.StatusAtendimento;
            //PEGANDO OS IDS DE IdItem e IdFuncionario DO dto PASSO PELO CLIENTE
            var ItensRecebidos = dto.Itens
                .Select(i => (i.IdItem, i.IdFuncionario)).ToHashSet();

            //COMPARAR OS IDS DE IdItem e IdFuncionario COM O DO BANCO DE DADOS.
            var ItensSelecionadosRemover = atendimento.Atendimentoitems
                .Where(i => !ItensRecebidos.Contains((i.Iditem, i.Idfuncionario))).ToList();

            //EXCLUI OS IDS QUE NÃO FORAM PASSADOS PELO CLIENTE VIA dto
            _context.Atendimentoitems. RemoveRange(ItensSelecionadosRemover);

            //PECORRE OS ITENS DO ATENDIMENTO PASSADO PELO CLIENTE
            foreach (var itemDto in dto.Itens)
            {

             //VERIFICAR A EXISTENCIA DO ITEM NO BANCO DE DADOS
              var itemExistente = atendimento.Atendimentoitems
                 .FirstOrDefault(i =>
              i.Iditem == itemDto.IdItem &&
               i.Idfuncionario == itemDto.IdFuncionario);
                //CASO ENCONTRE O ITEM ALTERA APENAS A QUANTIDADE E VALOR UNITÁRIO
                if (itemExistente != null)
                {
                    itemExistente.QuantidadeItens = itemDto.Quantidade;
                    itemExistente.ValorunitarioEntrada = itemDto.ValorUnitario;
                }
                else
                {
                    //CASO NÃO ENCONTRE O ITEM ADICIONA UM NOVO ITEM NO BANCO DE DADOS
                    atendimento.Atendimentoitems.Add(new Atendimentoitem
                    {
                        Idatendimento = atendimento.Idatendimento,
                        Iditem = itemDto.IdItem,
                        Idfuncionario = itemDto.IdFuncionario,
                        QuantidadeItens = itemDto.Quantidade,
                        ValorunitarioEntrada = itemDto.ValorUnitario
                    });
                }

            }

            //SALVA NO BANCO DE DADOS
            await _context.SaveChangesAsync();

            //NÃO RETORNA NADA
            return NoContent();

        }


        [HttpDelete("{id:int}")]
        [NonAction] // O cancelamento em PedidosController devolve os produtos ao estoque.
        public async Task<ActionResult> DeleteAtendimento(int id)
        {
            //VERIFICA A EXISTENCIA DO ATENDIMENTO
            var atendimento = await _context.Atendimentos
                .Include(a => a.Atendimentoitems)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);

            //CASO NÃO ENCONTRE O ATENDIMENTO PASSADO PELO CLIENTE VIA ID NO PARÂMETRO
            //EMITE MENSAGEM ABAIXO
            if (atendimento == null)
            {
                return NotFound(new
                {
                    mensagem = "ATENDIMENTO NÃO LOCALIZADO!!!"
                });
            }

            //PREPARA A EXCLUSÃO OS ITENS NA TABELA AtendimentoItem
            _context.Atendimentoitems.RemoveRange(atendimento.Atendimentoitems);

            //PREPARA A EXCLUSÃO OS ITENS NA TABELA Atendimento
            _context.Atendimentos.Remove(atendimento);

            //CONFIRMA AS EXCLUSÕES
            await _context.SaveChangesAsync();

            //NÃO RETORNA NADA
            return NoContent();
        }
    }
  }

