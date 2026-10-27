using AplicacaoBackAula.DTO;


public class FuncionarioPutDTO
{
    public string Cargo { get; set; }
    public DateOnly DataAdmissao { get; set; }
    public PessoaDTO Pessoa { get; set; }
}
