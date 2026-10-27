using System;
using System.Collections.Generic;

namespace AplicacaoBackAula.Models;

public partial class VwProdutosCategoriasEstoque
{
    public int Iditem { get; set; }

    public string Item { get; set; } = null!;

    public string DescricaoItem { get; set; } = null!;

    public int Idcategoria { get; set; }

    public string Categoria { get; set; } = null!;

    public int CodigoPreco { get; set; }

    public decimal PrecoVenda { get; set; }

    public string Unidade { get; set; } = null!;

    public int QuantidadeEstoque { get; set; }
}
