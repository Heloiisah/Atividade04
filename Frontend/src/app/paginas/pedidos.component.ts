import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Identificacao, Pedido } from '../modelos/modelos';

@Component({
  selector: 'app-pedidos', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './pedidos.component.html'
})
export class PedidosComponent implements OnInit {
  @Input({ required: true }) identidade!: Identificacao;
  @Input() administracao = false;
  filtroStatus = '';
  consultaId = 0;
  pedidos: Pedido[] = [];
  detalhe?: Pedido;

  constructor(private api: ApiService, private aviso: AvisoService) {}

  ngOnInit(): void {
    if (this.administracao) void this.filtrarPedidos();
    else void this.carregarMeusPedidos();
  }

  async carregarMeusPedidos(): Promise<void> {
    if (!this.identidade.idPessoa) { this.pedidos = []; return; }
    try { this.pedidos = await firstValueFrom(this.api.meusPedidos(this.identidade)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async identificar(): Promise<void> {
    try {
      this.pedidos = await firstValueFrom(this.api.meusPedidos(this.identidade));
      this.aviso.avisar('Cliente identificado.');
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async verPedido(id: number): Promise<void> {
    try { this.detalhe = await firstValueFrom(this.administracao
      ? this.api.pedido(id) : this.api.meuPedido(id, this.identidade)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async filtrarPedidos(): Promise<void> {
    try { this.pedidos = await firstValueFrom(this.api.pedidos(this.filtroStatus)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async mudarStatus(id: number, status: string): Promise<void> {
    try {
      await firstValueFrom(this.api.status(id, status));
      this.pedidos = await firstValueFrom(this.api.pedidos(this.filtroStatus));
      this.detalhe = undefined;
      this.aviso.avisar('Situação do pedido atualizada.');
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  nomeStatus(status: string): string {
    return ({ A: 'Aberto', P: 'Em processamento', C: 'Concluído', X: 'Cancelado' } as Record<string, string>)[status] || status;
  }
}

