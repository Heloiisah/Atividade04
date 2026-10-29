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
import { IconeComponent } from './componentes/icone.component';

type Tela =
  | 'catalogo'
  | 'cliente'
  | 'carrinho'
  | 'meus'
  | 'painel'
  | 'clientes'
  | 'funcionarios'
  | 'produtos'
  | 'servicos'
  | 'categorias'
  | 'estoque'
  | 'entradas'
  | 'pedidos';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    CatalogoComponent,
    ClientesComponent,
    CarrinhoComponent,
    PedidosComponent,
    PainelComponent,
    FuncionariosComponent,
    ProdutosComponent,
    ServicosComponent,
    CategoriasComponent,
    EstoqueComponent,
    EntradasComponent,
    IconeComponent
  ],
  templateUrl: './app.component.html'
})
export class AppComponent {
  tela: Tela = 'catalogo';
  identidade: Identificacao = { idPessoa: 0, cpf: 0, telefone: 0, email: '' };
  readonly menuAdmin: { tela: Tela; nome: string; descricao: string; icone: string }[] = [
    { tela: 'painel', nome: 'Início', descricao: 'Resumo do sistema', icone: 'inicio' },
    {
      tela: 'clientes',
      nome: 'Clientes',
      descricao: 'Consultar e cadastrar clientes',
      icone: 'clientes'
    },
    {
      tela: 'funcionarios',
      nome: 'Funcionários',
      descricao: 'Gerenciar a equipe',
      icone: 'funcionarios'
    },
    {
      tela: 'categorias',
      nome: 'Categorias',
      descricao: 'Organizar produtos e serviços',
      icone: 'categorias'
    },
    {
      tela: 'produtos',
      nome: 'Produtos',
      descricao: 'Gerenciar os produtos da loja',
      icone: 'produtos'
    },
    {
      tela: 'servicos',
      nome: 'Serviços',
      descricao: 'Gerenciar os tratamentos',
      icone: 'servicos'
    },
    {
      tela: 'pedidos',
      nome: 'Pedidos',
      descricao: 'Consultar e atualizar pedidos',
      icone: 'pedidos'
    },
    {
      tela: 'entradas',
      nome: 'Entrada de produtos',
      descricao: 'Registrar mercadorias recebidas',
      icone: 'entradas'
    },
    {
      tela: 'estoque',
      nome: 'Estoque',
      descricao: 'Consultar produtos disponíveis',
      icone: 'estoque'
    }
  ];

  get emAdministracao(): boolean {
    for (const item of this.menuAdmin) {
      if (item.tela === this.tela) {
        return true;
      }
    }

    return false;
  }

  constructor(
    public carrinho: CarrinhoService,
    public aviso: AvisoService
  ) {}

  abrir(tela: Tela): void {
    this.tela = tela;
    this.aviso.limpar();
  }
}
