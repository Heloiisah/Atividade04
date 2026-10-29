import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Categoria, Servico } from '../modelos/modelos';

@Component({
  selector: 'app-servicos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './servicos.component.html'
})
export class ServicosComponent implements OnInit {
  consultaId = 0;
  categorias: Categoria[] = [];
  servicos: Servico[] = [];
  servicoForm = this.novoServico();

  constructor(
    private api: ApiService,
    private aviso: AvisoService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      this.servicos = await firstValueFrom(this.api.servicos());
      this.categorias = await firstValueFrom(this.api.categorias());
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  editarServico(s: Servico): void {
    this.servicoForm = {
      idItem: s.idItem,
      idCategoria: s.idCategoria,
      item: s.item,
      descricaoItem: s.descricaoItem,
      preco: s.preco,
      tipoItem: s.tipoItem,
      ativoProdutoServico: s.ativoProdutoServico
    };
  }

  async consultarServico(): Promise<void> {
    try {
      this.servicoForm = await firstValueFrom(this.api.servico(this.consultaId));
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  async salvarServico(): Promise<void> {
    try {
      await firstValueFrom(this.api.salvarServico(this.servicoForm));
      this.servicoForm = this.novoServico();
      this.servicos = await firstValueFrom(this.api.servicos());
      this.aviso.avisar('Serviço salvo.');
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  async desativarServico(id: number): Promise<void> {
    try {
      await firstValueFrom(this.api.desativarServico(id));
      this.servicos = await firstValueFrom(this.api.servicos());
      this.aviso.avisar('Serviço desativado.');
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  novoServico(): Servico {
    return {
      idItem: 0,
      idCategoria: 0,
      item: '',
      descricaoItem: '',
      preco: 0,
      tipoItem: 'S',
      ativoProdutoServico: 'S'
    };
  }
}
