namespace AplicacaoBackAula.DTO
{
    public class PedidoCompraDTO
    {
        public int IdPessoa { get; set; }
        public long CPF { get; set; }
        public long Telefone { get; set; }
        public string Email { get; set; } = "";
        public List<PedidoCompraItemDTO> Itens { get; set; } = new();
    }

    public class PedidoCompraItemDTO
    {
        public int IdItem { get; set; }
        public int IdFuncionario { get; set; }
        public int Quantidade { get; set; }
    }

    public class PedidoStatusDTO
    {
        public string StatusAtendimento { get; set; } = "";
    }

    public class IdentificacaoClienteDTO
    {
        public int IdPessoa { get; set; }
        public long CPF { get; set; }
        public long Telefone { get; set; }
        public string Email { get; set; } = "";
    }
}
