using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoaController : ControllerBase
    {
        // DEFININDO ATRIBUTO DO CONTEXT
        private readonly BdAulaBackendContext _context;

        // MÉTODO CONSTRUTOR
        public PessoaController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA PESSOAS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pessoa>>> GetPessoas()
        {
            return await _context.Pessoas.ToListAsync();
        }

        // BUSCA PESSOA PELO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Pessoa>> GetPessoa(int id)
        {
            var pessoa = await _context.Pessoas
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            // CASO NÃO ENCONTRE A PESSOA
            if (pessoa == null)
            {
                return NotFound();
            }

            return pessoa;
        }

        // ADICIONAR UMA PESSOA
        [HttpPost]
        public async Task<ActionResult<Pessoa>> PostPessoa(Pessoa pessoa)
        {
            if (await _context.Pessoas.AnyAsync(p => p.Cpf == pessoa.Cpf || p.Email == pessoa.Email))
            {
                return BadRequest("CPF ou e-mail já cadastrado.");
            }

            // PREPARA OS DADOS PARA SEREM ENVIADOS AO BANCO DE DADOS
            _context.Pessoas.Add(pessoa);

            // SALVA OS DADOS NO BANCO DE DADOS
            await _context.SaveChangesAsync();

            // RETORNA OS DADOS CADASTRADOS
            return CreatedAtAction(
                nameof(GetPessoa),
                new { id = pessoa.Idpessoa },
                pessoa
            );
        }

        // ALTERAR PESSOA
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPessoa(
            int id,
            Pessoa pessoa)
        {
            // VERIFICA SE O ID INFORMADO É IGUAL AO ID DA PESSOA
            if (id != pessoa.Idpessoa)
            {
                return BadRequest();
            }

            var atual = await _context.Pessoas.AsNoTracking().FirstOrDefaultAsync(p => p.Idpessoa == id);
            if (atual == null)
            {
                return NotFound();
            }

            var verificarCpf = atual.Cpf != pessoa.Cpf;
            var verificarEmail = atual.Email != pessoa.Email;
            if (await _context.Pessoas.AnyAsync(p => p.Idpessoa != id &&
                ((verificarCpf && p.Cpf == pessoa.Cpf) ||
                 (verificarEmail && p.Email == pessoa.Email))))
            {
                return BadRequest("CPF ou e-mail já cadastrado.");
            }

            // PREPARA A ALTERAÇÃO DOS DADOS DA PESSOA
            _context.Entry(pessoa).State = EntityState.Modified;

            // SALVA AS ALTERAÇÕES NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // VERIFICA SE A PESSOA EXISTE
                if (!PessoaExiste(id))
                {
                    return NotFound();
                }

                throw;
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        // EXCLUIR PESSOA
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePessoa(int id)
        {
            // VERIFICA SE A PESSOA EXISTE NO BANCO DE DADOS
            var pessoa = await _context.Pessoas
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            // CASO NÃO ENCONTRE A PESSOA
            if (pessoa == null)
            {
                return NotFound();
            }

            // PREPARA A EXCLUSÃO DA PESSOA
            _context.Pessoas.Remove(pessoa);

            // SALVA A EXCLUSÃO NO BANCO DE DADOS
            await _context.SaveChangesAsync();

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        // VERIFICA SE A PESSOA EXISTE
        private bool PessoaExiste(int id)
        {
            return _context.Pessoas
                .Any(e => e.Idpessoa == id);
        }
    }
}
