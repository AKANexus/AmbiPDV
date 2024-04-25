using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using System;
using System.Web.UI.WebControls;
using System.Data;
using System.Linq;
using System.ComponentModel;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Funcoes;
using PDV_WPF.DataSets;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para ParamsAdministradora.xaml
    /// </summary>
    public partial class ParamsAdministradora : Window
    {
        public ObservableCollection<MaqCtaItem> macCtaAdmin { get; } = new();
        public List<MaqCtaListItem> ContasBancarias;
        private readonly FbConnection ConnecionServ = new();

        public ParamsAdministradora()
        {
            DataContext = this;
            InitializeComponent();
            ConnecionServ.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
            PreencheDados();
        }


        private void but_Confirmar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SalvarParametrosMaqCta())
                this.Close();

        }

        private void but_Cancelar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is ComboBox comboBox)
                comboBox.SelectedItem = null;
        }

        private void PreencheDados()
        {
            using (var taCliente = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter())
            using (var taParametros = new FDBDataSetTableAdapters.TB_PARAMETROTableAdapter())
            using (var tblBancos = new FDBDataSet.TB_BANCO_CTADataTable())
            using (var taBancos = new FDBDataSetTableAdapters.TB_BANCO_CTATableAdapter())
            using (var tblAdmins = new DataSets.FDBDataSetOperSeed.TB_CARTAO_ADMINISTRADORADataTable())
            using (var taAdmins = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CARTAO_ADMINISTRADORATableAdapter())
            {
                try
                {
                    macCtaAdmin.Clear();

                    taBancos.Connection =
                        taAdmins.Connection =
                            taParametros.Connection =
                                taCliente.Connection =
                                    ConnecionServ;

                    taAdmins.Fill(dataTable: tblAdmins);
                    taBancos.Fill(dataTable: tblBancos);

                    if (tblAdmins.Rows.Count > 0 && tblBancos.Rows.Count > 0)
                    {
                        ContasBancarias = tblBancos.Select(selector: x => new MaqCtaListItem(x.ID_CONTA, x.DESCRICAO)).ToList();

                        foreach (var admins in tblAdmins.AsEnumerable().Select(selector: x => (x.ID_ADMINISTRADORA, x.DESCRICAO)))
                        {
                            // Para cada maquininha cadastrada, verificar na TB_PARAMETRO se
                            // existe algum registro com a coluna "INFORMACAO" igual a MAQCTA_{ID_MAQUININHA} (interpolar nome com ID).                            
                            // A partir dai pegar o que estiver na coluna "CONTEUDO" que será a conta vinculada e dias para vencimento.
                            // Caso não retorne nada então a maquininha não tem vinculos.

                            FDBDataSet.TB_PARAMETRORow paramsConta = taParametros.GetParameter(CONFIG: $"MAQCTA_{admins.ID_ADMINISTRADORA}").FirstOrDefault();

                            macCtaAdmin.Add(new MaqCtaItem(descMaquininha: admins.DESCRICAO,
                                                           idMaquininha: admins.ID_ADMINISTRADORA,
                                                           contas: ContasBancarias,
                                                           idCtaCurrent: paramsConta?.CONTEUDO.Split('|') is string[] ctaCurrent && ctaCurrent.Length > 1 ? ctaCurrent[0].Safeshort() : default,
                                                           idParametro: paramsConta?.ID_PARAMETRO ?? default,
                                                           vencimento: paramsConta?.CONTEUDO.Split('|') is string[] vencto && vencto.Length > 1 ? vencto[1] : default));

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu erro ao preencher administradoras e contas bancárias.\n\nException: {ex.InnerException.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
        }

        private bool SalvarParametrosMaqCta()
        {
            using (var taParametros = new FDBDataSetTableAdapters.TB_PARAMETROTableAdapter())
            {
                taParametros.Connection = ConnecionServ;

                try
                {
                    foreach (var configs in macCtaAdmin)
                    {
                        if (configs.CtaSelecionada != null && configs.IdParametro != 0)
                        {
                            taParametros.UpdateOrInsert(ID_PARAMETRO: configs.IdParametro,
                                                        INFORMACAO: $"MAQCTA_{configs.IdMaquininha}",
                                                        CONTEUDO: $"{configs.CtaSelecionada.Id}|{configs.DiasVencimento}",
                                                        DESCRICAO: "Vinculo da maquininha administradora com alguma conta bancaria",
                                                        ID_FUNCIONARIO: 0);
                            continue;
                        }

                        if (configs.CtaSelecionada != null)
                        {
                            taParametros.InsertInto(INFORMACAO: $"MAQCTA_{configs.IdMaquininha}",
                                                    CONTEUDO: $"{configs.CtaSelecionada.Id}|{configs.DiasVencimento}",
                                                    DESCRICAO: "Vinculo da maquininha administradora com alguma conta bancaria",
                                                    ID_FUNCIONARIO: 0);
                            continue;
                        }

                        if (configs.CtaSelecionada == null & configs.IdParametro != 0)
                        {
                            taParametros.DeleteByIdParametro(ID_PARAMETRO: configs.IdParametro);
                        }
                    }

                    MessageBox.Show("Vinculo das maquininhas/administradoras alterado com sucesso!\n\n" +
                                    "Reinicie a aplicação para que as novas configurações sejam utilizadas no PDV.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar informações na base de dados.\n\nException: {ex.InnerException.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        Action<object, TextCompositionEventArgs> TextBox_PreviewTextInput = (sender, e) =>
        {
            if(!Regex.IsMatch(e.Text, @"^[0-9]+$"))
                e.Handled = true;
        };            
    }

    public class MaqCtaItem : INotifyPropertyChanged
    {
        private string _descricaoMaquininha;
        public string DescricaoMaquininha
        {
            get
            {
                return $"{_descricaoMaquininha}:";
            }
            set
            {
                if (_descricaoMaquininha == value) return;
                _descricaoMaquininha = value.Length == 32 ? $"{value}.." : value;
                OnPropertyChanged(nameof(DescricaoMaquininha));
            }
        }

        private MaqCtaListItem? _ctaSelecionada;
        public MaqCtaListItem? CtaSelecionada
        {
            get => _ctaSelecionada;
            set
            {
                if (_ctaSelecionada == value) return;
                _ctaSelecionada = value;
                OnPropertyChanged(nameof(CtaSelecionada));
            }
        }

        public List<MaqCtaListItem> ContasBancarias { get; set; }
        public int IdMaquininha { get; set; }
        public int IdParametro { get; set; }

        public int _diasVencimento;
        public string DiasVencimento 
        {
            get => _diasVencimento.ToString();
            set
            {
                if(_diasVencimento.ToString() == value) return;
                _diasVencimento = value.Safeint();
                OnPropertyChanged(nameof(DiasVencimento));
            } 
        }


        public MaqCtaItem(string descMaquininha, int idMaquininha, List<MaqCtaListItem> contas, short idCtaCurrent, int idParametro, string vencimento)
        {
            DescricaoMaquininha = descMaquininha.Trunca(32);
            IdMaquininha = idMaquininha;
            ContasBancarias = contas;
            CtaSelecionada = contas.FirstOrDefault(x => x.Id == idCtaCurrent);
            IdParametro = idParametro;
            DiasVencimento = vencimento;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MaqCtaListItem
    {
        public short Id { get; set; }
        public string DescricaoConta { get; set; }

        public MaqCtaListItem(short id, string descricaoConta)
        {
            Id = id;
            DescricaoConta = descricaoConta;
        }
    }
}
