
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnderecoController : ControllerBase
    {
        // DECLARAÇÃO DO ATRIBUTO DO CONTEXT
        private readonly BdAulaBackendContext _context;

        // MÉTODO CONSTRUTOR
        public EnderecoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTAR TODOS OS ENDEREÇOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Endereco>>> GetEnderecos()
        {
            return await _context.Enderecos.ToListAsync();
        }

        // BUSCAR ENDEREÇO PELO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Endereco>> GetEndereco(int id)
        {
            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e => e.Idendereco == id);

            if (endereco == null)
            {
                return NotFound(new
                {
                    mensagem = "ENDEREÇO NÃO LOCALIZADO!!!"
                });
            }

            return Ok(endereco);
        }

        // ADICIONAR ENDEREÇO
        [HttpPost]
        public async Task<ActionResult<Endereco>> PostEndereco(Endereco endereco)
        {
            // PREPARA O ENDEREÇO PARA SER SALVO
            _context.Enderecos.Add(endereco);

            // SALVA NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel cadastrar o endereco.");
            }

            // RETORNA O ENDEREÇO CADASTRADO
            return CreatedAtAction(
                nameof(GetEndereco),
                new { id = endereco.Idendereco },
                endereco
            );
        }

        // ALTERAR ENDEREÇO
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEndereco(int id, Endereco endereco)
        {
            // VERIFICA SE O ID INFORMADO É IGUAL AO ID DO ENDEREÇO
            if (id != endereco.Idendereco)
            {
                return BadRequest();
            }

            // PREPARA O ENDEREÇO PARA ALTERAÇÃO
            _context.Entry(endereco).State = EntityState.Modified;

            // SALVA AS ALTERAÇÕES NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnderecoExiste(id))
                {
                    return NotFound(new
                    {
                        mensagem = "ENDEREÇO NÃO LOCALIZADO!!!"
                    });
                }

                throw;
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel atualizar o endereco.");
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        // EXCLUIR ENDEREÇO
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEndereco(int id)
        {
            // VERIFICA A EXISTÊNCIA DO ENDEREÇO
            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e => e.Idendereco == id);

            if (endereco == null)
            {
                return NotFound(new
                {
                    mensagem = "ENDEREÇO NÃO LOCALIZADO!!!"
                });
            }

            // PREPARA A EXCLUSÃO
            _context.Enderecos.Remove(endereco);

            // CONFIRMA A EXCLUSÃO NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel excluir o endereco.");
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        // VERIFICA SE O ENDEREÇO EXISTE
        private bool EnderecoExiste(int id)
        {
            return _context.Enderecos
                .Any(e => e.Idendereco == id);
        }
    }
}
