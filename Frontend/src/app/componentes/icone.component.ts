import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-icone',
  standalone: true,
  template: `
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.6"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path [attr.d]="caminhos[nome] || caminhos['inicio']" />
    </svg>
  `
})
export class IconeComponent {
  @Input() nome = 'inicio';
  readonly caminhos: Record<string, string> = {
    inicio: 'M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h7v7h-7z',
    loja: 'M3 10h18l-2-6H5z M4 10v10h16V10 M9 20v-6h6v6 M8 4l-1 6 M16 4l1 6',
    clientes:
      'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2 M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8 M17 4a4 4 0 0 1 0 8 M22 21v-2a4 4 0 0 0-3-3.87',
    funcionarios:
      'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2 M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8 M17 11l2 2 3-4',
    produtos: 'M12 3l9 5v9l-9 5-9-5V8z M3 8l9 5 9-5 M12 13v9 M7.5 5.5l9 5',
    servicos:
      'M12 3C7 7 7 12 12 16c5-4 5-9 0-13 M12 16C7 9 2 11 2 13c0 5 6 8 10 8 4 0 10-3 10-8 0-2-5-4-10 3',
    categorias: 'M3 5h18 M3 12h18 M3 19h18 M7 3v4 M17 10v4 M10 17v4',
    entradas: 'M4 7l8-4 8 4v13H4z M4 7l8 4 8-4 M12 11v6 M9 14l3 3 3-3',
    estoque: 'M3 21V8l9-5 9 5v13 M1 21h22 M7 21V11h10v10 M7 15h10 M7 18h10',
    pedidos: 'M8 5H5v16h14V5h-3 M9 3h6v4H9z M8 12h8 M8 16h5',
    carrinho:
      'M2 3h3l3 12h11l3-8H6 M9 21a1 1 0 1 0 0-2 1 1 0 0 0 0 2 M18 21a1 1 0 1 0 0-2 1 1 0 0 0 0 2',
    buscar: 'M21 21l-5-5 M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16',
    seta: 'M5 12h14 M14 7l5 5-5 5',
    voltar: 'M19 12H5 M10 7l-5 5 5 5',
    remover: 'M3 6h18 M9 6V3h6v3 M5 6l1 15h12l1-15 M10 10v7 M14 10v7',
    fechar: 'M6 6l12 12 M6 18L18 6',
    certo: 'M4 12l5 5L20 6',
    folha: 'M20 3C9 3 3 7 3 13a7 7 0 0 0 7 7c6 0 10-6 10-17 M3 21L15 9'
  };
}
