import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AvisoService {
  mensagem = '';
  temErro = false;

  avisar(mensagem: string, erro = false): void {
    this.mensagem = mensagem;
    this.temErro = erro;
  }

  limpar(): void { this.mensagem = ''; }

  mostrarErro(erro: unknown): void {
    const resposta = erro as HttpErrorResponse;
    const texto = typeof resposta.error === 'string' ? resposta.error : resposta.error?.mensagem;
    this.avisar(texto || (resposta.status === 0 ? 'A API não respondeu. Verifique se está em execução.' :
      resposta.status === 404 ? 'Registro não encontrado.' : 'Não foi possível concluir a operação.'), true);
  }
}

