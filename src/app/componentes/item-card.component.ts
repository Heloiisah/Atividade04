import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CatalogoItem } from '../modelos/modelos';
import { imagemItem } from '../modelos/imagem-item';
import { IconeComponent } from './icone.component';

@Component({
  selector: 'app-item-card',
  standalone: true,
  imports: [CommonModule, IconeComponent],
  template: `
    <article class="item-card">
      <div class="item-imagem">
        <img
          [src]="imagemItem(item)"
          [alt]="
            item.tipoItem === 'P'
              ? 'Imagem ilustrativa de cosméticos'
              : 'Imagem ilustrativa de um tratamento estético'
          "
          loading="lazy"
          width="720"
          height="480"
        />
        <span class="tipo-badge">{{ item.tipoItem === 'P' ? 'Produto' : 'Serviço' }}</span>
      </div>
      <div class="item-conteudo">
        <span class="tipo">{{ item.categoria }}</span>
        <h2>{{ item.item }}</h2>
        <p>{{ item.descricaoItem }}</p>
        <div class="preco">
          <strong>{{ item.preco | currency: 'BRL' }}</strong>
          <small
            *ngIf="item.tipoItem === 'P'"
            [class.sem-estoque]="!item.quantidadeEstoque"
          >
            {{ item.quantidadeEstoque ? item.quantidadeEstoque + ' em estoque' : 'Indisponível' }}
          </small>
          <small *ngIf="item.tipoItem === 'S'">Serviço</small>
        </div>
        <button
          type="button"
          (click)="adicionar.emit(item)"
          [attr.aria-label]="'Adicionar ' + item.item + ' ao carrinho'"
          [disabled]="item.tipoItem === 'P' && !item.quantidadeEstoque"
        >
          <app-icone nome="carrinho"></app-icone>
          Adicionar ao carrinho
        </button>
        <button
          type="button"
          class="ver-detalhes"
          (click)="detalhar.emit(item.idItem)"
          [attr.aria-label]="'Ver detalhes de ' + item.item"
        >
          Ver detalhes
        </button>
      </div>
    </article>
  `
})
export class ItemCardComponent {
  @Input({ required: true }) item!: CatalogoItem;
  @Output() adicionar = new EventEmitter<CatalogoItem>();
  @Output() detalhar = new EventEmitter<number>();
  readonly imagemItem = imagemItem;
}
