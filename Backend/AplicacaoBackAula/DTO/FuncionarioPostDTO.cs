namespace AplicacaoBackAula.DTO
{
    public class FuncionarioPostDTO
    {
        public string Cargo { get; set; }
        public DateOnly DataAdmissao { get; set; }
        public PessoaDTO Pessoa { get; set; }
    }
}
