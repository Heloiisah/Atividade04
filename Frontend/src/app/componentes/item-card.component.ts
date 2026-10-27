import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CatalogoItem } from '../modelos/modelos';

@Component({
  selector: 'app-item-card', standalone: true, imports: [CommonModule],
  template: `<article class="item-card">
    <span class="tipo">{{item.tipoItem === 'P' ? 'Produto' : 'Serviço'}} / {{item.categoria}}</span>
    <h3>{{item.item}}</h3><p>{{item.descricaoItem}}</p>
    <div class="preco"><strong>{{item.preco | currency:'BRL'}}</strong>
      <small *ngIf="item.tipoItem === 'P'">{{item.quantidadeEstoque || 0}} disponíveis</small></div>
    <button type="button" (click)="adicionar.emit(item)"
      [disabled]="item.tipoItem === 'P' && !item.quantidadeEstoque">Adicionar ao carrinho</button>
  </article>`
})
export class ItemCardComponent {
  @Input({ required: true }) item!: CatalogoItem;
  @Output() adicionar = new EventEmitter<CatalogoItem>();
}

