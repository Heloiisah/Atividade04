USE bd_aula_backend;

CREATE OR REPLACE VIEW vw_clientes_enderecos AS
SELECT
  p.idpessoa,
  p.nome,
  p.cpf,
  p.email,
  p.telefone,
  e.idendereco,
  e.cep,
  e.numero,
  e.complemento
FROM
  pessoa p
  INNER JOIN endereco e ON e.idpessoa = p.idpessoa
  LEFT JOIN funcionario f ON f.idpessoa = p.idpessoa
WHERE
  f.idpessoa IS NULL;

CREATE OR REPLACE VIEW vw_funcionarios_enderecos AS
SELECT
  p.idpessoa,
  p.nome,
  p.cpf,
  f.cargo,
  f.data_admissao,
  e.idendereco,
  e.cep,
  e.numero,
  e.complemento
FROM
  funcionario f
  INNER JOIN pessoa p ON p.idpessoa = f.idpessoa
  INNER JOIN endereco e ON e.idpessoa = p.idpessoa;

CREATE OR REPLACE VIEW vw_produtos_categorias_estoque AS
SELECT
  i.iditem,
  i.item,
  i.descricao_item,
  i.idcategoria,
  c.categoria,
  p.codigo_preco,
  p.preco_venda,
  p.unidade,
  e.quantidade_estoque
FROM
  item i
  INNER JOIN categoria c ON c.idcategoria = i.idcategoria
  INNER JOIN produto p ON p.iditem = i.iditem
  INNER JOIN estoque e ON e.iditem = p.iditem;

CREATE OR REPLACE VIEW vw_servicos_categorias AS
SELECT
  i.iditem,
  i.item,
  i.descricao_item,
  i.preco,
  i.idcategoria,
  c.categoria
FROM
  item i
  INNER JOIN categoria c ON c.idcategoria = i.idcategoria
WHERE
  i.tipo_item = 'S';

CREATE OR REPLACE VIEW vw_atendimentos AS
SELECT
  a.idatendimento,
  a.data_atendimento,
  a.status_atendimento,
  a.idpessoa,
  cliente.nome AS cliente,
  ai.iditem,
  i.item,
  ai.quantidade_itens,
  ai.valorunitario_entrada,
  ai.idfuncionario,
  pessoa_funcionario.nome AS funcionario
FROM
  atendimento a
  INNER JOIN pessoa cliente ON cliente.idpessoa = a.idpessoa
  INNER JOIN atendimentoitem ai ON ai.idatendimento = a.idatendimento
  INNER JOIN item i ON i.iditem = ai.iditem
  INNER JOIN funcionario f ON f.idpessoa = ai.idfuncionario
  INNER JOIN pessoa pessoa_funcionario ON pessoa_funcionario.idpessoa = f.idpessoa;
