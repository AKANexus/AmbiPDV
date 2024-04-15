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

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para ParamsAdministradora.xaml
    /// </summary>
    public partial class ParamsAdministradora : Window
    {
        public ObservableCollection<MaqCtaItem> macCtaAdmin = new();
        public List<(short id, string desc)> ContasBancarias;
        private readonly FbConnection ConnecionServ = new();

        public ParamsAdministradora()
        {
            InitializeComponent();
            DataContext = this;
            ConnecionServ.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
            PreencheDados();
        }



        private void but_Confirmar_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void but_Cancelar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void PreencheDados()
        {            
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
                                ConnecionServ;

                    taAdmins.Fill(dataTable: tblAdmins);                   
                    taBancos.Fill(dataTable: tblBancos);                    

                    if (tblAdmins.Rows.Count > 0 && tblBancos.Rows.Count > 0)
                    {
                        foreach (var admins in tblAdmins.AsEnumerable().Select(selector: x => (x.ID_ADMINISTRADORA, x.DESCRICAO)))
                        {
                            //TODO: Para cada maquininha cadastrada, verificar na TB_PARAMETRO se
                            //existe algum registro com a coluna "INFORMACAO" igual a MAQCTA_{ID_MAQUININHA} (interpolar nome com ID)
                            //A partir dai pegar o que estiver na coluna "CONTEUDO" que será a conta vinculada.
                            //Caso não retorne nada então a maquininha não tem vinculos.

                            ContasBancarias = tblBancos.Select(selector: x => (x.ID_CONTA, x.DESCRICAO)).ToList();

                            var idCta = taParametros.GetVinculoCtaMaq(ID_MAQ: $"MAQCTA_{admins.ID_ADMINISTRADORA}");

                            macCtaAdmin.Add(new MaqCtaItem(descMaquininha: admins.DESCRICAO,
                                                           idMaquininha: admins.ID_ADMINISTRADORA,
                                                           contas: ContasBancarias,
                                                           idCtaCurrent: idCta.Safeshort()));
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    public class MaqCtaItem : INotifyPropertyChanged
    {       
        public string DescricaoMaquininha { get; set; }
        public int IdMaquininha { get; set; }

        private (short idConta, string descricaoConta)? _ctaSelecionada;
        public (short idConta, string descricaoConta)? CtaSelecionada
        {
            get => _ctaSelecionada; 
            set
            {
                if(_ctaSelecionada == value) return;
                _ctaSelecionada = value;
                OnPropertyChanged(nameof(CtaSelecionada));
            }
        }
        public List<(short idConta, string descricaoConta)> ContasBancarias { get; set; }


        public MaqCtaItem(string descMaquininha, int idMaquininha, List<(short id, string desc)> contas, short idCtaCurrent)
        {
            DescricaoMaquininha = descMaquininha;
            IdMaquininha = idMaquininha;
            ContasBancarias = contas;
            
            CtaSelecionada = contas.FirstOrDefault(x => x.id == idCtaCurrent);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
