import { Injectable } from '@angular/core';
import { CarrinhoItem, CatalogoItem } from '../modelos/modelos';

@Injectable({ providedIn: 'root' })
export class CarrinhoService {
  itens: CarrinhoItem[] = [];

  adicionar(item: CatalogoItem): void {
    for (const linha of this.itens) {
      if (linha.item.idItem === item.idItem) {
        linha.quantidade++;
        return;
      }
    }

    this.itens.push({ item: item, quantidade: 1, idFuncionario: 0 });
  }

  quantidade(id: number): number {
    for (const linha of this.itens) {
      if (linha.item.idItem === id) {
        return linha.quantidade;
      }
    }

    return 0;
  }

  remover(id: number): void {
    const itensRestantes: CarrinhoItem[] = [];

    for (const linha of this.itens) {
      if (linha.item.idItem !== id) {
        itensRestantes.push(linha);
      }
    }

    this.itens = itensRestantes;
  }

  limpar(): void {
    this.itens = [];
  }

  get total(): number {
    let total = 0;

    for (const linha of this.itens) {
      total += linha.quantidade * linha.item.preco;
    }

    return total;
  }
}
