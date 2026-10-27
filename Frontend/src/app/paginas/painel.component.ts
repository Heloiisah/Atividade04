import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Estoque, Pedido, Pessoa, Produto, Servico } from '../modelos/modelos';

@Component({
  selector: 'app-painel', standalone: true, imports: [CommonModule],
  templateUrl: './painel.component.html'
})
export class PainelComponent implements OnInit {
  pessoas: Pessoa[] = [];
  produtos: Produto[] = [];
  servicos: Servico[] = [];
  pedidos: Pedido[] = [];
  estoques: Estoque[] = [];

  constructor(private api: ApiService, private aviso: AvisoService) {}

  async ngOnInit(): Promise<void> {
    try {
      const [pessoas, funcionarios, produtos, servicos, pedidos, estoques] = await Promise.all([
        firstValueFrom(this.api.pessoas()), firstValueFrom(this.api.funcionarios()),
        firstValueFrom(this.api.produtos()), firstValueFrom(this.api.servicos()),
        firstValueFrom(this.api.pedidos()), firstValueFrom(this.api.estoque())
      ]);
      this.pessoas = pessoas.filter(p => !funcionarios.some(f => f.idPessoa === p.idPessoa));
      this.produtos = produtos;
      this.servicos = servicos;
      this.pedidos = pedidos;
      this.estoques = estoques;
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  get pedidosAbertos(): number {
    return this.pedidos.filter(p => p.statusAtendimento === 'A').length;
  }
}

