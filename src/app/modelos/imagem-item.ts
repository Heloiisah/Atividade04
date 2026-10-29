import { CatalogoItem } from './modelos';

export function imagemItem(item: CatalogoItem): string {
  const categoria = item.categoria;

  if (item.tipoItem === 'P') {
    if (
      categoria === 'Maquiagem' ||
      categoria === 'Extensão de Cílios' ||
      categoria === 'Design de Sobrancelhas' ||
      categoria === 'Micropigmentação'
    ) {
      return 'assets/imagens/maquiagem.jpg';
    }

    if (
      categoria === 'Tratamentos Corporais' ||
      categoria === 'Produtos Corporais' ||
      categoria === 'Produtos Capilares' ||
      categoria === 'Cuidados Capilares' ||
      categoria === 'Depilação'
    ) {
      return 'assets/imagens/cuidados-corporais.jpg';
    }

    return 'assets/imagens/cosmeticos.jpg';
  }

  if (
    categoria === 'Massagens' ||
    categoria === 'Tratamentos Corporais' ||
    categoria === 'Produtos Corporais' ||
    categoria === 'Drenagem Linfática'
  ) {
    return 'assets/imagens/massagem.jpg';
  }

  return 'assets/imagens/cuidados-faciais.jpg';
}
