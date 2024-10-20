﻿using FirebirdSql.Data.FirebirdClient;
using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using PDV_WPF.Objetos.Enums;
using System.Reflection;
using PDV_WPF.Funcoes;
using PDV_WPF.Properties;
using System.Collections.ObjectModel;
using System.Data;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para perguntaSenha.xaml
    /// </summary>
    public partial class perguntaSenha : Window
    {
        #region Fields & Properties

        public bool? permiteescape { get; set; }
        public enum nivelDeAcesso { Nenhum, Funcionario, Gerente }
        public nivelDeAcesso NivelAcesso { get; set; }
        private string Acao { get; set; }
        private Permissoes? PermissaoAtual { get; set; } = null;        
        public ObservableCollection<string> Funcionarios { get; set; }          

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();        

        #endregion Fields & Properties

        #region (De)Constructor

        public perguntaSenha(string acao)
        {
            StartPasswordClass(acao);
            lbl_dica.Content = "DIGITE SUA SENHA PARA FECHAR ESTA NOTIFICAÇÃO";
            txb_Senha.Focus();
        }

        public perguntaSenha(string acao, Permissoes permissaoRequisitada)
        {            
            using(var taPdvUsers = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
            {
                taPdvUsers.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath);
                EnumerableRowCollection<string> users = taPdvUsers.GetUsersPdv().Select(selector: x => x.USERNAME);
                Funcionarios = new ObservableCollection<string>();
                foreach (var user in users)
                    Funcionarios.Add(user);
            }

            StartPasswordClass(acao);
            lbl_dica.Content = "DIGITE UMA SENHA GERENCIAL PARA CONTINUAR";
            panel_Usuarios.Visibility = Visibility.Visible;
            cbb_Usuario.Focus();
            PermissaoAtual = permissaoRequisitada;
        }

        #endregion (De)Constructor

        #region Events

        private void cbb_Usuario_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)        
                txb_Senha.Focus();
        }

        private void txb_Senha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Senha.Password.Length > 0)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    try
                    {
                        using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                        using var taUsersPdv = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();                        
                        using var md5Hash = MD5.Create();
                        taUsersPdv.Connection = LOCAL_FB_CONN;

                        if(cbb_Usuario.Text == "" || cbb_Usuario.Text == null)
                        {    
                            if(PermissaoAtual is null)
                            {
                                if(ChecaHash(txb_Senha.Password, (string)taUsersPdv.PegaHashPorUser(operador)) || taUsersPdv.ChecaSenhaSupervisor(GenerateHash(txb_Senha.Password)).Safeint() > 0)
                                {
                                    DialogResult = true;
                                    NivelAcesso = nivelDeAcesso.Funcionario;
                                    return;
                                }
                                DialogBox.Show(strings.ACESSO_RESTRITO,
                                               DialogBoxButtons.No, DialogBoxIcons.None, false,
                                               strings.SENHA_INVALIDA_NOTIFICAÇÃO);
                                txb_Senha.Password = String.Empty;
                                return;
                            }
                            if (taUsersPdv.ChecaSenhaSupervisor(GenerateHash(txb_Senha.Password)).Safeint() > 0)
                            {
                                RegisterAccessManage(LOCAL_FB_CONN, "SUPERVISOR", (int?)taUsersPdv.PegaIdPorUser(cbb_Usuario.Text) ?? 0);                                                             
                                return;
                            }
                            DialogBox.Show(strings.ACESSO_RESTRITO,
                                           DialogBoxButtons.No, DialogBoxIcons.None, false,
                                           strings.SENHA_INVALIDA_SUPER);
                            txb_Senha.Password = String.Empty;
                            return;
                        }

                        if(ChecaHash(txb_Senha.Password, (string)taUsersPdv.PegaHashPorUser(cbb_Usuario.Text)))
                        {
                            if (taUsersPdv.ChecaPriv(cbb_Usuario.Text).Safeint() > 0)
                            {
                                RegisterAccessManage(LOCAL_FB_CONN, "GERENTE", (int?)taUsersPdv.PegaIdPorUser(cbb_Usuario.Text) ?? 0);
                                return;
                            }

                            Permissoes permissoesUsuario = (Permissoes)taUsersPdv.GetPermissoes(USERNAME: cbb_Usuario.Text);
                            if((permissoesUsuario & PermissaoAtual) != 0)
                            {
                                RegisterAccessManage(LOCAL_FB_CONN, "USUARIO", (int?)taUsersPdv.PegaIdPorUser(cbb_Usuario.Text) ?? 0);
                                return;
                            }

                            DialogResult = true;
                            NivelAcesso = nivelDeAcesso.Funcionario;
                            return;
                        }
                        else
                        {
                            DialogBox.Show(strings.ACESSO_RESTRITO,
                                          DialogBoxButtons.No, DialogBoxIcons.None, false,
                                          strings.SENHA_INVALIDA_USER);
                            txb_Senha.Password = String.Empty;
                        }                        
                    }
                    catch (Exception ex)
                    {
                        logErroAntigo(RetornarMensagemErro(ex, true));
                        MessageBox.Show("Erro ao conferir senha de usuário. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                        Environment.Exit(0);;
                        return;
                    }
                });
            }
        }

        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && permiteescape != false)
            {
                DialogResult = false;
                NivelAcesso = nivelDeAcesso.Nenhum;
                Close();
            }
        }

        #endregion Events

        #region Methods

        private void StartPasswordClass(string acao)
        {
            InitializeComponent();            
            lbl_Acao.Text = acao.ToUpper();
            Acao = acao;
            DataContext = this;
        }

        private void RegisterAccessManage(FbConnection connection, string accessLevel, int id)
        {
            using var taFuncAuditoria = new DataSets.FDBDataSetOperSeedTableAdapters.TB_FUNC_AUDITORIA_SISTableAdapter();
            taFuncAuditoria.Connection = connection;

            string message = accessLevel switch
            {
                "SUPERVISOR" => $"Senha de Supervisor usada em ({Acao}) no caixa {NO_CAIXA} operado por {operador}.",
                "GERENTE" => $"Senha de gerencia do usuário {cbb_Usuario.Text} usada em ({Acao}) no caixa {NO_CAIXA} operado por {operador}.",
                "USUARIO" => $"Senha do usuário {cbb_Usuario.Text} usada em ({Acao}) no caixa {NO_CAIXA} operado por {operador}.",
                _ => "Nivel de hierarquia não esperado."
            };

            NivelAcesso = nivelDeAcesso.Gerente;
            taFuncAuditoria.InsertAudit(ID_FUNCIONARIO: id,
                                        DATA: DateTime.Now,
                                        HORA: DateTime.Now,
                                        DESCRICAO: message,
                                        ID_AUDTIPO: 30,
                                        BUILD: Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                                        SYNCED: 0);
            NivelAcesso = nivelDeAcesso.Gerente;
            DialogResult = true;
        }
            
        #endregion Methods                
    }
}
