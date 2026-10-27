namespace AplicacaoBackAula.DTO
{
    public class ProdutoItemPostDTO
    {
        public int Idcategoria { get; set; }
        public string Item { get; set; }
        public string DescricaoItem { get; set; }
        public decimal PrecoItem { get; set; }
        public string TipoItem { get; set; }
        public string AtivoProdutoServico { get; set; }
        public int CodigoPreco { get; set; }
        public decimal PrecoVenda { get; set; }
        public string Unidade { get; set; }
    }
}