import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Pessoa } from '../modelos/modelos';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './clientes.component.html'
})
export class ClientesComponent implements OnInit {
  @Input() administracao = false;
  @Output() voltar = new EventEmitter<void>();
  consultaId = 0;
  pessoas: Pessoa[] = [];
  clienteForm = this.novoCliente();

  constructor(
    private api: ApiService,
    private aviso: AvisoService
  ) {}

  ngOnInit(): void {
    if (this.administracao) {
      this.carregarClientes();
    }
  }

  async carregarClientes(): Promise<void> {
    try {
      const pessoas = await firstValueFrom(this.api.pessoas());
      const funcionarios = await firstValueFrom(this.api.funcionarios());
      this.pessoas = [];

      for (const pessoa of pessoas) {
        let funcionarioEncontrado = false;

        for (const funcionario of funcionarios) {
          if (funcionario.idPessoa === pessoa.idPessoa) {
            funcionarioEncontrado = true;
            break;
          }
        }

        if (!funcionarioEncontrado) {
          this.pessoas.push(pessoa);
        }
      }
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  editarCliente(p: Pessoa): void {
    this.clienteForm = {
      idPessoa: p.idPessoa,
      nome: p.nome,
      cpf: p.cpf,
      dataNascimento: p.dataNascimento,
      sexo: p.sexo,
      email: p.email,
      telefone: p.telefone,
      ativoPessoa: p.ativoPessoa,
      enderecos: []
    };

    for (const endereco of p.enderecos) {
      this.clienteForm.enderecos.push({
        idEndereco: endereco.idEndereco,
        idPessoa: endereco.idPessoa,
        cep: endereco.cep,
        numero: endereco.numero,
        complemento: endereco.complemento
      });
    }
  }

  async consultarCliente(): Promise<void> {
    try {
      this.clienteForm = await firstValueFrom(this.api.pessoa(this.consultaId));
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  async salvarCliente(): Promise<void> {
    try {
      const novo = !this.clienteForm.idPessoa;
      const resposta = await firstValueFrom(this.api.salvarPessoa(this.clienteForm));
      let id = this.clienteForm.idPessoa;
      if (novo && resposta) {
        id = resposta.idPessoa;
      }
      this.clienteForm = this.novoCliente();
      if (this.administracao) {
        await this.carregarClientes();
      }
      this.aviso.avisar(`Cliente ${id} salvo. Anote o ID para consultar pedidos.`);
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  novoCliente(): Pessoa {
    return {
      idPessoa: 0,
      nome: '',
      cpf: 0,
      dataNascimento: '',
      sexo: '',
      email: '',
      telefone: 0,
      ativoPessoa: 'S',
      enderecos: [{ idPessoa: 0, cep: '', numero: '', complemento: '' }]
    };
  }
}
