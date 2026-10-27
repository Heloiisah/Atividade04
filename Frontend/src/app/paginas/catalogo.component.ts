import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { CarrinhoService } from '../servicos/carrinho.service';
import { ItemCardComponent } from '../componentes/item-card.component';
import { CatalogoItem, Categoria } from '../modelos/modelos';
import { IconeComponent } from '../componentes/icone.component';

@Component({
  selector: 'app-catalogo',
  standalone: true,
  imports: [CommonModule, FormsModule, ItemCardComponent, IconeComponent],
  templateUrl: './catalogo.component.html'
})
export class CatalogoComponent implements OnInit {
  busca = '';
  filtroCategoria = 0;
  filtroTipo = '';
  consultaId = 0;
  carregando = false;
  itens: CatalogoItem[] = [];
  categorias: Categoria[] = [];
  itemDetalhe?: CatalogoItem;

  constructor(
    private api: ApiService,
    private carrinho: CarrinhoService,
    private aviso: AvisoService
  ) {}

  async ngOnInit(): Promise<void> {
    this.carregando = true;
    try {
      await this.carregarCatalogo();
      this.categorias = await firstValueFrom(this.api.categorias());
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    } finally {
      this.carregando = false;
    }
  }

  async carregarCatalogo(): Promise<void> {
    this.carregando = true;
    try {
      this.itens = await firstValueFrom(
        this.api.catalogo(this.busca, this.filtroCategoria, this.filtroTipo)
      );
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    } finally {
      this.carregando = false;
    }
  }

  selecionarCategoria(id: number): void {
    this.filtroCategoria = id;
    this.itemDetalhe = undefined;
    void this.carregarCatalogo();
  }

  limparFiltros(): void {
    this.busca = '';
    this.filtroTipo = '';
    this.selecionarCategoria(0);
  }

  async consultarCatalogo(id: number): Promise<void> {
    try {
      this.itemDetalhe = await firstValueFrom(this.api.catalogoItem(id));
      setTimeout(() => {
        const detalhe = document.getElementById('detalhes-item');
        if (detalhe) {
          detalhe.focus();
        }
      });
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  adicionar(item: CatalogoItem): void {
    if (item.tipoItem === 'P') {
      const quantidadeNoCarrinho = this.carrinho.quantidade(item.idItem);
      let estoque = item.quantidadeEstoque;

      if (estoque === undefined) {
        estoque = 0;
      }

      if (quantidadeNoCarrinho >= estoque) {
        this.aviso.avisar('Quantidade disponível atingida.', true);
        return;
      }
    }
    this.carrinho.adicionar(item);
    this.aviso.avisar('Item adicionado ao carrinho.');
  }
}
