import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Estoque } from '../modelos/modelos';

@Component({
  selector: 'app-estoque', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './estoque.component.html'
})
export class EstoqueComponent implements OnInit {
  consultaId = 0;
  estoques: Estoque[] = [];
  estoqueDetalhe?: Estoque;

  constructor(private api: ApiService, private aviso: AvisoService) {}

  async ngOnInit(): Promise<void> {
    try { this.estoques = await firstValueFrom(this.api.estoque()); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async consultarEstoque(): Promise<void> {
    try { this.estoqueDetalhe = await firstValueFrom(this.api.estoqueItem(this.consultaId)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }
}

