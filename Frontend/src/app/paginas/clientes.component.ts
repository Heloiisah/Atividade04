import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Funcionario, Pessoa } from '../modelos/modelos';

@Component({
  selector: 'app-clientes', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './clientes.component.html'
})
export class ClientesComponent implements OnInit {
  @Input() administracao = false;
  @Output() voltar = new EventEmitter<void>();
  consultaId = 0;
  pessoas: Pessoa[] = [];
  clienteForm = this.novoCliente();

  constructor(private api: ApiService, private aviso: AvisoService) {}

  ngOnInit(): void { if (this.administracao) void this.carregarClientes(); }

  async carregarClientes(): Promise<void> {
    try {
      const [pessoas, funcionarios] = await Promise.all([
        firstValueFrom(this.api.pessoas()), firstValueFrom(this.api.funcionarios())
      ]);
      this.pessoas = pessoas.filter(p => !funcionarios.some((f: Funcionario) => f.idPessoa === p.idPessoa));
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  editarCliente(p: Pessoa): void {
    this.clienteForm = { ...p, enderecos: p.enderecos.map(e => ({ ...e })) };
  }

  async consultarCliente(): Promise<void> {
    try { this.clienteForm = await firstValueFrom(this.api.pessoa(this.consultaId)); }
    catch (erro) { this.aviso.mostrarErro(erro); }
  }

  async salvarCliente(): Promise<void> {
    try {
      const novo = !this.clienteForm.idPessoa;
      const resposta = await firstValueFrom(this.api.salvarPessoa(this.clienteForm));
      const id = novo ? (resposta as Pessoa).idPessoa : this.clienteForm.idPessoa;
      this.clienteForm = this.novoCliente();
      if (this.administracao) await this.carregarClientes();
      this.aviso.avisar(`Cliente ${id} salvo. Anote o ID para consultar pedidos.`);
    } catch (erro) { this.aviso.mostrarErro(erro); }
  }

  novoCliente(): Pessoa {
    return { idPessoa: 0, nome: '', cpf: 0, dataNascimento: '', sexo: '', email: '',
      telefone: 0, ativoPessoa: 'S',
      enderecos: [{ idPessoa: 0, cep: '', numero: '', complemento: '' }] };
  }
}

