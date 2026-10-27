import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Categoria } from '../modelos/modelos';

@Component({
  selector: 'app-categorias', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './categorias.component.html'
})
export class CategoriasComponent implements OnInit {
  consultaId = 0;
  categorias: Categoria[] = [];
  categoriaForm: Categoria = { idcategoria: 0, categoria: '' };

  constructor(private api: ApiService, private aviso: AvisoService) {}

  ngOnInit(): void { void this.carregar(); }

  async carregar(): Promise<void> {
    try { this.categorias = await firstValueFrom(this.api.categorias()); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  editarCategoria(c: Categoria): void { this.categoriaForm = { ...c }; }

  async consultarCategoria(): Promise<void> {
    try { this.categoriaForm = await firstValueFrom(this.api.categoria(this.consultaId)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async salvarCategoria(): Promise<void> {
    try {
      await firstValueFrom(this.api.salvarCategoria(this.categoriaForm));
      this.categoriaForm = { idcategoria: 0, categoria: '' };
      await this.carregar();
      this.aviso.avisar('Categoria salva.');
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async excluirCategoria(id: number): Promise<void> {
    try {
      await firstValueFrom(this.api.excluirCategoria(id));
      await this.carregar();
      this.aviso.avisar('Categoria excluída.');
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }
}

