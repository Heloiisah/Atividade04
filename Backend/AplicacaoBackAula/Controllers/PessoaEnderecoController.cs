using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using System;

namespace AplicacaoBackAula.Controllers
{
    // CONFIGURAÇÃO DA API
    [ApiController]
    [Route("api/[controller]")]
    public class PessoaEnderecoController : ControllerBase
    {
        // DEFININDO ATRIBUTO DO CONTEXT
        private readonly BdAulaBackendContext _context;

        // MÉTODO CONSTRUTOR
        public PessoaEnderecoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA TODAS AS PESSOAS COM SEUS RESPECTIVOS ENDEREÇOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaDTO>>> GetAll()
        {
            var pessoas = await _context.Pessoas
                .Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                    {
                        IdEndereco = e.Idendereco,
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                }).ToListAsync();

            return Ok(pessoas);
        }

        // BUSCA UMA PESSOA PELO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaDTO>> GetPessoa(int id)
        {
            var pessoa = await _context.Pessoas
                .Where(p => p.Idpessoa == id)
                .Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                    {
                        IdEndereco = e.Idendereco,
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            // CASO NÃO ENCONTRE A PESSOA
            if (pessoa == null)
            {
                return NotFound();
            }

            return Ok(pessoa);
        }

        // ADICIONAR UMA PESSOA COM SEUS ENDEREÇOS
        [HttpPost]
        public async Task<ActionResult<PessoaDTO>> Post(PessoaDTO dto)
        {
            // VERIFICA SE OS DADOS DA PESSOA ESTÃO COMPLETOS
            if (dto.Enderecos == null || dto.Nome == null || dto.Nome == "" ||
                dto.CPF < 10000000000 || dto.CPF > 99999999999 ||
                dto.Telefone <= 0 || dto.DataNascimento.Year < 1000 ||
                dto.DataNascimento > DateOnly.FromDateTime(DateTime.Today) ||
                dto.Sexo == null || dto.Sexo == "" || dto.Email == null || !dto.Email.Contains('@') ||
                dto.AtivoPessoa == null || dto.AtivoPessoa == "")
            {
                return BadRequest("Dados da pessoa incompletos.");
            }

            // PERCORRE OS ENDEREÇOS RECEBIDOS PELO CLIENTE
            foreach (var endereco in dto.Enderecos)
            {
                // VERIFICA SE OS DADOS DO ENDEREÇO ESTÃO COMPLETOS
                if (endereco.CEP == null || endereco.CEP.Length != 8 ||
                    endereco.Numero == null || endereco.Numero == "" ||
                    endereco.Complemento == null)
                {
                    return BadRequest("Endereco incompleto.");
                }
            }

            if (await _context.Pessoas.AnyAsync(p => p.Cpf == dto.CPF || p.Email == dto.Email))
                return BadRequest("CPF ou e-mail já cadastrado.");

            // CRIA O OBJETO PESSOA E PROJETA OS DADOS DO DTO NESSE OBJETO
            var ObjPessoa = new Pessoa
            {
                Nome = dto.Nome,
                Cpf = dto.CPF,
                DataNascimento = dto.DataNascimento,
                Sexo = dto.Sexo,
                Email = dto.Email,
                Telefone = dto.Telefone,
                AtivoPessoa = dto.AtivoPessoa,

                Enderecos = dto.Enderecos.Select(e => new Endereco
                {
                    Cep = e.CEP,
                    Numero = e.Numero,
                    Complemento = e.Complemento
                }).ToList()
            };

            // PREPARA OS DADOS PARA SEREM ENVIADOS AO BANCO DE DADOS
            _context.Pessoas.Add(ObjPessoa);

            // SALVA OS DADOS NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel cadastrar a pessoa.");
            }

            // PREPARA OS DADOS RETORNADOS PELO BANCO DE DADOS
            var respostaBd = new PessoaDTO
            {
                IdPessoa = ObjPessoa.Idpessoa,
                Nome = ObjPessoa.Nome,
                CPF = ObjPessoa.Cpf,
                DataNascimento = ObjPessoa.DataNascimento,
                Sexo = ObjPessoa.Sexo,
                Email = ObjPessoa.Email,
                Telefone = ObjPessoa.Telefone,
                AtivoPessoa = ObjPessoa.AtivoPessoa,

                Enderecos = ObjPessoa.Enderecos.Select(e => new EnderecoDTO
                {
                    IdEndereco = e.Idendereco,
                    IdPessoa = e.Idpessoa,
                    Numero = e.Numero,
                    CEP = e.Cep,
                    Complemento = e.Complemento
                }).ToList()
            };

            // RETORNA OS DADOS CADASTRADOS
            return CreatedAtAction(
                nameof(GetPessoa),
                new { id = ObjPessoa.Idpessoa },
                respostaBd
            );
        }

        // ALTERAR PESSOA E SEUS ENDEREÇOS
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, PessoaDTO dto)
        {
            // VERIFICA SE OS DADOS DA PESSOA ESTÃO COMPLETOS
            if (dto.Enderecos == null || dto.Nome == null || dto.Nome == "" ||
                dto.CPF < 10000000000 || dto.CPF > 99999999999 ||
                dto.Telefone <= 0 || dto.DataNascimento.Year < 1000 ||
                dto.DataNascimento > DateOnly.FromDateTime(DateTime.Today) ||
                dto.Sexo == null || dto.Sexo == "" || dto.Email == null || !dto.Email.Contains('@') ||
                dto.AtivoPessoa == null || dto.AtivoPessoa == "")
            {
                return BadRequest("Dados da pessoa incompletos.");
            }

            // BUSCA A PESSOA E SEUS ENDEREÇOS
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            // VERIFICA SE A PESSOA EXISTE
            if (pessoa == null)
            {
                return NotFound();
            }

            var verificarCpf = pessoa.Cpf != dto.CPF;
            var verificarEmail = pessoa.Email != dto.Email;
            if (await _context.Pessoas.AnyAsync(p => p.Idpessoa != id &&
                ((verificarCpf && p.Cpf == dto.CPF) ||
                 (verificarEmail && p.Email == dto.Email))))
                return BadRequest("CPF ou e-mail já cadastrado.");

            // PROJETA OS DADOS DO DTO PARA O MODELO PESSOA
            pessoa.Nome = dto.Nome;
            pessoa.Cpf = dto.CPF;
            pessoa.DataNascimento = dto.DataNascimento;
            pessoa.Sexo = dto.Sexo;
            pessoa.Email = dto.Email;
            pessoa.Telefone = dto.Telefone;
            pessoa.AtivoPessoa = dto.AtivoPessoa;

            // GUARDA OS IDS DOS ENDEREÇOS RECEBIDOS
            var idsRecebidos = dto.Enderecos
                .Where(e => e.IdEndereco.HasValue)
                .Select(e => e.IdEndereco!.Value)
                .ToHashSet();

            // IDENTIFICA OS ENDEREÇOS QUE DEVEM SER REMOVIDOS
            var enderecosRemover = pessoa.Enderecos
                .Where(e => !idsRecebidos.Contains(e.Idendereco))
                .ToList();

            // PREPARA A EXCLUSÃO DOS ENDEREÇOS REMOVIDOS PELO CLIENTE
            _context.Enderecos.RemoveRange(enderecosRemover);

            // PERCORRE OS ENDEREÇOS DA PESSOA
            foreach (var endPessoa in dto.Enderecos)
            {
                // VERIFICA SE OS DADOS DO ENDEREÇO ESTÃO COMPLETOS
                if (endPessoa.CEP == null || endPessoa.CEP.Length != 8 ||
                    endPessoa.Numero == null || endPessoa.Numero == "" ||
                    endPessoa.Complemento == null)
                {
                    return BadRequest("Endereco incompleto.");
                }

                // VERIFICA SE O ENDEREÇO JÁ POSSUI ID
                if (endPessoa.IdEndereco > 0)
                {
                    // BUSCA O ENDEREÇO NA LISTA DE ENDEREÇOS DA PESSOA
                    var endereco = pessoa.Enderecos
                        .FirstOrDefault(e => e.Idendereco == endPessoa.IdEndereco);

                    // CASO O ENDEREÇO NÃO PERTENÇA À PESSOA
                    if (endereco == null)
                    {
                        return BadRequest($"O endereço não pertence a essa pessoa");
                    }

                    // ALTERA OS DADOS DO ENDEREÇO EXISTENTE
                    endereco.Numero = endPessoa.Numero;
                    endereco.Cep = endPessoa.CEP;
                    endereco.Complemento = endPessoa.Complemento;
                }
                else
                {
                    // ADICIONA UM NOVO ENDEREÇO À PESSOA
                    pessoa.Enderecos.Add(new Endereco
                    {
                        Numero = endPessoa.Numero,
                        Cep = endPessoa.CEP,
                        Complemento = endPessoa.Complemento
                    });
                }
            }

            // SALVA AS ALTERAÇÕES NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel atualizar a pessoa.");
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        // EXCLUIR PESSOA E SEUS ENDEREÇOS
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // BUSCA A PESSOA E SEUS ENDEREÇOS
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            // VERIFICA SE A PESSOA EXISTE
            if (pessoa == null)
            {
                return NotFound();
            }

            // VERIFICA SE A PESSOA POSSUI REGISTROS VINCULADOS
            if (_context.Funcionarios.Any(f => f.Idpessoa == id) ||
                _context.Atendimentos.Any(a => a.Idpessoa == id))
            {
                return StatusCode(409, "A pessoa possui registros vinculados.");
            }

            // PREPARA A EXCLUSÃO DOS ENDEREÇOS DA PESSOA
            foreach (var endereco in pessoa.Enderecos.ToList())
            {
                _context.Enderecos.Remove(endereco);
            }

            // PREPARA A EXCLUSÃO DA PESSOA
            _context.Pessoas.Remove(pessoa);

            // SALVA AS EXCLUSÕES NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel excluir a pessoa.");
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }
    }
}
