import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { CarrinhoService } from '../servicos/carrinho.service';
import { ItemCardComponent } from '../componentes/item-card.component';
import { CatalogoItem, Categoria } from '../modelos/modelos';

@Component({
  selector: 'app-catalogo', standalone: true,
  imports: [CommonModule, FormsModule, ItemCardComponent],
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

  constructor(private api: ApiService, private carrinho: CarrinhoService,
    private aviso: AvisoService) {}

  async ngOnInit(): Promise<void> {
    this.carregando = true;
    try {
      await this.carregarCatalogo();
      this.categorias = await firstValueFrom(this.api.categorias());
    } catch (erro) { this.aviso.mostrarErro(erro); }
    finally { this.carregando = false; }
  }

  async carregarCatalogo(): Promise<void> {
    try { this.itens = await firstValueFrom(this.api.catalogo(this.busca, this.filtroCategoria, this.filtroTipo)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async consultarCatalogo(id: number): Promise<void> {
    try { this.itemDetalhe = await firstValueFrom(this.api.catalogoItem(id)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  adicionar(item: CatalogoItem): void {
    const atual = this.carrinho.itens.find(i => i.item.idItem === item.idItem);
    if (item.tipoItem === 'P' && (atual?.quantidade ?? 0) >= (item.quantidadeEstoque ?? 0)) {
      this.aviso.avisar('Quantidade disponível atingida.', true); return;
    }
    this.carrinho.adicionar(item);
    this.aviso.avisar('Item adicionado ao carrinho.');
  }
}

