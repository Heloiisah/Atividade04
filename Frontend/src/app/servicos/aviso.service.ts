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

  limpar(): void {
    this.mensagem = '';
  }

  mostrarErro(erro: unknown): void {
    const resposta = erro as HttpErrorResponse;
    let mensagem = 'Não foi possível concluir a operação.';

    if (resposta.status === 0) {
      mensagem = 'A API não respondeu. Verifique se está em execução.';
    } else if (resposta.status === 404) {
      mensagem = 'Registro não encontrado.';
    }

    if (typeof resposta.error === 'string' && resposta.error) {
      mensagem = resposta.error;
    } else if (resposta.error && resposta.error.mensagem) {
      mensagem = resposta.error.mensagem;
    }

    this.avisar(mensagem, true);
  }
}
