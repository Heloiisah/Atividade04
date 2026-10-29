import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Estoque, Pedido, Pessoa, Produto, Servico } from '../modelos/modelos';

@Component({
  selector: 'app-painel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './painel.component.html'
})
export class PainelComponent implements OnInit {
  pessoas: Pessoa[] = [];
  produtos: Produto[] = [];
  servicos: Servico[] = [];
  pedidos: Pedido[] = [];
  estoques: Estoque[] = [];

  constructor(
    private api: ApiService,
    private aviso: AvisoService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      const pessoas = await firstValueFrom(this.api.pessoas());
      const funcionarios = await firstValueFrom(this.api.funcionarios());
      this.pessoas = [];

      for (const pessoa of pessoas) {
        let funcionarioEncontrado = false;

        for (const funcionario of funcionarios) {
          if (funcionario.idPessoa === pessoa.idPessoa) {
            funcionarioEncontrado = true;
            break;
          }
        }

        if (!funcionarioEncontrado) {
          this.pessoas.push(pessoa);
        }
      }

      this.produtos = await firstValueFrom(this.api.produtos());
      this.servicos = await firstValueFrom(this.api.servicos());
      this.pedidos = await firstValueFrom(this.api.pedidos());
      this.estoques = await firstValueFrom(this.api.estoque());
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  get pedidosAbertos(): number {
    let total = 0;

    for (const pedido of this.pedidos) {
      if (pedido.statusAtendimento === 'A') {
        total++;
      }
    }

    return total;
  }

  nomeStatus(status: string): string {
    if (status === 'A') {
      return 'Aberto';
    }
    if (status === 'P') {
      return 'Em processamento';
    }
    if (status === 'C') {
      return 'Concluído';
    }
    if (status === 'X') {
      return 'Cancelado';
    }

    return status;
  }
}
