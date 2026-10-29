import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../servicos/api.service';
import { AvisoService } from '../servicos/aviso.service';
import { Funcionario, Pessoa } from '../modelos/modelos';

@Component({
  selector: 'app-funcionarios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './funcionarios.component.html'
})
export class FuncionariosComponent implements OnInit {
  consultaId = 0;
  funcionarios: Funcionario[] = [];
  funcionarioForm = this.novoFuncionario();

  constructor(
    private api: ApiService,
    private aviso: AvisoService
  ) {}

  ngOnInit(): void {
    void this.carregar();
  }

  async carregar(): Promise<void> {
    try {
      this.funcionarios = await firstValueFrom(this.api.funcionarios());
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  editarFuncionario(f: Funcionario): void {
    const pessoa = f.pessoa;
    this.funcionarioForm = {
      idPessoa: f.idPessoa,
      cargo: f.cargo,
      dataAdmissao: f.dataAdmissao,
      pessoa: {
        idPessoa: pessoa.idPessoa,
        nome: pessoa.nome,
        cpf: pessoa.cpf,
        dataNascimento: pessoa.dataNascimento,
        sexo: pessoa.sexo,
        email: pessoa.email,
        telefone: pessoa.telefone,
        ativoPessoa: pessoa.ativoPessoa,
        enderecos: []
      }
    };

    for (const endereco of pessoa.enderecos) {
      this.funcionarioForm.pessoa.enderecos.push({
        idEndereco: endereco.idEndereco,
        idPessoa: endereco.idPessoa,
        cep: endereco.cep,
        numero: endereco.numero,
        complemento: endereco.complemento
      });
    }
  }

  async consultarFuncionario(): Promise<void> {
    try {
      this.funcionarioForm = await firstValueFrom(this.api.funcionario(this.consultaId));
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  async salvarFuncionario(): Promise<void> {
    try {
      await firstValueFrom(this.api.salvarFuncionario(this.funcionarioForm));
      this.funcionarioForm = this.novoFuncionario();
      await this.carregar();
      this.aviso.avisar('Funcionário salvo.');
    } catch (erro) {
      this.aviso.mostrarErro(erro);
    }
  }

  novoFuncionario(): Funcionario {
    const pessoa: Pessoa = {
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
    return { idPessoa: 0, cargo: '', dataAdmissao: '', pessoa };
  }
}
