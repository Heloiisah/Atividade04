export interface Endereco {
  idEndereco?: number;
  idPessoa: number;
  cep: string;
  numero: string;
  complemento: string;
}

export interface Pessoa {
  idPessoa: number;
  nome: string;
  cpf: number;
  dataNascimento: string;
  sexo: string;
  email: string;
  telefone: number;
  ativoPessoa: string;
  enderecos: Endereco[];
}

export interface Funcionario {
  idPessoa: number;
  cargo: string;
  dataAdmissao: string;
  pessoa: Pessoa;
}

export interface Categoria {
  idcategoria: number;
  categoria: string;
}

export interface Produto {
  idItem: number;
  idcategoria: number;
  item: string;
  descricaoItem: string;
  precoItem: number;
  tipoItem: string;
  ativoProdutoServico: string;
  codigoPreco: number;
  precoVenda: number;
  unidade: string;
}

export interface Servico {
  idItem: number;
  idCategoria: number;
  item: string;
  descricaoItem: string;
  preco: number;
  tipoItem: string;
  ativoProdutoServico: string;
}

export interface CatalogoItem {
  idItem: number;
  idCategoria: number;
  categoria: string;
  item: string;
  descricaoItem: string;
  tipoItem: string;
  ativoProdutoServico: string;
  preco: number;
  unidade?: string;
  quantidadeEstoque?: number;
}

export interface Estoque {
  idItem: number;
  item: string;
  descricaoItem: string;
  preco: number;
  precoVenda: number;
  unidade: string;
  categoria: string;
  quantidadeEstoque: number;
  ativoProdutoServico: string;
}

export interface Entrada {
  idEntradaProduto: number;
  idItem: number;
  dataEntradaProduto: string;
  quantidadeEntrada: number;
  unidade: string;
  valorUnitarioEntrada: number;
  quantidadeEstoque?: number;
}

export interface PedidoItem {
  idAtendimento: number;
  idItem: number;
  idFuncionario: number;
  quantidadeItens: number;
  valorUnitarioEntrada: number;
  item: string;
  nomeFuncionario: string;
  cargo: string;
}

export interface Pedido {
  idAtendimento: number;
  idPessoa: number;
  dataAtendimento: string;
  statusAtendimento: string;
  nomePessoa: string;
  valorTotal: number;
  itens: PedidoItem[];
}

export interface Identificacao {
  idPessoa: number;
  cpf: number;
  telefone: number;
  email: string;
}

export interface CarrinhoItem {
  item: CatalogoItem;
  quantidade: number;
  idFuncionario: number;
}

export interface PedidoCompraItem {
  idItem: number;
  idFuncionario: number;
  quantidade: number;
}
