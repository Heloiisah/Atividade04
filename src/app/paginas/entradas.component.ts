import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Entrada, Estoque } from '../modelos/modelos';

@Component({
  selector: 'app-entradas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './entradas.component.html'
})
export class EntradasComponent implements OnInit {
  consultaId = 0;
  estoques: Estoque[] = [];
  entradas: Entrada[] = [];
  entradaDetalhe?: Entrada;
  entradaForm = this.novaEntrada();

  constructor(
    private api: ApiService,
    private aviso: AvisoService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      this.estoques = await firstValueFrom(this.api.estoque());
      this.entradas = await firstValueFrom(this.api.entradas());
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  async registrarEntrada(): Promise<void> {
    try {
      await firstValueFrom(this.api.registrarEntrada(this.entradaForm));
      this.entradaForm = this.novaEntrada();
      this.estoques = await firstValueFrom(this.api.estoque());
      this.entradas = await firstValueFrom(this.api.entradas());
      this.aviso.avisar('Entrada registrada e estoque atualizado.');
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  async consultarEntrada(): Promise<void> {
    try {
      this.entradaDetalhe = await firstValueFrom(this.api.entrada(this.consultaId));
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  novaEntrada(): Entrada {
    return {
      idEntradaProduto: 0,
      idItem: 0,
      dataEntradaProduto: new Date().toISOString().slice(0, 10),
      quantidadeEntrada: 1,
      unidade: 'UN',
      valorUnitarioEntrada: 0
    };
  }
}
