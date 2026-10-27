using System;
using System.Collections.Generic;

namespace AplicacaoBackAula.Models;

public partial class VwServicosCategoria
{
    public int Iditem { get; set; }

    public string Item { get; set; } = null!;

    public string DescricaoItem { get; set; } = null!;

    public decimal Preco { get; set; }

    public int Idcategoria { get; set; }

    public string Categoria { get; set; } = null!;
}
