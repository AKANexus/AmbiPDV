using System;

namespace PDV_WPF.Objetos.Enums
{
    enum StateTEF : int
    {
        OperacaoPadrao, CancelamentoRequisitado, AguardaMenu, AguardaEnter, AguardaCampo, AguardaSenha, AguardaValor, AguardaSimNao, RetornaMenuAnterior
    }
    enum PagAssist : int
    {
        erro = -1, bemvindo, legal, selecao, confignada, configserial, configecf, configspooler, configsat, confirmacao, fim
    }

    [Flags]
    public enum Permissoes
    {
        Nenhum = 0,
        AbrirConfiguracoes = 1 << 0, //1

        ConsultaAvancada = 1 << 6, //64
        EfetuarDevolucao = 1 << 7, //128
        DescontoItem = 1 << 8, //256
        DescontoVenda = 1 << 9, //512
        EfetuarSangria = 1 << 10, //1024
        EfetuarLogoff = 1 << 11, //2048
        MinimizarPdv = 1 << 12, //4096
        FecharPdv = 1 << 13, //8192
        ReimprimirCupom = 1 << 14, //16384
        ReimprimirFechamento = 1 << 15, //32768
        ReimprimirSangria = 1 << 16, //65536
        VendaPrazo = 1 << 17, //131072
                             

        PermissaoTotal = ~Nenhum //262143
    }
}
