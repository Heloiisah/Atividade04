import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { CarrinhoService } from '../servicos/carrinho.service';
import { Funcionario, Identificacao } from '../modelos/modelos';

@Component({
  selector: 'app-carrinho', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './carrinho.component.html'
})
export class CarrinhoComponent implements OnInit {
  @Input({ required: true }) identidade!: Identificacao;
  @Output() confirmado = new EventEmitter<void>();
  funcionarios: Funcionario[] = [];

  constructor(private api: ApiService, public carrinho: CarrinhoService,
    private aviso: AvisoService) {}

  async ngOnInit(): Promise<void> {
    try { this.funcionarios = await firstValueFrom(this.api.funcionarios()); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  quantidadeValida(): boolean {
    return this.carrinho.itens.every(i => i.quantidade > 0 &&
      (i.item.tipoItem !== 'P' || i.quantidade <= (i.item.quantidadeEstoque ?? 0)));
  }

  async confirmar(): Promise<void> {
    if (!this.identidade.idPessoa) {
      this.aviso.avisar('Identifique o cliente antes de confirmar.', true); return;
    }
    if (!this.carrinho.itens.length || !this.quantidadeValida() ||
        this.carrinho.itens.some(i => !i.idFuncionario)) {
      this.aviso.avisar('Confira quantidades e funcionários dos itens.', true); return;
    }
    try {
      const pedido = await firstValueFrom(this.api.confirmar(this.identidade,
        this.carrinho.itens.map(i => ({ idItem: i.item.idItem,
          idFuncionario: i.idFuncionario, quantidade: i.quantidade }))));
      this.carrinho.limpar();
      this.confirmado.emit();
      this.aviso.avisar(`Pedido ${pedido.idAtendimento} registrado em aberto.`);
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }
}

