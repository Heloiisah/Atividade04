import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { CarrinhoService } from '../servicos/carrinho.service';
import { Funcionario, Identificacao, PedidoCompraItem } from '../modelos/modelos';
import { IconeComponent } from '../componentes/icone.component';
import { imagemItem } from '../modelos/imagem-item';

@Component({
  selector: 'app-carrinho',
  standalone: true,
  imports: [CommonModule, FormsModule, IconeComponent],
  templateUrl: './carrinho.component.html'
})
export class CarrinhoComponent implements OnInit {
  @Input({ required: true }) identidade!: Identificacao;
  @Output() confirmado = new EventEmitter<void>();
  @Output() continuar = new EventEmitter<void>();
  readonly imagemItem = imagemItem;
  funcionarios: Funcionario[] = [];

  constructor(
    private api: ApiService,
    public carrinho: CarrinhoService,
    private aviso: AvisoService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      this.funcionarios = await firstValueFrom(this.api.funcionarios());
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  quantidadeValida(): boolean {
    for (const linha of this.carrinho.itens) {
      if (!linha.quantidade || linha.quantidade <= 0) {
        return false;
      }

      if (linha.item.tipoItem === 'P') {
        let estoque = linha.item.quantidadeEstoque;
        if (estoque === undefined) {
          estoque = 0;
        }

        if (linha.quantidade > estoque) {
          return false;
        }
      }
    }

    return true;
  }

  async confirmar(): Promise<void> {
    if (!this.identidade.idPessoa) {
      this.aviso.avisar('Identifique o cliente antes de confirmar.', true);
      return;
    }
    if (this.carrinho.itens.length === 0 || !this.quantidadeValida()) {
      this.aviso.avisar('Confira quantidades e funcionários dos itens.', true);
      return;
    }
    const itens: PedidoCompraItem[] = [];

    for (const linha of this.carrinho.itens) {
      if (!linha.idFuncionario) {
        this.aviso.avisar('Confira quantidades e funcionários dos itens.', true);
        return;
      }

      itens.push({
        idItem: linha.item.idItem,
        idFuncionario: linha.idFuncionario,
        quantidade: linha.quantidade
      });
    }

    try {
      const pedido = await firstValueFrom(this.api.confirmar(this.identidade, itens));
      this.carrinho.limpar();
      this.confirmado.emit();
      this.aviso.avisar(`Pedido ${pedido.idAtendimento} registrado em aberto.`);
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }
}
