import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Categoria, Produto } from '../modelos/modelos';

@Component({
  selector: 'app-produtos', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './produtos.component.html'
})
export class ProdutosComponent implements OnInit {
  consultaId = 0;
  categorias: Categoria[] = [];
  produtos: Produto[] = [];
  produtoForm = this.novoProduto();

  constructor(private api: ApiService, private aviso: AvisoService) {}

  async ngOnInit(): Promise<void> {
    try {
      this.produtos = await firstValueFrom(this.api.produtos());
      this.categorias = await firstValueFrom(this.api.categorias());
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  editarProduto(p: Produto): void { this.produtoForm = { ...p }; }

  async consultarProduto(): Promise<void> {
    try { this.produtoForm = await firstValueFrom(this.api.produto(this.consultaId)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async salvarProduto(): Promise<void> {
    try {
      await firstValueFrom(this.api.salvarProduto(this.produtoForm));
      this.produtoForm = this.novoProduto();
      this.produtos = await firstValueFrom(this.api.produtos());
      this.aviso.avisar('Produto salvo.');
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async desativarProduto(id: number): Promise<void> {
    try {
      await firstValueFrom(this.api.desativarProduto(id));
      this.produtos = await firstValueFrom(this.api.produtos());
      this.aviso.avisar('Produto desativado.');
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  novoProduto(): Produto {
    return { idItem: 0, idcategoria: 0, item: '', descricaoItem: '', precoItem: 0,
      tipoItem: 'P', ativoProdutoServico: 'S', codigoPreco: 1, precoVenda: 0, unidade: 'UN' };
  }
}

