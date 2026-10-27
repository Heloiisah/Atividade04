namespace AplicacaoBackAula.DTO
{
    public class ProdutoDTO
    {
        public int IdItem { get; set; }
        // DADOS TABELA ITEM
        public int Idcategoria { get; set; }
        public string? Item { get; set; }
        public string? DescricaoItem { get; set; }
        public decimal PrecoItem { get; set; }
        public string? TipoItem { get; set; }
        public string AtivoProdutoServico { get; set; } = null!;

        //DADOS TABELA PRODUTO
        public int CodigoPreco { get; set; }
        public decimal PrecoVenda { get; set; }
        public string? Unidade { get; set; }
    }
}
