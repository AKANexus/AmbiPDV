using System;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Abrir configurações do sistema.")] AbrirConfiguracoes = 1 << 0, //1
        [Display(Name = "Cancelar venda atual.")] CancelarVenda =  1 << 1, // 2
        [Display(Name = "Cancelar cupons finalizados.")] CancelarCupons = 1 << 2, // 4
        [Display(Name = "Fechar turno.")] FecharTurno = 1 << 3, // 8
        [Display(Name = "Estornar item do cupom.")] EstornarItem = 1 << 4, // 16
        [Display(Name = "Abrir gaveta de dinheiro")] AbrirGaveta = 1 << 5, // 32
        [Display(Name = "Consulta avançada.")] ConsultaAvancada = 1 << 6, // 64
        [Display(Name = "Efetuar devolução de item.")] EfetuarDevolucao = 1 << 7, // 128
        [Display(Name = "Desconto no item.")] DescontoItem = 1 << 8, // 256
        [Display(Name = "Desconto na venda.")] DescontoVenda = 1 << 9, // 512
        [Display(Name = "Efetuar sangria.")] EfetuarSangria = 1 << 10, // 1024
        [Display(Name = "Efetuar logoff.")] EfetuarLogoff = 1 << 11, // 2048
        [Display(Name = "Minimizar o PDV.")] MinimizarPdv = 1 << 12, // 4096
        [Display(Name = "Fechar o PDV.")] FecharPdv = 1 << 13, // 8192
        [Display(Name = "Reimprimir cupons finalizados.")] ReimprimirCupom = 1 << 14, // 16384
        [Display(Name = "Reimprimir fechamentos.")] ReimprimirFechamento = 1 << 15, // 32768
        [Display(Name = "Reimprimir sangrias.")] ReimprimirSangria = 1 << 16, // 65536
        [Display(Name = "Venda a prazo.")] VendaPrazo = 1 << 17, // 131072
                             

        PermissaoTotal = ~Nenhum //262143 or -1
    }
}
