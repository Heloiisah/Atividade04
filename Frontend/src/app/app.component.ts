import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { CarrinhoService } from './servicos/carrinho.service';
import { AvisoService } from './servicos/aviso.service';
import { Identificacao } from './modelos/modelos';
import { CatalogoComponent } from './paginas/catalogo.component';
import { ClientesComponent } from './paginas/clientes.component';
import { CarrinhoComponent } from './paginas/carrinho.component';
import { PedidosComponent } from './paginas/pedidos.component';
import { PainelComponent } from './paginas/painel.component';
import { FuncionariosComponent } from './paginas/funcionarios.component';
import { ProdutosComponent } from './paginas/produtos.component';
import { ServicosComponent } from './paginas/servicos.component';
import { CategoriasComponent } from './paginas/categorias.component';
import { EstoqueComponent } from './paginas/estoque.component';
import { EntradasComponent } from './paginas/entradas.component';

type Tela = 'catalogo' | 'cliente' | 'carrinho' | 'meus' | 'painel' | 'clientes' |
  'funcionarios' | 'produtos' | 'servicos' | 'categorias' | 'estoque' | 'entradas' | 'pedidos';

@Component({
  selector: 'app-root', standalone: true,
  imports: [CommonModule, CatalogoComponent, ClientesComponent, CarrinhoComponent,
    PedidosComponent, PainelComponent, FuncionariosComponent, ProdutosComponent,
    ServicosComponent, CategoriasComponent, EstoqueComponent, EntradasComponent],
  templateUrl: './app.component.html'
})
export class AppComponent {
  tela: Tela = 'catalogo';
  identidade: Identificacao = { idPessoa: 0, cpf: 0, telefone: 0, email: '' };

  constructor(public carrinho: CarrinhoService, public aviso: AvisoService) {}

  abrir(tela: Tela): void {
    this.tela = tela;
    this.aviso.limpar();
  }
}

