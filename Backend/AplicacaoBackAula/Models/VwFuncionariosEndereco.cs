using System;
using System.Collections.Generic;

namespace AplicacaoBackAula.Models;

public partial class VwFuncionariosEndereco
{
    public int Idpessoa { get; set; }

    public string Nome { get; set; } = null!;

    public long Cpf { get; set; }

    public string Cargo { get; set; } = null!;

    public DateOnly DataAdmissao { get; set; }

    public int Idendereco { get; set; }

    public string Cep { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string Complemento { get; set; } = null!;
}
