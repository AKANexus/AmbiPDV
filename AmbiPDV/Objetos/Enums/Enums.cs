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
}
