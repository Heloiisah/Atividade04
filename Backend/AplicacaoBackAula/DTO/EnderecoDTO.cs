namespace AplicacaoBackAula.DTO
{
    public class EnderecoDTO
    {

        public int? IdEndereco { get; set; }
        public int IdPessoa { get; set; }

        public string CEP { get; set; } = null!;

        public string Numero { get; set; } = null!;

        public string Complemento { get; set; } = null!;
    }
}
