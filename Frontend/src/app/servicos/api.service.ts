import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CatalogoItem, Categoria, Entrada, Estoque, Funcionario, Identificacao, Pedido, Pessoa, Produto, Servico } from '../modelos/modelos';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  catalogo(nome = '', categoria = 0, tipo = '') {
    let params = new HttpParams().set('ativos', true);
    if (nome) params = params.set('nome', nome);
    if (categoria) params = params.set('categoria', categoria);
    if (tipo) params = params.set('tipo', tipo);
    return this.http.get<CatalogoItem[]>('/api/catalogo', { params });
  }
  catalogoItem(id: number) { return this.http.get<CatalogoItem>(`/api/catalogo/${id}`); }

  categorias() { return this.http.get<Categoria[]>('/api/categorium'); }
  categoria(id: number) { return this.http.get<Categoria>(`/api/categorium/${id}`); }
  salvarCategoria(c: Categoria): Observable<Categoria | void> { return c.idcategoria
    ? this.http.put<void>(`/api/categorium/${c.idcategoria}`, c)
    : this.http.post<Categoria>('/api/categorium', c); }
  excluirCategoria(id: number) { return this.http.delete<void>(`/api/categorium/${id}`); }

  pessoas() { return this.http.get<Pessoa[]>('/api/pessoaendereco'); }
  pessoa(id: number) { return this.http.get<Pessoa>(`/api/pessoaendereco/${id}`); }
  salvarPessoa(p: Pessoa): Observable<Pessoa | void> { return p.idPessoa
    ? this.http.put<void>(`/api/pessoaendereco/${p.idPessoa}`, p)
    : this.http.post<Pessoa>('/api/pessoaendereco', p); }

  funcionarios() { return this.http.get<Funcionario[]>('/api/funcionario'); }
  funcionario(id: number) { return this.http.get<Funcionario>(`/api/funcionario/${id}`); }
  salvarFuncionario(f: Funcionario): Observable<Funcionario | void> {
    const dados = { cargo: f.cargo, dataAdmissao: f.dataAdmissao, pessoa: f.pessoa };
    return f.idPessoa
      ? this.http.put<void>(`/api/funcionario/${f.idPessoa}`, dados)
      : this.http.post<Funcionario>('/api/funcionario', dados);
  }

  produtos() { return this.http.get<Produto[]>('/api/produtoitem'); }
  produto(id: number) { return this.http.get<Produto>(`/api/produtoitem/${id}`); }
  salvarProduto(p: Produto): Observable<Produto | void> { return p.idItem
    ? this.http.put<void>(`/api/produtoitem/${p.idItem}`, p)
    : this.http.post<Produto>('/api/produtoitem', p); }
  desativarProduto(id: number) { return this.http.put<void>(`/api/produtoitem/${id}/desativar`, {}); }

  servicos() { return this.http.get<Servico[]>('/api/itemservico'); }
  servico(id: number) { return this.http.get<Servico>(`/api/itemservico/${id}`); }
  salvarServico(s: Servico): Observable<Servico | void> { return s.idItem
    ? this.http.put<void>(`/api/itemservico/${s.idItem}`, s)
    : this.http.post<Servico>('/api/itemservico', s); }
  desativarServico(id: number) { return this.http.put<void>(`/api/itemservico/${id}/desativar`, {}); }

  estoque() { return this.http.get<Estoque[]>('/api/estoque'); }
  estoqueItem(id: number) { return this.http.get<Estoque>(`/api/estoque/${id}`); }
  entradas() { return this.http.get<Entrada[]>('/api/entradaproduto'); }
  entrada(id: number) { return this.http.get<Entrada>(`/api/entradaproduto/${id}`); }
  registrarEntrada(e: Entrada) { return this.http.post<Entrada>('/api/entradaproduto', e); }

  pedidos(status = '') { return this.http.get<Pedido[]>('/api/pedidos',
    { params: status ? { status } : {} }); }
  pedido(id: number) { return this.http.get<Pedido>(`/api/pedidos/${id}`); }
  meusPedidos(dados: Identificacao) { return this.http.post<Pedido[]>('/api/pedidos/meus', dados); }
  meuPedido(id: number, dados: Identificacao) {
    return this.http.post<Pedido>(`/api/pedidos/meu/${id}`, dados);
  }
  confirmar(dados: Identificacao, itens: { idItem: number; idFuncionario: number; quantidade: number }[]) {
    return this.http.post<Pedido>('/api/pedidos', { ...dados, itens });
  }
  status(id: number, statusAtendimento: string) {
    return this.http.put<void>(`/api/pedidos/${id}/status`, { statusAtendimento });
  }
}

