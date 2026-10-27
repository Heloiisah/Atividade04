import { Injectable } from '@angular/core';
import { CarrinhoItem, CatalogoItem } from '../modelos/modelos';

@Injectable({ providedIn: 'root' })
export class CarrinhoService {
  itens: CarrinhoItem[] = [];

  adicionar(item: CatalogoItem): void {
    const atual = this.itens.find(i => i.item.idItem === item.idItem);
    if (atual) atual.quantidade++;
    else this.itens.push({ item, quantidade: 1, idFuncionario: 0 });
  }

  remover(id: number): void { this.itens = this.itens.filter(i => i.item.idItem !== id); }
  limpar(): void { this.itens = []; }
  get total(): number { return this.itens.reduce((soma, i) => soma + i.quantidade * i.item.preco, 0); }
}

