using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionarioController : ControllerBase
    {
        // DEFININDO ATRIBUTO DO CONTEXT
        private readonly BdAulaBackendContext _context;

        // MÉTODO CONSTRUTOR
        public FuncionarioController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA TODOS OS FUNCIONÁRIOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioDTO>>> GetFuncionarios()
        {
            var funcionarios = await _context.Funcionarios
                .Select(f => new FuncionarioDTO
                {
                    IdPessoa = f.Idpessoa,
                    Cargo = f.Cargo,
                    DataAdmissao = f.DataAdmissao,
                    Pessoa = new PessoaDTO
                    {
                        IdPessoa = f.IdpessoaNavigation.Idpessoa,
                        Nome = f.IdpessoaNavigation.Nome,
                        CPF = f.IdpessoaNavigation.Cpf,
                        DataNascimento = f.IdpessoaNavigation.DataNascimento,
                        Sexo = f.IdpessoaNavigation.Sexo,
                        Email = f.IdpessoaNavigation.Email,
                        Telefone = f.IdpessoaNavigation.Telefone,
                        AtivoPessoa = f.IdpessoaNavigation.AtivoPessoa,
                        Enderecos = f.IdpessoaNavigation.Enderecos.Select(e => new EnderecoDTO
                        {
                            IdEndereco = e.Idendereco,
                            IdPessoa = e.Idpessoa,
                            CEP = e.Cep,
                            Numero = e.Numero,
                            Complemento = e.Complemento
                        }).ToList()
                    }
                }).ToListAsync();

            return Ok(funcionarios);
        }

        // BUSCA UM FUNCIONÁRIO PELO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<FuncionarioDTO>> GetFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios
                .Where(f => f.Idpessoa == id)
                .Select(f => new FuncionarioDTO
                {
                    IdPessoa = f.Idpessoa,
                    Cargo = f.Cargo,
                    DataAdmissao = f.DataAdmissao,
                    Pessoa = new PessoaDTO
                    {
                        IdPessoa = f.IdpessoaNavigation.Idpessoa,
                        Nome = f.IdpessoaNavigation.Nome,
                        CPF = f.IdpessoaNavigation.Cpf,
                        DataNascimento = f.IdpessoaNavigation.DataNascimento,
                        Sexo = f.IdpessoaNavigation.Sexo,
                        Email = f.IdpessoaNavigation.Email,
                        Telefone = f.IdpessoaNavigation.Telefone,
                        AtivoPessoa = f.IdpessoaNavigation.AtivoPessoa,
                        Enderecos = f.IdpessoaNavigation.Enderecos.Select(e => new EnderecoDTO
                        {
                            IdEndereco = e.Idendereco,
                            IdPessoa = e.Idpessoa,
                            CEP = e.Cep,
                            Numero = e.Numero,
                            Complemento = e.Complemento
                        }).ToList()
                    }
                }).FirstOrDefaultAsync();

            if (funcionario == null)
            {
                return NotFound();
            }

            return Ok(funcionario);
        }

        // ADICIONAR UM FUNCIONÁRIO
        [HttpPost]
        public async Task<ActionResult<FuncionarioDTO>> PostFuncionario(FuncionarioPostDTO dto)
        {
            // VERIFICA SE A PESSOA E OS ENDEREÇOS FORAM INFORMADOS
            if (dto.Pessoa == null || dto.Pessoa.Enderecos == null)
            {
                return BadRequest("Dados da pessoa incompletos.");
            }

            if (string.IsNullOrWhiteSpace(dto.Pessoa.Nome) ||
                dto.Pessoa.CPF < 10000000000 || dto.Pessoa.CPF > 99999999999 ||
                dto.Pessoa.Telefone <= 0 || dto.Pessoa.DataNascimento == default ||
                string.IsNullOrWhiteSpace(dto.Pessoa.Sexo) ||
                string.IsNullOrWhiteSpace(dto.Pessoa.Email) || !dto.Pessoa.Email.Contains('@') ||
                string.IsNullOrWhiteSpace(dto.Cargo) || dto.DataAdmissao == default ||
                dto.Pessoa.Enderecos.Count == 0)
            {
                return BadRequest("Dados do funcionário incompletos.");
            }

            if (await _context.Pessoas.AnyAsync(p => p.Cpf == dto.Pessoa.CPF ||
                p.Email == dto.Pessoa.Email))
            {
                return BadRequest("CPF ou e-mail já cadastrado.");
            }

            // PERCORRE OS ENDEREÇOS RECEBIDOS PELO CLIENTE
            foreach (var endereco in dto.Pessoa.Enderecos)
            {
                // VERIFICA OS DADOS DO ENDEREÇO
                if (endereco.CEP == null || endereco.CEP == "" ||
                    endereco.Numero == null || endereco.Numero == "" ||
                    endereco.Complemento == null)
                {
                    return BadRequest("Endereco incompleto.");
                }
            }

            // CRIA O OBJETO PESSOA
            var pessoa = new Pessoa
            {
                Nome = dto.Pessoa.Nome,
                Cpf = dto.Pessoa.CPF,
                DataNascimento = dto.Pessoa.DataNascimento,
                Sexo = dto.Pessoa.Sexo,
                Email = dto.Pessoa.Email,
                Telefone = dto.Pessoa.Telefone,
                AtivoPessoa = dto.Pessoa.AtivoPessoa,

                Enderecos = dto.Pessoa.Enderecos.Select(e => new Endereco
                {
                    Cep = e.CEP,
                    Numero = e.Numero,
                    Complemento = e.Complemento
                }).ToList()
            };

            // CRIA O OBJETO FUNCIONÁRIO E VINCULA À PESSOA
            var funcionario = new Funcionario
            {
                Cargo = dto.Cargo,
                DataAdmissao = dto.DataAdmissao,
                IdpessoaNavigation = pessoa
            };

            // PREPARA OS DADOS PARA SEREM ENVIADOS AO BANCO
            _context.Funcionarios.Add(funcionario);

            // SALVA OS DADOS NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel cadastrar o funcionario.");
            }

            // PREPARA OS DADOS PARA RETORNAR AO CLIENTE
            var resposta = new FuncionarioDTO
            {
                IdPessoa = pessoa.Idpessoa,
                Cargo = funcionario.Cargo,
                DataAdmissao = funcionario.DataAdmissao,
                Pessoa = new PessoaDTO
                {
                    IdPessoa = pessoa.Idpessoa,
                    Nome = pessoa.Nome,
                    CPF = pessoa.Cpf,
                    DataNascimento = pessoa.DataNascimento,
                    Sexo = pessoa.Sexo,
                    Email = pessoa.Email,
                    Telefone = pessoa.Telefone,
                    AtivoPessoa = pessoa.AtivoPessoa,

                    Enderecos = pessoa.Enderecos.Select(e => new EnderecoDTO
                    {
                        IdEndereco = e.Idendereco,
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                }
            };

            // RETORNA OS DADOS CADASTRADOS
            return CreatedAtAction(
                nameof(GetFuncionario),
                new { id = funcionario.Idpessoa },
                resposta
            );
        }

        // ALTERAR FUNCIONÁRIO
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFuncionario(int id, FuncionarioPutDTO dto)
        {
            // VERIFICA SE A PESSOA E OS ENDEREÇOS FORAM INFORMADOS
            if (dto.Pessoa == null || dto.Pessoa.Enderecos == null)
            {
                return BadRequest("Dados da pessoa incompletos.");
            }

            if (string.IsNullOrWhiteSpace(dto.Pessoa.Nome) ||
                dto.Pessoa.CPF < 10000000000 || dto.Pessoa.CPF > 99999999999 ||
                dto.Pessoa.Telefone <= 0 || dto.Pessoa.DataNascimento == default ||
                string.IsNullOrWhiteSpace(dto.Pessoa.Sexo) ||
                string.IsNullOrWhiteSpace(dto.Pessoa.Email) || !dto.Pessoa.Email.Contains('@') ||
                string.IsNullOrWhiteSpace(dto.Cargo) || dto.DataAdmissao == default ||
                dto.Pessoa.Enderecos.Count == 0)
            {
                return BadRequest("Dados do funcionário incompletos.");
            }

            // BUSCA O FUNCIONÁRIO NO BANCO DE DADOS
            var funcionario = await _context.Funcionarios
                .Include(f => f.IdpessoaNavigation)
                .FirstOrDefaultAsync(f => f.Idpessoa == id);

            // VERIFICA SE O FUNCIONÁRIO EXISTE
            if (funcionario == null)
            {
                return NotFound();
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

            var verificarCpf = pessoa.Cpf != dto.Pessoa.CPF;
            var verificarEmail = pessoa.Email != dto.Pessoa.Email;
            if (await _context.Pessoas.AnyAsync(p => p.Idpessoa != id &&
                ((verificarCpf && p.Cpf == dto.Pessoa.CPF) ||
                 (verificarEmail && p.Email == dto.Pessoa.Email))))
            {
                return BadRequest("CPF ou e-mail já cadastrado.");
            }

            // PERCORRE OS ENDEREÇOS RECEBIDOS PELO CLIENTE
            foreach (var endereco in dto.Pessoa.Enderecos)
            {
                // VERIFICA OS DADOS DO ENDEREÇO
                if (endereco.CEP == null || endereco.CEP == "" ||
                    endereco.Numero == null || endereco.Numero == "" ||
                    endereco.Complemento == null)
                {
                    return BadRequest("Endereco incompleto.");
                }

                // VERIFICA SE O ENDEREÇO PERTENCE À PESSOA
                if (endereco.IdEndereco > 0 &&
                    !pessoa.Enderecos.Any(e => e.Idendereco == endereco.IdEndereco))
                {
                    return BadRequest("O endereco nao pertence a esse funcionario.");
                }
            }

            // ALTERA OS DADOS DO FUNCIONÁRIO
            funcionario.Cargo = dto.Cargo;
            funcionario.DataAdmissao = dto.DataAdmissao;

            // ALTERA OS DADOS DA PESSOA
            pessoa.Nome = dto.Pessoa.Nome;
            pessoa.Cpf = dto.Pessoa.CPF;
            pessoa.DataNascimento = dto.Pessoa.DataNascimento;
            pessoa.Sexo = dto.Pessoa.Sexo;
            pessoa.Email = dto.Pessoa.Email;
            pessoa.Telefone = dto.Pessoa.Telefone;
            pessoa.AtivoPessoa = dto.Pessoa.AtivoPessoa;

            // IDENTIFICA E REMOVE OS ENDEREÇOS QUE NÃO FORAM RECEBIDOS
            foreach (var endereco in pessoa.Enderecos.ToList())
            {
                if (!dto.Pessoa.Enderecos.Any(e => e.IdEndereco == endereco.Idendereco))
                {
                    _context.Enderecos.Remove(endereco);
                }
            }

            // PERCORRE OS ENDEREÇOS RECEBIDOS
            foreach (var dadosEndereco in dto.Pessoa.Enderecos)
            {
                // VERIFICA SE O ENDEREÇO JÁ POSSUI ID
                if (dadosEndereco.IdEndereco > 0)
                {
                    // BUSCA O ENDEREÇO EXISTENTE
                    var endereco = pessoa.Enderecos
                        .FirstOrDefault(e => e.Idendereco == dadosEndereco.IdEndereco);

                    if (endereco == null)
                    {
                        return BadRequest("O endereco nao pertence a esse funcionario.");
                    }

                    // ALTERA OS DADOS DO ENDEREÇO
                    endereco.Cep = dadosEndereco.CEP;
                    endereco.Numero = dadosEndereco.Numero;
                    endereco.Complemento = dadosEndereco.Complemento;
                }
                else
                {
                    // ADICIONA UM NOVO ENDEREÇO
                    pessoa.Enderecos.Add(new Endereco
                    {
                        Cep = dadosEndereco.CEP,
                        Numero = dadosEndereco.Numero,
                        Complemento = dadosEndereco.Complemento
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
                return StatusCode(409, "Nao foi possivel atualizar o funcionario.");
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }

        // EXCLUIR FUNCIONÁRIO
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            // VERIFICA SE O FUNCIONÁRIO EXISTE
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Idpessoa == id);

            if (funcionario == null)
            {
                return NotFound();
            }

            // VERIFICA SE O FUNCIONÁRIO POSSUI ATENDIMENTOS VINCULADOS
            if (_context.Atendimentoitems.Any(a => a.Idfuncionario == id) ||
                _context.Atendimentos.Any(a => a.Idpessoa == id))
            {
                return StatusCode(409, "O funcionario possui atendimentos vinculados.");
            }

            // BUSCA A PESSOA E SEUS ENDEREÇOS
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            if (pessoa == null)
            {
                return NotFound();
            }

            // PREPARA A EXCLUSÃO DOS ENDEREÇOS
            foreach (var endereco in pessoa.Enderecos.ToList())
            {
                _context.Enderecos.Remove(endereco);
            }

            // PREPARA A EXCLUSÃO DO FUNCIONÁRIO E DA PESSOA
            _context.Funcionarios.Remove(funcionario);
            _context.Pessoas.Remove(pessoa);

            // SALVA AS EXCLUSÕES NO BANCO DE DADOS
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(409, "Nao foi possivel excluir o funcionario.");
            }

            // NÃO RETORNA CONTEÚDO
            return NoContent();
        }
    }
}
